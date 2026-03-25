using Microsoft.Toolkit.Uwp.Notifications;

namespace AudioFixer.Notifications;

public sealed class ToastService
{
    public void ShowProblemDetected(int affectedCount, int totalCount)
    {
        new ToastContentBuilder()
            .SetToastScenario(ToastScenario.Reminder)
            .AddText("Audio Device Problem Detected")
            .AddText($"{affectedCount} of {totalCount} CS35L56 amp device(s) have Code 43 errors.")
            .AddText("Restart audio services to fix?")
            .AddButton(new ToastButton()
                .SetContent("Fix Now")
                .AddArgument("action", "fix"))
            .AddButton(new ToastButton()
                .SetContent("Dismiss")
                .AddArgument("action", "dismiss"))
            .Show();
    }

    public void ShowAllOk(int totalCount)
    {
        new ToastContentBuilder()
            .SetToastScenario(ToastScenario.Reminder)
            .AddText("Audio Devices OK")
            .AddText($"All {totalCount} CS35L56 amp device(s) are working correctly.")
            .AddText("Restart audio services anyway?")
            .AddButton(new ToastButton()
                .SetContent("Restart Anyway")
                .AddArgument("action", "fix"))
            .AddButton(new ToastButton()
                .SetContent("Dismiss")
                .AddArgument("action", "dismiss"))
            .Show();
    }

    public void ShowTaskNotRegistered()
    {
        new ToastContentBuilder()
            .SetToastScenario(ToastScenario.Reminder)
            .AddText("Audio Fixer - Setup Required")
            .AddText("Scheduled task is not registered. Register now to automatically detect and fix CS35L56 audio errors after resume from standby?")
            .AddButton(new ToastButton()
                .SetContent("Register")
                .AddArgument("action", "install"))
            .AddButton(new ToastButton()
                .SetContent("Not Now")
                .AddArgument("action", "dismiss"))
            .Show();
    }

    public void ShowTaskRegistered()
    {
        new ToastContentBuilder()
            .SetToastScenario(ToastScenario.Reminder)
            .AddText("Audio Fixer - Registered")
            .AddText("Scheduled task registered. Audio Fixer will now automatically run when a CS35L56 firmware download failure is detected.")
            .AddButton(new ToastButton()
                .SetContent("OK")
                .AddArgument("action", "dismiss"))
            .Show();
    }

    public void ShowTaskRegistrationFailed(string errorMessage)
    {
        new ToastContentBuilder()
            .SetToastScenario(ToastScenario.Reminder)
            .AddText("Audio Fixer - Registration Failed")
            .AddText(errorMessage)
            .AddButton(new ToastButton()
                .SetContent("OK")
                .AddArgument("action", "dismiss"))
            .Show();
    }

    public void ShowFixSuccess()
    {
        new ToastContentBuilder()
            .AddText("Audio Devices Fixed")
            .AddText("All CS35L56 amp devices are now working correctly.")
            .Show();
    }

    public void ShowFixFailed(string errorMessage)
    {
        new ToastContentBuilder()
            .SetToastScenario(ToastScenario.Reminder)
            .AddText("Audio Fix Failed")
            .AddText(errorMessage)
            .Show();
    }
}
