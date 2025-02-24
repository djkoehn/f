using F.Framework.Core.Interfaces;
using Godot;

namespace F.Framework.Logging;

public class Log : ILog
{
    private readonly GDWriter _writer;

    public Log()
    {
        _writer = new GDWriter();
    }

    public void Info(string message)
    {
        _writer.Write(LogLevel.Info, message);
    }

    public void Debug(string message)
    {
        _writer.Write(LogLevel.Debug, message);
    }

    public void Warn(string message)
    {
        _writer.Write(LogLevel.Info, $"[WARN] {message}");
    }

    public void Error(string message)
    {
        _writer.Write(LogLevel.Error, message);
    }

    public void Error(string message, Exception ex)
    {
        _writer.Write(LogLevel.Error, $"{message}\nException: {ex}");
    }
}