namespace F.Framework.Core.Interfaces;

public interface ILog
{
    void Info(string message);
    void Debug(string message);
    void Warn(string message);
    void Error(string message);
    void Error(string message, Exception ex);
}