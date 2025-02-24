using Chickensoft.GodotNodeInterfaces;

namespace F.Framework.Logging;

public interface ILogService : INode
{
    void Debug(string message);
    void Info(string message);
    void Warn(string message);
    void Error(string message);
    void Error(string message, Exception e);
}