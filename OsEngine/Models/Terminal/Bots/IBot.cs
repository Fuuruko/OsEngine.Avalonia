using System;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;

namespace OsEngine.Models.Terminal;

public interface IBot
{
    /// <summary>
    /// source type
    /// </summary>
    BotTabType TabType { get; }

    /// <summary>
    /// Remove tab and all child structures
    /// </summary>
    void Delete();

    /// <summary>
    /// Clear
    /// </summary>
    void Clear();

    /// <summary>
    /// are events sent to the top from the tab?
    /// </summary>
    bool EventsIsOn { get; set; }

    /// <summary>
    /// are events sent to the top from the tab?
    /// </summary>
    bool EmulatorIsOn { get; set; }
}

public abstract class BaseBot : ILog
{
    public string Name { get; set; }

    public StartProgram StartProgram { get; set; }

    /// <summary>
    /// Tab number
    /// </summary>
    // NOTE: Not really used
    public int TabNum { get; set; }

    /// <summary>
    /// Time of the last update of the candle
    /// </summary>
    public DateTime LastTimeCandleUpdate { get; set; }

    /// <summary>
    /// Source removed
    /// </summary>
    public event Action TabDeletedEvent;

    public void OnLogRecieved(string str, LogMessageType type) =>
        LogRecieved?.Invoke(str, type);

    public event Action<string, LogMessageType> LogRecieved;

    [Obsolete(nameof(OnLogRecieved))]
    public void SetNewLogMessage(string str, LogMessageType type) =>
        LogMessageEvent?.Invoke(str, type);

    [Obsolete(nameof(LogRecieved))]
    public event Action<string, LogMessageType> LogMessageEvent;
}
