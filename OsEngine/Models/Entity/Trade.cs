/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Globalization;
using Newtonsoft.Json;

namespace OsEngine.Models.Entity;

// TODO: make it struct
public class Trade
{
    /// <summary>
    /// Instrument code for which the transaction took place
    /// </summary>
    // NOTE: Not sure if every Trade need security code,
    // rather what contain List of Trades should contain it
    // or it should be shareable
    // NOTE: Use string.Intern when set this. That should reduce memory
    public string SecurityNameCode
    {
        get;
        set => field = string.Intern(value);
    }

    /// <summary>
    /// Transaction number in the system
    /// </summary>
    // NOTE: convert to ulong?
    public string Id;

    /// <summary>
    /// Transaction number in the system. In Tester and Optimizer
    /// </summary>
    // NOTE: Why not use Id instead
    public long IdInTester;

    public DateTime Time { get; set; }
    public decimal Price { get; set; }
    public decimal Volume { get; set; }

    public Side Side;

    [JsonIgnore]
    public bool IsBuy => Side == Side.Buy;
    [JsonIgnore]
    public bool IsSell => Side == Side.Sell;

    // NOTE: i'm not sure it should be here
    // And if it is why decimal instead long/ulong
    public decimal OpenInterest;

    /// <summary>
    /// Tester only. Timeframe of the candlestick that generated the trade
    /// </summary>
    // NOTE: Not sure if every Trade need security code,
    // rather what contain List of Trades should contain it
    public TimeFrame TimeFrameInTester;

    // a new part. This part of the final is not to be downloaded. It can be obtained from OsData, only from standard connectors

    // NOTE: Not sure if Trade should contain these

    /// <summary>
    /// The best buy in the market depth when this trade came in.
    /// </summary>
    // TODO: Excessive, Price and Side more than enough
    public decimal Bid;

    /// <summary>
    /// The best sale in a market depth when this trade came in.
    /// </summary>
    // TODO: Excessive, Price and Side more than enough
    public decimal Ask;

    /// <summary>
    /// The total volume of buy in the market depth at the moment when this trade came in
    /// </summary>
    // TODO: Excessive, Volume and Side more than enough
    public decimal BidsVolume;

    /// <summary>
    /// The total volume of sales in a market depth at the moment when this trade came in
    /// </summary>
    // TODO: Excessive, Volume and Side more than enough
    public decimal AsksVolume;

    /// <summary>
    ///To take a line to save
    /// </summary>
    /// <returns>line with the state of the object</returns>
    public string GetSaveString()
    {
        //20150401,100000,86160.000000000,2
        // либо 20150401,100000,86160.000000000,2, Buy/Sell
        string result = "";
        result += Time.ToString("yyyyMMdd,HHmmss") + ",";
        result += Price.ToString(CultureInfo.InvariantCulture) + ",";
        result += Volume.ToString(CultureInfo.InvariantCulture) + ",";
        result += Side + ",";
        result += Time.Microsecond;

        if (Id != null)
        {
            result += ",";
            result += Id;
        }
        else
        {
            result += ",";
        }

        if (Bid != 0 && Ask != 0 &&
            BidsVolume != 0 && AsksVolume != 0)
        {
            result += ",";
            result += Bid.ToString(CultureInfo.InvariantCulture) + ",";
            result += Ask.ToString(CultureInfo.InvariantCulture) + ",";
            result += BidsVolume.ToString(CultureInfo.InvariantCulture) + ",";
            result += AsksVolume.ToString(CultureInfo.InvariantCulture);
        }

        return result;
    }

    /// <summary>
    /// Upload a tick from a saved line
    /// </summary>
    /// <param name="In">incoming data</param>
    public void SetTradeFromString(string In)
    {
        //20150401,100000,86160.000000000,2
        // либо 20150401,100000,86160.000000000,2, Buy/Sell

        if (string.IsNullOrWhiteSpace(In)) { return; }

        string[] sIn = In.Split(',');

        if (sIn.Length >= 6 && (sIn[5] == "C" || sIn[5] == "S"))
        {
            // download data from IqFeed
            // загружаем данные из IqFeed
            Time = Convert.ToDateTime(sIn[0]);
            Price = sIn[1].ToDecimal();
            Volume = sIn[2].ToDecimal();
            Bid = sIn[3].ToDecimal();
            Ask = sIn[4].ToDecimal();
            Side = GetSideIqFeed();

            return;
        }

        Time = DateTimeParseHelper.ParseFromTwoStrings(sIn[0], sIn[1]);
        
        Price = sIn[2].ToDecimal();

        Volume = sIn[3].ToDecimal();

        if (sIn.Length > 4)
        {
            Enum.TryParse(sIn[4], true, out Side);
        }

        if (sIn.Length > 5)
        {
            Time.AddMicroseconds(Convert.ToInt32(sIn[5]));
        }

        if (sIn.Length > 6)
        {
            Id = sIn[6];
        }

        if (sIn.Length > 8)
        {
            Bid = sIn[7].ToDecimal();
            Ask = sIn[8].ToDecimal();
            BidsVolume = sIn[9].ToDecimal();
            AsksVolume = sIn[10].ToDecimal();
        }
    }

    // NOTE: Should not be here
    private Random _rand = new();

    /// <summary>
    /// direction generation for transactions from IqFeed
    /// </summary>
    private Side GetSideIqFeed()
    {

        if (Bid != Ask)
        {
            //the deal was for sale / сделка была на продажу
            if (Bid == Price) { return Side.Sell; }
            // the deal was to buy / сделка была на покупку
            if (Ask == Price) { return Side.Buy; }
        }


        // in other cases, we indicate a random direction/ в остальных случаях указываем случайное направление

        return _rand.Next(2) == 0 ? Side.Buy : Side.Sell;
    }
}
