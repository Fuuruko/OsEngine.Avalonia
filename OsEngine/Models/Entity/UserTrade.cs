/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Globalization;
using Newtonsoft.Json;

namespace OsEngine.Models.Entity;

/// <summary>
/// customer transaction on the exchange
/// </summary>
// TODO: Make it struct?
// TODO: Rename UserTrade
public class MyTrade
{
    public decimal Price;
    public decimal Volume;

    /// <summary>
    /// Trade number
    /// </summary>
    [JsonIgnore]
    [Obsolete(nameof(Number))]
    public string NumberTrade;
    public string Number;

    /// <summary>
    /// Parent's warrant number
    /// </summary>
    [JsonIgnore]
    [Obsolete(nameof(ParentOrderNumber))]
    public string NumberOrderParent;
    // NOTE: Rename? Or maybe create order field instead?
    // Also change type to long or uint?
    // Don't really think somebody make 4M trades in one day
    public string ParentOrderNumber;

    /// <summary>
    /// The robot's position number in OsEngine
    /// </summary>
    // NOTE: Can be safely turned to int
    [JsonIgnore]
    [Obsolete(nameof(PositionNumber))]
    public string NumberPosition;
    public string PositionNumber;

    /// <summary>
    /// Instrument code
    /// </summary>
    public string SecurityNameCode;

    public DateTime Time;

    // NOTE: Used only in Quik 
    // public int MicroSeconds;

    /// <summary>
    /// Party to the transaction
    /// </summary>
    public Side Side;

    private static readonly CultureInfo CultureInfo = new CultureInfo("ru-RU");

    /// <summary>
    /// To take a line to save
    /// </summary>
    // TODO: Replace with json serialization
    internal string GetStringFofSave()
    {
        string result = "";

        result += Volume.ToString(CultureInfo) + "&";
        result += Price.ToString(CultureInfo) + "&";
        result += NumberOrderParent.ToString(CultureInfo) + "&";
        result += Time.ToString(CultureInfo) + "&";
        result += NumberTrade.ToString(CultureInfo) + "&";
        result += Side + "&";
        result += SecurityNameCode + "&";
        result += NumberPosition + "&";

        return result;
    }

    /// <summary>
    /// Upload from an incoming line
    /// </summary>
    // TODO: Replace with json deserialization
    internal void SetTradeFromString(string saveString)
    {
        string[] arraySave = saveString.Split('&');

        Volume = arraySave[0].ToDecimal();
        Price = arraySave[1].ToDecimal();
        NumberOrderParent = arraySave[2];
        Time = Convert.ToDateTime(arraySave[3], CultureInfo);
        NumberTrade = arraySave[4];
        Enum.TryParse(arraySave[5], out Side);
        SecurityNameCode = arraySave[6];
        NumberPosition = arraySave[7];
    }

    /// <summary>
    /// To take a line for a hint
    /// </summary>
    // TODO: Move to Charts
    // public string ToolTip
    // {
    //     get
    //     {
    //         if (_toolTip != null)
    //         {
    //             return _toolTip;
    //         }
    //
    //         if (NumberPosition != null)
    //         {
    //             _toolTip = "Pos. num: " + NumberPosition + "\r\n";
    //         }
    //
    //         if(!NumberTrade.StartsWith("emu"))
    //         {
    //             _toolTip += "Ord. num: " + NumberOrderParent + "\r\n";
    //             _toolTip += "Trade num: " + NumberTrade + "\r\n";
    //         }
    //
    //         _toolTip += "Side: " + Side + "\r\n";
    //         _toolTip += "Time: " + Time + "\r\n";
    //         _toolTip += "Price: " + Price.ToStringWithNoEndZero() + "\r\n";
    //         _toolTip += "Volume: " + Volume.ToStringWithNoEndZero() + "\r\n";
    //
    //         return _toolTip;
    //     }
    // }
    // private string _toolTip;

    /// <summary>
    /// Service info to tester. Number candle
    /// </summary>
    // TODO: Probably can be done not here
    // public int NumberCandleInTester;
}
