using System;

namespace OsEngine.Models.Entity.Server;

public class ServerWorkingTimeSettings
{
    /// <summary>
    /// beginning of the trading session
    /// начало торговой сессии
    /// </summary>
    public TimeSpan StartSessionTime;

    /// <summary>
    /// ending of the trading session
    /// конец торговой сессии
    /// </summary>
    public TimeSpan EndSessionTime;

    /// <summary>
    /// server time zone
    /// временная зона сервера
    /// </summary>
    public string ServerTimeZone;

    /// <summary>
    /// if the exchange is working on weekends, it returns true 
    /// если биржа работает по выходным возвращается true
    /// </summary>
    [Obsolete(nameof(IsWorkingOnWeekends))]
    public bool WorkingAtWeekend;
    public bool IsWorkingOnWeekends;
}
