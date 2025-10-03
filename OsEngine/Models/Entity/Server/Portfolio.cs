/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using OsEngine.Models.Market;

namespace OsEngine.Models.Entity.Server;

/// <summary>
/// Portfolio (account) in the trading system and positions opened on this account
/// </summary>
public class Portfolio
{
    /// <summary>
    /// Account number
    /// </summary>
    public string Number;
    public string AccountNumber;

    /// <summary>
    /// Deposit at the beginning of the session
    /// </summary>
    public decimal ValueBegin;

    /// <summary>
    /// Deposit amount now
    /// </summary>
    public decimal ValueCurrent;

    /// <summary>
    /// Blocked part of the deposit. And positions and bids
    /// </summary>
    public decimal ValueBlocked;

    /// <summary>
    /// Profit or loss on open positions
    /// </summary>
    public decimal UnrealizedPnl;

    /// <summary>
    /// Connector to which the portfolio belongs
    /// </summary>
    public ServerType ServerType;

    /// <summary>
    /// Connector unique name in multi-connection mode
    /// </summary>
    // TODO: replace with Server Guid
    public string ServerUniqueName;
    public Guid ServerUniqueID;

    // then goes the storage of open positions in the system by portfolio

    public List<PositionOnBoard> PositionOnBoards;
    // NOTE: Consider what use ReadOnlyCollection or IReadOnlyList?
    // public IReadOnlyList<PositionOnBoard> PositionOnBoard => PositionOnBoard.AsReadOnly();

    /// <summary>
    /// Take positions on the portfolio in the trading system
    /// </summary>
    [Obsolete(nameof(PositionOnBoards))]
    public List<PositionOnBoard> GetPositionOnBoard()
    {
        return PositionOnBoards;
    }

    /// <summary>
    /// Update the position of the instrument in the trading system
    /// </summary>
    public void SetNewPosition(PositionOnBoard position)
    {
        if (string.IsNullOrEmpty(position.SecurityNameCode))
        {
            return;
        }

        if (string.IsNullOrEmpty(position.PortfolioName))
        {
            position.PortfolioName = Number;
        }

        if (PositionOnBoards != null && PositionOnBoards.Count != 0)
        {
            for (int i = 0; i < PositionOnBoards.Count; i++)
            {
                if (PositionOnBoards[i].SecurityNameCode == position.SecurityNameCode)
                {
                    PositionOnBoards[i].ValueCurrent = position.ValueCurrent;
                    PositionOnBoards[i].ValueBlocked = position.ValueBlocked;
                    PositionOnBoards[i].UnrealizedPnl = position.UnrealizedPnl;

                    return;
                }
            }
        }

        PositionOnBoards ??= [];

        if (PositionOnBoards.Count == 0)
        {
            PositionOnBoards.Add(position);
        }
        else if (position.SecurityNameCode == "USDT"
            || position.SecurityNameCode == "USDC"
            || position.SecurityNameCode == "USD"
            || position.SecurityNameCode == "RUB"
            || position.SecurityNameCode == "EUR")
        {
            PositionOnBoards.Insert(0, position);
        }
        else if (PositionOnBoards.Count == 1)
        {
            if (FirstIsBiggerThanSecond(position.SecurityNameCode, PositionOnBoards[0].SecurityNameCode))
            {
                PositionOnBoards.Add(position);
            }
            else
            {
                PositionOnBoards.Insert(0, position);
            }
        }
        else
        { // insert name sort

            bool isInsert = false;

            for (int i = 0; i < PositionOnBoards.Count; i++)
            {
                if (PositionOnBoards[i].SecurityNameCode == "USDT"
              || PositionOnBoards[i].SecurityNameCode == "USDC"
              || PositionOnBoards[i].SecurityNameCode == "USD"
              || PositionOnBoards[i].SecurityNameCode == "RUB"
              || PositionOnBoards[i].SecurityNameCode == "EUR")
                {
                    continue;
                }

                if (FirstIsBiggerThanSecond(
                    position.SecurityNameCode,
                    PositionOnBoards[i].SecurityNameCode) == false)
                {
                    PositionOnBoards.Insert(i, position);
                    isInsert = true;
                    break;
                }
            }

            if (isInsert == false)
            {
                PositionOnBoards.Add(position);
            }
        }
    }

    private bool FirstIsBiggerThanSecond(string s1, string s2)
    {
        for (int i = 0; i < (s1.Length > s2.Length ? s2.Length : s1.Length); i++)
        {
            if (s1.ToCharArray()[i] < s2.ToCharArray()[i]) return false;
            if (s1.ToCharArray()[i] > s2.ToCharArray()[i]) return true;
        }
        return false;
    }

    /// <summary>
    /// Clear all positions on the exchange
    /// </summary>
    public void ClearPositionOnBoard()
    {
        PositionOnBoards.Clear();
    }
}
