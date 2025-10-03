using System;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;

namespace OsEngine.Models.Terminal;

public abstract partial class BaseStrategy : ILog
{
    public string NameStrategyUniq;

    public string FileName;

    /// <summary>
    /// the name the user wants to see in the interface
    /// </summary>
    public string PublicName;

    /// <summary>
    /// the program that launched the robot. Tester  Robot  Optimizer
    /// </summary>
    public StartProgram StartProgram;

    /// <summary>
    /// indicates if the robot is an included script
    /// </summary>
    public bool IsScript;

    /// <summary>
    /// a description of the robot's operating logic. Displayed in the menu for selecting a robot to create
    /// </summary>
    public string Description;

    protected event Action<string> CriticalErrorEvent;
    public event Action<string, LogMessageType> LogRecieved;

    public void OnLogRecieved(string message, LogMessageType type)
    {
        LogRecieved?.Invoke(message, type);
    }

    [Obsolete($"Use {nameof(OnLogRecieved)} instead")]
    public void SendNewLogMessage(string message, LogMessageType type) =>
        OnLogRecieved(message, type);
}
