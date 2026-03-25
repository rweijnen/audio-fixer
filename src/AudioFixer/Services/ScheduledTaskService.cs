using System.Runtime.InteropServices;
using AudioFixer.Native;

namespace AudioFixer.Services;

public static class ScheduledTaskService
{
    private const string TaskName = "AudioFixer";
    private const string TaskFolder = "\\";
    private const string TaskDescription =
        "Detects CS35L56 amp firmware download failures (Code 43) after resume " +
        "from standby and offers to fix by cycling the Intel SST OED device.";

    // Simplified subscription: Provider + EventID only.
    // The 3556 device filtering is done in our app code after launch.
    private const string EventSubscription =
        "<QueryList>" +
        "<Query Id='0' Path='System'>" +
        "<Select Path='System'>" +
        "*[System[Provider[@Name='CirrusLogic-Drv-XuCsMe'] and EventID=101]]" +
        "</Select>" +
        "</Query>" +
        "</QueryList>";

    private const string TriggerDelay = "PT10S";

    private static LogService? _log;

    public static void SetLogger(LogService log) => _log = log;

    private static void Log(string message) => _log?.Log(message);
    private static void LogError(string message) => _log?.LogError(message);

    public static bool IsInstalled()
    {
        ITaskService? scheduler = null;
        try
        {
            scheduler = CreateTaskService();
            var folder = scheduler.GetFolder(TaskFolder);
            folder.GetTask(TaskName);
            Log("TASK     IsInstalled: task found");
            return true;
        }
        catch (FileNotFoundException)
        {
            Log("TASK     IsInstalled: task not found");
            return false;
        }
        catch (COMException ex)
        {
            LogError($"IsInstalled COM error: 0x{ex.HResult:X8} {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            LogError($"IsInstalled unexpected error: {ex.GetType().Name} {ex.Message}");
            return false;
        }
    }

    public static bool Install()
    {
        string exePath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Could not determine executable path.");

        Log($"TASK     Install: exe={exePath}");

        ITaskService? scheduler = null;
        try
        {
            scheduler = CreateTaskService();
        }
        catch (Exception ex)
        {
            LogError($"Install: CreateTaskService failed: {ex.GetType().Name} {ex.Message}");
            throw;
        }

        ITaskDefinition definition;
        try
        {
            definition = scheduler.NewTask(0);
        }
        catch (Exception ex)
        {
            LogError($"Install: NewTask failed: {ex.GetType().Name} {ex.Message}");
            throw;
        }

        try
        {
            definition.get_RegistrationInfo().put_Description(TaskDescription);
            Log("TASK     Install: set description");
        }
        catch (Exception ex)
        {
            LogError($"Install: put_Description failed: {ex.GetType().Name} {ex.Message}");
            throw;
        }

        try
        {
            var principal = definition.get_Principal();
            principal.put_LogonType(TaskConstants.TASK_LOGON_INTERACTIVE_TOKEN);
            principal.put_RunLevel(TaskConstants.TASK_RUNLEVEL_HIGHEST);
            Log("TASK     Install: set principal (interactive, highest)");
        }
        catch (Exception ex)
        {
            LogError($"Install: Principal setup failed: {ex.GetType().Name} {ex.Message}");
            throw;
        }

        try
        {
            var settings = definition.get_Settings();
            settings.put_MultipleInstances(TaskConstants.TASK_INSTANCES_IGNORE_NEW);
            settings.put_DisallowStartIfOnBatteries(false);
            settings.put_StopIfGoingOnBatteries(false);
            settings.put_AllowHardTerminate(true);
            settings.put_StartWhenAvailable(false);
            settings.put_RunOnlyIfNetworkAvailable(false);
            settings.put_AllowDemandStart(true);
            settings.put_Enabled(true);
            settings.put_Hidden(false);
            settings.put_RunOnlyIfIdle(false);
            settings.put_ExecutionTimeLimit("PT5M");
            settings.put_Priority(7);
            Log("TASK     Install: configured settings");
        }
        catch (Exception ex)
        {
            LogError($"Install: Settings setup failed: {ex.GetType().Name} {ex.Message}");
            throw;
        }

        try
        {
            var triggerObj = definition.get_Triggers().Create(TaskConstants.TASK_TRIGGER_EVENT);
            var trigger = (IEventTrigger)triggerObj;
            trigger.put_Subscription(EventSubscription);
            trigger.put_Delay(TriggerDelay);
            trigger.put_Enabled(true);
            Log("TASK     Install: created event trigger");
        }
        catch (InvalidCastException ex)
        {
            LogError($"Install: ITrigger->IEventTrigger cast failed: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            LogError($"Install: Trigger setup failed: {ex.GetType().Name} 0x{(ex as COMException)?.HResult:X8} {ex.Message}");
            throw;
        }

        try
        {
            var actionObj = definition.get_Actions().Create(TaskConstants.TASK_ACTION_EXEC);
            var action = (IExecAction)actionObj;
            action.put_Path(exePath);
            Log("TASK     Install: created exec action");
        }
        catch (InvalidCastException ex)
        {
            LogError($"Install: IAction->IExecAction cast failed: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            LogError($"Install: Action setup failed: {ex.GetType().Name} {ex.Message}");
            throw;
        }

        try
        {
            var folder = scheduler.GetFolder(TaskFolder);
            folder.RegisterTaskDefinition(
                TaskName,
                definition,
                TaskConstants.TASK_CREATE_OR_UPDATE,
                null, null,
                TaskConstants.TASK_LOGON_INTERACTIVE_TOKEN,
                null);
            Log("TASK     Install: RegisterTaskDefinition succeeded");
        }
        catch (Exception ex)
        {
            LogError($"Install: RegisterTaskDefinition failed: {ex.GetType().Name} 0x{(ex as COMException)?.HResult:X8} {ex.Message}");
            throw;
        }

        return true;
    }

    public static bool Uninstall()
    {
        try
        {
            var scheduler = CreateTaskService();
            var folder = scheduler.GetFolder(TaskFolder);
            folder.DeleteTask(TaskName, 0);
            Log("TASK     Uninstall: task deleted");
            return true;
        }
        catch (Exception ex)
        {
            LogError($"Uninstall failed: {ex.GetType().Name} {ex.Message}");
            throw;
        }
    }

    private static ITaskService CreateTaskService()
    {
        var obj = Activator.CreateInstance(typeof(TaskSchedulerClass));
        if (obj == null)
            throw new InvalidOperationException("Failed to create TaskScheduler COM instance");

        var scheduler = (ITaskService)obj;
        scheduler.Connect(null, null, null, null);
        Log("TASK     Connected to Task Scheduler");
        return scheduler;
    }
}
