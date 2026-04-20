namespace DotNetExamApi.Infrastructure;

public class FileLoggerService : IDisposable
{
    private readonly string _logPath;
    private StreamWriter _writer;
    private bool _disposed;

    public FileLoggerService(string logPath)
    {
        _logPath = logPath;
        Directory.CreateDirectory(Path.GetDirectoryName(logPath) ?? ".");
        _writer = new StreamWriter(logPath, append: true);
    }

    public void Log(string message)
    {
        if (_disposed) return;
        _writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        _writer.Flush();
    }

    public void LogError(string message, Exception? ex = null)
    {
        if (_disposed) return;
        var errorLine = ex != null
            ? $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message} - {ex.Message}"
            : $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}";
        _writer.WriteLine(errorLine);
        _writer.Flush();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _writer?.Close();
        _disposed = true;
    }

    ~FileLoggerService()
    {
        _writer?.Close();
    }
}
