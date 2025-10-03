/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
 */

using System;
using System.Collections.Generic;
using System.IO;
// using OsEngine.Market.Connectors;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;
// using OsEngine.OsTrader.Panels;
// using OsEngine.OsTrader.Panels.Tab;

namespace OsEngine.Models.Market.Servers.Tester;

/// <summary>
/// Tester security. Encapsulates test data and data upload methods.
/// </summary>
public class SecurityTester
{
    public Security Security;

    public string FileAddress;

    public DateTime TimeStart;

    public DateTime TimeEnd;

    public SecurityTesterDataType DataType;

    public TimeSpan TimeFrameSpan;

    public TimeFrame TimeFrame
    {
        get;
        set
        {
            if (value == field) { return; }

            field = value;

            if ((int)value > 0)
            {
                TimeFrameSpan = (TimeSpan)value.GetTimeSpan();
            }
        }
    }

    // data upload management

    public bool IsActive;

    private StreamReader _reader;

    public void Clear()
    {
        try
        {
            _reader = new StreamReader(FileAddress);
            LastCandle = null;
            LastTrade = null;
            LastMarketDepth = null;
            _tradesId = 0;
        }
        catch (Exception errror)
        {
            SendLogMessage(errror.ToString());
        }
    }

    public void Load(DateTime now)
    {
        if (IsActive == false) { return; }

        if (DataType == SecurityTesterDataType.Tick)
        {
            CheckTrades(now);
        }
        else if (DataType == SecurityTesterDataType.Candle)
        {
            CheckCandles(now);
        }
        else if (DataType == SecurityTesterDataType.MarketDepth)
        {
            CheckMarketDepth(now);
        }
    }

    // parsing candle files


    public Candle LastCandle { get; set; }

    private void CheckCandles(DateTime now)
    {
        if (_reader == null || _reader.EndOfStream)
        {
            _reader = new StreamReader(FileAddress);
        }
        if (now < TimeStart || TimeEnd < now)
        {
            return;
        }

        if (LastCandle != null &&
            LastCandle.TimeStart > now)
        {
            return;
        }

        if (LastCandle != null &&
            LastCandle.TimeStart == now)
        {
            List<Trade> lastTradesSeries =
                [
                new Trade()
                {
                    Price = LastCandle.Open,
                    Volume = 1,
                    Side = Side.Sell,
                    Time = LastCandle.TimeStart,
                    SecurityNameCode = Security.Name,
                    TimeFrameInTester = TimeFrame,
                    IdInTester = _tradesId++
                },
                new Trade()
                {
                    Price = LastCandle.High,
                    Volume = 1,
                    Side = Side.Buy,
                    Time = LastCandle.TimeStart,
                    SecurityNameCode = Security.Name,
                    TimeFrameInTester = TimeFrame,
                    IdInTester = _tradesId++
                },
                new Trade()
                {
                    Price = LastCandle.Low,
                    Volume = 1,
                    Side = Side.Sell,
                    Time = LastCandle.TimeStart,
                    SecurityNameCode = Security.Name,
                    TimeFrameInTester = TimeFrame,
                    IdInTester = _tradesId++
                },
                new Trade()
                {
                    Price = LastCandle.Close,
                    Volume = 1,
                    Side = Side.Sell,
                    Time = LastCandle.TimeStart,
                    SecurityNameCode = Security.Name,
                    TimeFrameInTester = TimeFrame,
                    IdInTester = _tradesId++
                },
                ];

            NewTradesEvent?.Invoke(lastTradesSeries);

            NewCandleEvent?.Invoke(LastCandle, Security.Name, TimeFrameSpan);

            return;
        }

        while (LastCandle == null ||
               LastCandle.TimeStart < now)
        {
            LastCandle = new Candle();
            LastCandle.SetCandleFromString(_reader.ReadLine());
        }

        if (LastCandle.TimeStart <= now)
        {
            List<Trade> lastTradesSeries =
                [
                new Trade()
                {
                    Price = LastCandle.Open,
                    Volume = 1,
                    Side = Side.Sell,
                    Time = LastCandle.TimeStart,
                    SecurityNameCode = Security.Name,
                    TimeFrameInTester = TimeFrame,
                    IdInTester = _tradesId++
                },
                new Trade()
                {
                    Price = LastCandle.High,
                    Volume = 1,
                    Side = Side.Buy,
                    Time = LastCandle.TimeStart,
                    SecurityNameCode = Security.Name,
                    TimeFrameInTester = TimeFrame,
                    IdInTester = _tradesId++
                },
                new Trade()
                {
                    Price = LastCandle.Low,
                    Volume = 1,
                    Side = Side.Sell,
                    Time = LastCandle.TimeStart,
                    SecurityNameCode = Security.Name,
                    TimeFrameInTester = TimeFrame,
                    IdInTester = _tradesId++
                },
                new Trade()
                {
                    Price = LastCandle.Close,
                    Volume = 1,
                    Side = Side.Sell,
                    Time = LastCandle.TimeStart,
                    SecurityNameCode = Security.Name,
                    TimeFrameInTester = TimeFrame,
                    IdInTester = _tradesId++
                },
                ];

            NewTradesEvent?.Invoke(lastTradesSeries);

            NewCandleEvent?.Invoke(LastCandle, Security.Name, TimeFrameSpan);

        }
    }

