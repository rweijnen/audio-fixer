namespace AudioFixer.Services;

public sealed class LogService : IDisposable
{
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AudioFixer");

    private static readonly string LogFilePath = Path.Combine(LogDirectory, "audiofixer.log");
    private static readonly int RetentionDays = 14;

    private readonly StreamWriter _writer;

    public LogService()
    {
        Directory.CreateDirectory(LogDirectory);
        PruneOldEntries();
        var fs = new FileStream(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        _writer = new StreamWriter(fs) { AutoFlush = true };
    }

    public void Log(string message)
    {
        _writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}  {message}");
    }

    public void LogInvocation(string[] args)
    {
        string argsStr = args.Length > 0 ? string.Join(' ', args) : "(no args)";
        Log($"INVOKED  args={argsStr}");
    }

    public void LogDeviceStatus(bool hasProblem, int affectedCount, int totalCount)
    {
        if (hasProblem)
            Log($"STATUS   Code 43 detected on {affectedCount} of {totalCount} CS35L56 device(s)");
        else
            Log($"STATUS   All {totalCount} CS35L56 device(s) OK");
    }

    public void LogUserResponse(string response)
    {
        Log($"USER     {response}");
    }

    public void LogFixResult(bool success, string? errorMessage = null)
    {
        if (success)
            Log("FIX      Success - all devices recovered");
        else
            Log($"FIX      Failed - {errorMessage}");
    }

    public void LogTaskRegistration(bool installed)
    {
        Log(installed ? "TASK     Scheduled task registered" : "TASK     Scheduled task removed");
    }

    public void LogError(string message)
    {
        Log($"ERROR    {message}");
    }

    public void Dispose()
    {
        _writer.Dispose();
    }

    private void PruneOldEntries()
    {
        if (!File.Exists(LogFilePath))
            return;

        try
        {
            var cutoff = DateTime.Now.AddDays(-RetentionDays);
            var lines = File.ReadAllLines(LogFilePath);
            var retained = new List<string>();

            foreach (var line in lines)
            {
                // Parse timestamp from start of line (yyyy-MM-dd HH:mm:ss.fff)
                if (line.Length >= 23 &&
                    DateTime.TryParseExact(line[..23], "yyyy-MM-dd HH:mm:ss.fff",
                        null, System.Globalization.DateTimeStyles.None, out var timestamp))
                {
                    if (timestamp >= cutoff)
                        retained.Add(line);
                }
                else
                {
                    // Keep lines that don't have a parseable timestamp (shouldn't happen)
                    retained.Add(line);
                }
            }

            File.WriteAllLines(LogFilePath, retained);
        }
        catch
        {
            // Don't fail the app if log pruning fails
        }
    }
}
