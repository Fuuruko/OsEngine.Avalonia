/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Globalization;

namespace OsEngine.Models.Entity;

/// <summary>
/// an object encapsulating data for opening a Stop transaction. OpenAtStop
/// </summary>
// NOTE: Rename to StopOrder
public class PositionOpenerToStopLimit
{
    public string Security;

    // NOTE: Used only to display in datagrid
    public string TabName;

    public int Number;

    public PositionOpenerToStopLifeTimeType LifeTimeType;

    /// <summary>
    /// order price
    /// </summary>
    public decimal OrderPrice;
    [Obsolete(nameof(OrderPrice))]
    public decimal PriceOrder;

    /// <summary>
    /// the price of the line that we look at the breakdown
    /// </summary>
    public decimal PriceRedLine;

    /// <summary>
    /// side of price breakdown for order activation
    /// </summary>
    public StopActivateType ActivateType;

    /// <summary>
    /// volume for opening a position
    /// </summary>
    public decimal Volume;

    /// <summary>
    /// side of the position being opened
    /// </summary>
    public Side Side;

    /// <summary>
    /// Way to open a deal. Limit or Market
    /// </summary>
    public OrderPriceType OrderPriceType;

    /// <summary>
    /// order lifetime in bars (candle count)
    /// </summary>
    public int ExpiresBars { get; set; } = 0;

    public int OrderCreateBarNumber { get; set; }

    /// <summary>
    /// last candle time when checking the order life
    /// </summary>
    public DateTime LastCandleTime;

    /// <summary>
    /// type of opening signal to be written to the position
    /// </summary>
    public string SignalType;

    /// <summary>
    /// time of order creation
    /// </summary>
    public DateTime TimeCreate;

    /// <summary>
    /// Position to add new order
    /// </summary>
    public int PositionNumber;

    public string GetSaveString()
    {
        string saveStr = "";

        saveStr += Security + "&";
        saveStr += TabName + "&";
        saveStr += Number + "&";
        saveStr += LifeTimeType + "&";
        saveStr += PriceOrder + "&";
        saveStr += PriceRedLine + "&";
        saveStr += ActivateType + "&";
        saveStr += Volume + "&";
        saveStr += Side + "&";
        saveStr += ExpiresBars + "&";
        saveStr += OrderCreateBarNumber + "&";
        saveStr += LastCandleTime.ToString(CultureInfo) + "&";
        saveStr += SignalType + "&";
        saveStr += TimeCreate.ToString(CultureInfo) + "&";
        saveStr += OrderPriceType +"&";
        saveStr += PositionNumber ;

        return saveStr;
    }

    public void LoadFromString(string str)
    {
        string[] savStr = str.Split('&');

        Security = savStr[0];
        TabName = savStr[1];
        Number = Convert.ToInt32(savStr[2]);
        Enum.TryParse(savStr[3], out LifeTimeType);
        PriceOrder = savStr[4].ToDecimal();
        PriceRedLine = savStr[5].ToDecimal();
        Enum.TryParse(savStr[6], out ActivateType);
        Volume = savStr[7].ToDecimal();
        Enum.TryParse(savStr[8], out Side);
        ExpiresBars = Convert.ToInt32(savStr[9]);
        OrderCreateBarNumber = Convert.ToInt32(savStr[10]);
        LastCandleTime = Convert.ToDateTime(savStr[11], CultureInfo);
        SignalType = savStr[12];
        TimeCreate = Convert.ToDateTime(savStr[13], CultureInfo);

        if(savStr.Length > 14)
        {
            Enum.TryParse(savStr[14], out OrderPriceType);
        }

        if (savStr.Length > 15)
        {
            PositionNumber = Convert.ToInt32(savStr[15]);
        }
    }

    private static readonly CultureInfo CultureInfo = new("ru-RU");
}

/// <summary>
/// side of price breakdown for activation stop-opening order
/// </summary>
public enum StopActivateType
{

    /// <summary>
    /// activate when the price is higher or equal
    /// </summary>
    HigherOrEqual,

    /// <summary>
    /// activate when the price is lower or equal 
    /// </summary>
    LowerOrEqual,
}

public enum PositionOpenerToStopLifeTimeType
{
    CandlesCount,
    GTC,

    NoLifeTime = GTC,
}
