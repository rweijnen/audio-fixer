using Microsoft.Toolkit.Uwp.Notifications;

namespace AudioFixer.Notifications;

public sealed class ToastService
{
    public void ShowProblemDetected(int affectedCount, int totalCount)
    {
        new ToastContentBuilder()
            .AddText("Audio Device Problem Detected")
            .AddText($"{affectedCount} of {totalCount} CS35L56 amp device(s) have Code 43 errors")
            .AddButton(new ToastButton()
                .SetContent("Fix Now")
                .AddArgument("action", "fix"))
            .AddButton(new ToastButton()
                .SetContent("Dismiss")
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
            .AddText("Audio Fix Failed")
            .AddText(errorMessage)
            .Show();
    }
}
