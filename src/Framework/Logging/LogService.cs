using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

namespace F.Framework.Logging;

[Meta(typeof(IAutoNode))]
public partial class LogService : Node, ILogService, IProvide<ILogService>
{
    private readonly GDWriter _writer;

    public LogService()
    {
        _writer = new GDWriter();
    }

    public void Debug(string message)
    {
        _writer.Write(LogLevel.Debug, message);
    }

    public void Info(string message)
    {
        _writer.Write(LogLevel.Info, message);
    }

    public void Warn(string message) => GD.PushWarning($"[WARN] {message}");
    public void Error(string message)
    {
        _writer.Write(LogLevel.Error, message);
    }

    public void Error(string message, Exception e)
    {
        _writer.Write(LogLevel.Error, $"{message}\n{e}");
    }

    public override void _Notification(int what)
    {
        this.Notify(what);
    }

    ILogService IProvide<ILogService>.Value()
    {
        return this;
    }
}

public class GDWriter
{
    public void Write(LogLevel level, string message)
    {
        var prefix = level switch
        {
            LogLevel.Debug => "[DEBUG]",
            LogLevel.Info => "[INFO]",
            LogLevel.Error => "[ERROR]",
            _ => "[UNKNOWN]"
        };

        if (level == LogLevel.Error)
        {
            GD.PrintErr($"{prefix} {message}");
        }
        else
        {
            GD.Print($"{prefix} {message}");
        }
    }
}

public enum LogLevel
{
    Debug,
    Info,
    Error
}