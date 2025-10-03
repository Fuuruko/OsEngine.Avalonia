using System;

namespace OsEngine.Models.Logging;

// TODO: Use struct instead of class
public class Log(string message, LogMessageType type)
{
    // FIX: Make message only gettable 
    public string Message { get; set; } = message;
    public LogMessageType Type { get; } = type ;
    public DateTime Time { get; set; } = DateTime.Now;

    public override string ToString() => $"{Time}_{Type}_{Message}";

}
/// <summary>
/// log message type
/// тип сообщения для лога
/// </summary>
// TODO: Rename to LogLevel or LogType
public enum LogMessageType : byte
{
    System,
    SYSTEM = System,

    /// <summary>
    /// Bot got a signal from one of strategies 
    /// Робот получил сигнал из одной из стратегий
    /// </summary>
    Signal,
    Error,
    ERROR = Error,

    /// <summary>
    /// connect or disconnect message
    /// Сообщение о установке или обрыве соединения
    /// </summary>
    Connect,
    SERVER = Connect,

    /// <summary>
    /// transaction message
    /// Сообщение об исполнении транзакции
    /// </summary>
    Trade,
    TRADE = Trade,

    /// <summary>
    /// message without specification
    /// Сообщение без спецификации
    /// </summary>
    NoName,

    /// <summary>
    /// user action recorded
    /// Зафиксировано действие пользователя
    /// </summary>
    User,
    USER = User,

    /// <summary>
    /// Запись в логе с прошлой сессии
    /// </summary>
    // NOTE: Not really needed
    OldSession,
}
