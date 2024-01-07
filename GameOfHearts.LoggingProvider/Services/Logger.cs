namespace GameOfHearts.LoggingProvider.Services;

public sealed class Logger : IDisposable
{
    private readonly string _logPathBase;
    private readonly List<string> _logs;

    public Logger(string logPathBase)
    {
        // assume provided directory exists
        _logPathBase = logPathBase;
        _logs = new();
    }

    public Logger()
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        // using Visual Studio run, current directory should be
        // ...\GameOfHearts\GameOfHearts.ConsoleApp\bin\Debug\net8.0\
        // using dotnet run in terminal, current directory should be
        // ...\GameOfHearts\GameOfHearts.ConsoleApp\
        // move to \GameOfHears\logs
        string relativePath = currentDirectory.EndsWith("GameOfHearts.ConsoleApp")
            ? "../logs"
            : "../../../../logs";
        string logsDirectory = Path.Combine(currentDirectory, relativePath);
        _logPathBase = logsDirectory;

        // create directory if it does not exist
        if (!Directory.Exists(logsDirectory))
        {
            Directory.CreateDirectory(logsDirectory);
        }

        _logs = new();
    }

    private void WriteLogs()
    {
        string logFileName = $"log.{DateTime.Now:yyyy-MM-dd.HH-mm}.txt";
        string file = Path.Combine(_logPathBase, logFileName);
        Console.WriteLine($"Writing log in file: '{file}'...");

        File.WriteAllLines(file, _logs);
    }

    private void AddLog(string text)
    {
        _logs.Add(text);
    }

    public void LogInformation(string message)
    {
        AddLog($"{DateTime.Now:HH:mm:ss.fff} <INFO> {message}");
    }

    public void LogWarning(string message)
    {
        DateTime moment = DateTime.Now;
        string timeStamp = $"{moment.Hour}:{moment.Minute}:{moment.Second}.{moment.Millisecond}";
        AddLog($"{timeStamp} <WARN> {message}");
    }

    public void LogError(string message)
    {
        DateTime moment = DateTime.Now;
        string timeStamp = $"{moment.Hour}:{moment.Minute}:{moment.Second}.{moment.Millisecond}";
        AddLog($"{timeStamp} <ERROR> {message}");
    }

    public void Dispose()
    {
        WriteLogs();
    }
}
