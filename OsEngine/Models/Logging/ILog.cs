namespace OsEngine.Models.Logging;

public interface ILog
{
    event System.Action<string, LogMessageType> LogRecieved;

    void OnLogRecieved(string message, LogMessageType type);
}