    public event Action<Candle, string, TimeSpan> NewCandleEvent;

    public event Action<MarketDepth> NewMarketDepthEvent;

    // parsing tick files

    public Trade LastTrade;

    public List<Trade> LastTradeSeries;

    private string _lastString;

    private long _tradesId;

    public bool IsNewDayTrade;

    public DateTime LastTradeTime;

    private void CheckTrades(DateTime now)
    {
        if (_reader == null || (_reader.EndOfStream && LastTrade == null))
        {
            _reader = new StreamReader(FileAddress);
        }
        if (now > TimeEnd ||
            now < TimeStart)
        {
            return;
        }

        if (LastTrade != null &&
            LastTrade.Time.AddMilliseconds(-LastTrade.Time.Millisecond) > now)
        {
            return;
        }

        // swing the first second if / качаем первую секунду если 

        if (LastTrade == null)
        {
            _lastString = _reader.ReadLine();
            LastTrade = new Trade();
            LastTrade.SetTradeFromString(_lastString);
            LastTrade.SecurityNameCode = Security.Name;
            LastTrade.IdInTester = _tradesId++;
        }

        while (!_reader.EndOfStream &&
               LastTrade.Time.AddMilliseconds(-LastTrade.Time.Millisecond) < now)
        {
            _lastString = _reader.ReadLine();
            LastTrade.SetTradeFromString(_lastString);
            LastTrade.SecurityNameCode = Security.Name;
            LastTrade.IdInTester = _tradesId++;
        }

        if (LastTrade.Time.AddMilliseconds(-LastTrade.Time.Millisecond) > now)
        {
            return;
        }

        // here we have the first trade in the current second / здесь имеем первый трейд в текущей секунде

        List<Trade> lastTradesSeries = [];

        if (LastTrade != null
            && LastTrade.Time == now)
        {
            lastTradesSeries.Add(LastTrade);
        }

        while (!_reader.EndOfStream)
        {
            _lastString = _reader.ReadLine();
            Trade tradeN = new Trade() { SecurityNameCode = Security.Name };
            tradeN.SetTradeFromString(_lastString);
            tradeN.IdInTester = _tradesId++;

            if (tradeN.Time.AddMilliseconds(-tradeN.Time.Millisecond) <= now)
            {
                lastTradesSeries.Add(tradeN);
            }
            else
            {
                LastTrade = tradeN;
                break;
            }
        }

        if (LastTradeTime != DateTime.MinValue
            && lastTradesSeries.Count > 0
            && LastTradeTime.Date < lastTradesSeries[0].Time.Date)
        {
            IsNewDayTrade = true;
        }
        else
        {
            IsNewDayTrade = false;
        }

        for (int i = 0; i < lastTradesSeries.Count; i++)
        {
            List<Trade> trades = [lastTradesSeries[i]];
            LastTradeSeries = trades;
            NeedToCheckOrders();
            NewTradesEvent(trades);
        }

        if (lastTradesSeries.Count > 0)
        {
            LastTradeTime = lastTradesSeries[^1].Time;
        }
    }

    public event Action<List<Trade>> NewTradesEvent;

    public event Action NeedToCheckOrders;

    // parsing market depths

    public MarketDepth LastMarketDepth;

    private void CheckMarketDepth(DateTime now)
    {
        if (_reader == null || _reader.EndOfStream)
        {
            _reader = new StreamReader(FileAddress);
        }

        if (now > TimeEnd ||
            now < TimeStart)
        {
            return;
        }

        if (LastMarketDepth != null &&
            LastMarketDepth.Time > now)
        {
            return;
        }

        // if download the first second / качаем первую секунду если 

        if (LastMarketDepth == null)
        {
            _lastString = _reader.ReadLine();
            LastMarketDepth = new MarketDepth();
            LastMarketDepth.SetMarketDepthFromString(_lastString);
            LastMarketDepth.SecurityNameCode = Security.Name;
        }

        while (!_reader.EndOfStream &&
               LastMarketDepth.Time < now)
        {
            _lastString = _reader.ReadLine();
            LastMarketDepth.SetMarketDepthFromString(_lastString);
        }

        if (LastMarketDepth.Time.AddSeconds(-1) > now)
        {
            return;
        }

        NewMarketDepthEvent?.Invoke(LastMarketDepth);
    }

    // logging

    private void SendLogMessage(string message)
    {
        LogMessageEvent?.Invoke(message, LogMessageType.Error);
    }

    public event Action<string, LogMessageType> LogMessageEvent;

}
