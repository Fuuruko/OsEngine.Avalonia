/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

namespace OsEngine.Models.Entity;

/// <summary>
/// Common position on the instrument on the exchange
/// </summary>
// NOTE: Rename to Deposit(?)
public class PositionOnBoard
{
    /// <summary>
    /// Position at the beginning of the session
    /// </summary>
    public decimal ValueBegin;
    public decimal StartValue; // NOTE: InitialValue(?)

    /// <summary>
    /// Current volume
    /// </summary>
    public decimal ValueCurrent;
    public decimal CurrentValue;

    /// <summary>
    /// Blocked volume
    /// </summary>
    public decimal ValueBlocked;
    public decimal BlockedValue;

    /// <summary>
    /// Profit or loss on open positions
    /// </summary>
    public decimal UnrealizedPnl;

    /// <summary>
    /// Tool for which the position is open
    /// </summary>
    public string SecurityNameCode;

    /// <summary>
    /// Portfolio on which the position is open
    /// </summary>
    public string PortfolioName;
}
