namespace Variant01.Services;

public sealed class ActionLogger
{
    private readonly string _logPath;
    private readonly object _lock = new();

    public ActionLogger()
    {
        string directory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "Variant01",
            "ProcessManager");

        Directory.CreateDirectory(directory);

        _logPath = Path.Combine(directory, "actions.log");
    }

    public void Write(string message)
    {
        string entry =
            $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] {message}";

        lock (_lock)
        {
            File.AppendAllText(
                _logPath,
                entry + Environment.NewLine);
        }
    }

    public List<string> ReadAll()
    {
        lock (_lock)
        {
            return File.Exists(_logPath)
                ? File.ReadAllLines(_logPath).ToList()
                : [];
        }
    }
}