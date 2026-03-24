using System.Security.Principal;
using Microsoft.Toolkit.Uwp.Notifications;

namespace AudioFixer;

internal static class Program
{
    private static AudioFixerContext? _context;

    [STAThread]
    static void Main()
    {
        // Single-instance guard
        using var mutex = new Mutex(true, @"Global\AudioFixer_SingleInstance", out bool createdNew);
        if (!createdNew)
            return;

        // Verify admin rights (manifest should ensure this, but be explicit)
        if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
        {
            MessageBox.Show(
                "Audio Fixer requires administrator privileges to disable/enable audio devices.",
                "Audio Fixer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        // Register toast activation handler before Application.Run
        ToastNotificationManagerCompat.OnActivated += OnToastActivated;

        _context = new AudioFixerContext();
        Application.Run(_context);

        ToastNotificationManagerCompat.Uninstall();
    }

    private static void OnToastActivated(ToastNotificationActivatedEventArgsCompat e)
    {
        var args = ToastArguments.Parse(e.Argument);
        if (args.TryGetValue("action", out string? action) && action == "fix")
        {
            // Marshal to UI thread
            if (_context != null)
            {
                _ = _context.HandleToastFixAction();
            }
        }
    }
}
