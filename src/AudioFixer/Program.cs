using System.Security.Principal;
using AudioFixer.Notifications;
using AudioFixer.Services;
using Microsoft.Toolkit.Uwp.Notifications;

namespace AudioFixer;

internal static class Program
{
    private static readonly DeviceService DeviceService = new();
    private static readonly ToastService ToastService = new();
    private static LogService _log = null!;
    private static SynchronizationContext? _syncContext;
    private static bool _silent;

    [STAThread]
    static int Main(string[] args)
    {
        _log = new LogService();
        ScheduledTaskService.SetLogger(_log);
        _log.LogInvocation(args);

        _silent = args.Any(a => a.Equals("--silent", StringComparison.OrdinalIgnoreCase) ||
                                a.Equals("/silent", StringComparison.OrdinalIgnoreCase));

        bool install = args.Any(a => a.Equals("--install", StringComparison.OrdinalIgnoreCase) ||
                                     a.Equals("/install", StringComparison.OrdinalIgnoreCase));

        bool uninstall = args.Any(a => a.Equals("--uninstall", StringComparison.OrdinalIgnoreCase) ||
                                       a.Equals("/uninstall", StringComparison.OrdinalIgnoreCase));

        // Single-instance guard (not for install/uninstall commands)
        Mutex? mutex = null;
        if (!install && !uninstall)
        {
            mutex = new Mutex(true, @"Global\AudioFixer_SingleInstance", out bool createdNew);
            if (!createdNew)
            {
                _log.Log("EXIT     Another instance is already running");
                _log.Dispose();
                return 0;
            }
        }

        try
        {
            return Run(install, uninstall);
        }
        catch (Exception ex)
        {
            _log.LogError($"Unhandled exception: {ex}");
            return 1;
        }
        finally
        {
            mutex?.Dispose();
            _log.Dispose();
        }
    }

    private static int Run(bool install, bool uninstall)
    {
        // Verify admin rights
        if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
        {
            _log.LogError("Not running as administrator");
            return 1;
        }

        // Handle explicit install/uninstall
        if (install)
        {
            bool ok = ScheduledTaskService.Install();
            _log.LogTaskRegistration(ok);
            return ok ? 0 : 1;
        }

        if (uninstall)
        {
            bool ok = ScheduledTaskService.Uninstall();
            _log.LogTaskRegistration(!ok);
            return ok ? 0 : 1;
        }

        // All interactive paths need the message pump for toast callbacks
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        ToastNotificationManagerCompat.OnActivated += OnToastActivated;

        // Install the WinForms sync context before Application.Run so toast
        // callbacks can marshal to the UI thread immediately.
        WindowsFormsSynchronizationContext.AutoInstall = true;
        SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
        _syncContext = SynchronizationContext.Current;

        var context = new ApplicationContext();

        // Check if scheduled task is registered
        if (!ScheduledTaskService.IsInstalled())
        {
            _log.Log("TASK     Scheduled task not registered, prompting user");
            try
            {
                ToastService.ShowTaskNotRegistered();
                _log.Log("TOAST    ShowTaskNotRegistered succeeded");
            }
            catch (Exception ex)
            {
                _log.LogError($"Toast failed: {ex}");
                return 1;
            }
            Application.Run(context);
            return 0;
        }

        // Check device status
        var (hasProblem, affectedCount, totalCount) = DeviceService.GetProblemSummary();
        _log.LogDeviceStatus(hasProblem, affectedCount, totalCount);

        if (_silent)
        {
            if (!hasProblem)
                return 0;

            _log.LogUserResponse("Silent mode - fixing automatically");
            return FixAndReport();
        }

        // Interactive: show appropriate toast
        if (hasProblem)
        {
            ToastService.ShowProblemDetected(affectedCount, totalCount);
            _log.Log("TOAST    Problem notification shown, waiting for user response");
        }
        else
        {
            ToastService.ShowAllOk(totalCount);
            _log.Log("TOAST    All OK notification shown, waiting for user response");
        }

        Application.Run(context);
        return 0;
    }

    private static void OnToastActivated(ToastNotificationActivatedEventArgsCompat e)
    {
        var toastArgs = ToastArguments.Parse(e.Argument);
        toastArgs.TryGetValue("action", out string? action);

        switch (action)
        {
            case "fix":
                _log.LogUserResponse("Fix/restart confirmed via toast");
                _syncContext?.Post(_ =>
                {
                    FixAndReport();
                    Application.Exit();
                }, null);
                break;

            case "install":
                _log.LogUserResponse("Task registration confirmed via toast");
                _syncContext?.Post(_ =>
                {
                    try
                    {
                        bool ok = ScheduledTaskService.Install();
                        _log.LogTaskRegistration(ok);
                        ToastService.ShowTaskRegistered();
                    }
                    catch (Exception ex)
                    {
                        _log.LogError($"Task registration failed: {ex.Message}");
                        ToastService.ShowTaskRegistrationFailed(ex.Message);
                    }
                    Application.Exit();
                }, null);
                break;

            default:
                _log.LogUserResponse("Dismissed via toast");
                _syncContext?.Post(_ => Application.Exit(), null);
                break;
        }
    }

    private static int FixAndReport()
    {
        try
        {
            DeviceService.FixAudioDevicesAsync().GetAwaiter().GetResult();
            _log.LogFixResult(true);

            if (!_silent)
                ToastService.ShowFixSuccess();

            return 0;
        }
        catch (Exception ex)
        {
            _log.LogFixResult(false, ex.Message);

            if (!_silent)
                ToastService.ShowFixFailed(ex.Message);

            return 1;
        }
    }
}
