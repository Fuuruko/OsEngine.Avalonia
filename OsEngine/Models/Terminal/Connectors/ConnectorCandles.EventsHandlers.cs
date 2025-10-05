/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using OsEngine.Models.Candles;
using OsEngine.Models.Entity;
using OsEngine.Models.Entity.Server;
using OsEngine.Models.Logging;
using OsEngine.Models.Market.Servers;

namespace OsEngine.Models.Market.Connectors;

public partial class ConnectorCandles
{
    /// <summary>
    /// test finished. Event from tester
    /// </summary>
    // NOTE: Not used anywhere
    private void Connector_TestingEndEvent()
    {
        try
        {
            TestOverEvent?.Invoke();
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    // NOTE: Used only in Grids. Can be done there?
    private void Connector_TestingStartEvent()
    {
        try
        {
            TestStartEvent?.Invoke();
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// time of the last completed candle
    /// </summary>
    private DateTime _timeLastEndCandle = DateTime.MinValue;

    /// <summary>
    /// the candle has just ended
    /// </summary>
    private void MySeries_CandleFinishedEvent(CandleSeries candleSeries)
    {
        try
        {
            if (EventsIsOn == false)
            {
                return;
            }

            List<Candle> candles = Candles(true);

            if (candles == null || candles.Count == 0)
            {
                return;
            }

            DateTime timeLastCandle = candles[^1].TimeStart;

            if (timeLastCandle == _timeLastEndCandle
                    && CandleCreateMethodType == "Simple")
            {
                return;
            }

            _timeLastEndCandle = timeLastCandle;

            NewCandlesChangeEvent?.Invoke(candles);
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// the candle updated
    /// </summary>
    private void MySeries_CandleUpdateEvent(CandleSeries candleSeries)
    {
        try
        {
            if (LastCandlesChangeEvent != null && EventsIsOn == true)
            {
                LastCandlesChangeEvent(Candles(false));
            }
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// incoming order
    /// </summary>
    private void ConnectorBot_NewOrderIncomeEvent(Order order)
    {
        try
        {
            if (StartProgram != StartProgram.IsOsTrader)
            {// tester or optimizer
                if (order.SecurityNameCode != SecurityName)
                {
                    return;
                }
            }

            if (string.IsNullOrEmpty(order.ServerName))
            {
                order.ServerName = ServerFullName;
            }

            OrderChangeEvent?.Invoke(order);

            // FIX:
            // ServerMaster.InsertOrder(order);
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    private void _myServer_CancelOrderFailEvent(Order order)
    {
        try
        {
            if (StartProgram != StartProgram.IsOsTrader)
            {// tester or optimizer
                if (order.SecurityNameCode != SecurityName)
                {
                    return;
                }
            }

            CancelOrderFailEvent?.Invoke(order);
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// incoming my trade
    /// </summary>
    private void ConnectorBot_NewMyTradeEvent(MyTrade trade)
    {
        if (MyServer.ServerStatus != ServerConnectStatus.Connect)
        {
            return;
        }

        if (StartProgram != StartProgram.IsOsTrader)
        {// tester or optimizer
            if (trade.SecurityNameCode != SecurityName)
            {
                return;
            }
        }

        try
        {
            MyTradeEvent?.Invoke(trade);
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// incoming best bid with ask
    /// </summary>
    private void ConnectorBotNewBidAscIncomeEvent(decimal bestBid, decimal bestAsk, Security security)
    {
        try
        {
            if (security == null ||
                    security.Name != _securityName)
            {
                return;
            }

            BestBid = bestBid;
            BestAsk = bestAsk;

            if (StartProgram == StartProgram.IsOsTrader)
            {
                if (EmulatorIsOn || ServerType == ServerType.Finam)
                {
                    _emulator?.ProcessBidAsc(BestBid, BestAsk);
                }
                if (BestBidAskChangeEvent != null
                        && EventsIsOn == true)
                {
                    BestBidAskChangeEvent(bestBid, bestAsk);
                }
            }
            else
            {// Tester or Optimizer
                BestBidAskChangeEvent?.Invoke(bestBid, bestAsk);
            }
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// incoming depth
    /// </summary>
    private void ConnectorBot_NewMarketDepthEvent(MarketDepth glass)
    {
        try
        {
            if (_securityName == null)
            {
                return;
            }

            if (_securityName != glass.SecurityNameCode)
            {
                return;
            }

            if (GlassChangeEvent != null && EventsIsOn == true)
            {
                GlassChangeEvent(glass);
            }

            decimal bestBid = 0;

            if (glass.Bids != null &&
                    glass.Bids.Count > 0)
            {
                bestBid = glass.Bids[0].Price;
            }

            decimal bestAsk = 0;

            if (glass.Asks != null &&
                    glass.Asks.Count > 0)
            {
                bestAsk = glass.Asks[0].Price;
            }

            if (EmulatorIsOn)
            {
                _emulator?.ProcessBidAsc(bestAsk, bestBid);
            }

            if (bestAsk != 0)
            {
                BestAsk = bestAsk;
            }
            if (bestBid != 0)
            {
                BestBid = bestBid;
            }
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// incoming trades
    /// </summary>
    private void ConnectorBot_NewTradeEvent(List<Trade> tradesList)
    {
        try
        {
            if (_securityName == null
                    || tradesList == null
                    || tradesList.Count == 0)
            {
                return;
            }
            else
            {
                int count = tradesList.Count - 1;

                if (tradesList[count] == null ||
                        tradesList[count].SecurityNameCode != _securityName)
                {
                    return;
                }
            }
        }
        catch
        {
            // NOTE: LOL
            // it's hard to catch the error here. Who will understand what is wrong - well done 
            // ошибка здесь трудноуловимая. Кто понял что не так - молодец
            return;
        }

        try
        {
            if (TickChangeEvent != null && EventsIsOn == true)
            {
                TickChangeEvent(tradesList);
            }
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// incoming server time
    /// </summary>
    private void myServer_TimeServerChangeEvent(DateTime time)
    {
        try
        {
            if (TimeChangeEvent != null && EventsIsOn == true)
            {
                TimeChangeEvent(time);
            }
            if (EmulatorIsOn == true)
            {
                _emulator?.ProcessTime(time);
            }
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// on the stock market has changed the state of the portfolio
    /// </summary>
    private void Server_PortfoliosChangeEvent(List<Portfolio> portfolios)
    {
        try
        {
            Portfolio myPortfolio = null;

            for (int i = 0; i < portfolios.Count; i++)
            {
                if (PortfolioName == portfolios[i].Number)
                {
                    myPortfolio = portfolios[i];
                    break;
                }
            }

            if (myPortfolio != null &&
                    PortfolioOnExchangeChangedEvent != null)
            {
                PortfolioOnExchangeChangedEvent(myPortfolio);
            }
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    private void Server_NewAdditionalMarketDataEvent(OptionMarketData data)
    {
        try
        {
            if (_securityName != data.SecurityName)
            {
                return;
            }

            OptionMarketData.SecurityName = data.SecurityName;
            OptionMarketData.UnderlyingAsset = data.UnderlyingAsset;
            OptionMarketData.UnderlyingPrice = data.UnderlyingPrice;
            OptionMarketData.MarkPrice = data.MarkPrice;
            OptionMarketData.MarkIV = data.MarkIV;
            OptionMarketData.BidIV = data.BidIV;
            OptionMarketData.AskIV = data.AskIV;
            OptionMarketData.Delta = data.Delta;
            OptionMarketData.Gamma = data.Gamma;
            OptionMarketData.Vega = data.Vega;
            OptionMarketData.Theta = data.Theta;
            OptionMarketData.Rho = data.Rho;
            OptionMarketData.OpenInterest = data.OpenInterest;
            OptionMarketData.TimeCreate = data.TimeCreate;
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    private void Server_NewVolume24hUpdateEvent(SecurityVolumes data)
    {
        if (_securityName != data.SecurityNameCode)
        {
            return;
        }

        SecurityVolumes.SecurityNameCode = data.SecurityNameCode;

        bool isChange = false;

        if (data.Volume24h != 0 && SecurityVolumes.Volume24h != data.Volume24h)
        {
            SecurityVolumes.Volume24h = data.Volume24h;
            isChange = true;
        }

        if (data.Volume24hUSDT != 0 && SecurityVolumes.Volume24hUSDT != data.Volume24hUSDT)
        {
            SecurityVolumes.Volume24hUSDT = data.Volume24hUSDT;
            isChange = true;
        }

        if (isChange)
        {
            if (data.TimeUpdate != new DateTime(1970, 1, 1, 0, 0, 0) && SecurityVolumes.TimeUpdate != data.TimeUpdate)
            {
                SecurityVolumes.TimeUpdate = data.TimeUpdate;
            }

            SecurityVolumes marketData = new()
            {
                SecurityNameCode = SecurityVolumes.SecurityNameCode,
                Volume24h = SecurityVolumes.Volume24h,
                Volume24hUSDT = SecurityVolumes.Volume24hUSDT,
                TimeUpdate = SecurityVolumes.TimeUpdate
            };

            NewVolume24hChangedEvent?.Invoke(marketData);
        }
    }

    private void Server_NewFundingEvent(Funding data)
    {
        if (_securityName != data.SecurityNameCode)
        {
            return;
        }

        Funding.SecurityNameCode = data.SecurityNameCode;

        bool isChange = false;

        // NOTE: Can be simplified by gpt
        if (data.CurrentValue != 0 && Funding.CurrentValue != data.CurrentValue)
        {
            Funding.CurrentValue = data.CurrentValue;
            isChange = true;
        }

        if (data.NextFundingTime != new DateTime(1970, 1, 1, 0, 0, 0) && Funding.NextFundingTime != data.NextFundingTime)
        {
            Funding.NextFundingTime = data.NextFundingTime;
            isChange = true;
        }

        if (data.PreviousValue != 0 && Funding.PreviousValue != data.PreviousValue)
        {
            Funding.PreviousValue = data.PreviousValue;
            isChange = true;
        }

        if (data.PreviousFundingTime != new DateTime(1970, 1, 1, 0, 0, 0) && Funding.PreviousFundingTime != data.PreviousFundingTime)
        {
            Funding.PreviousFundingTime = data.PreviousFundingTime;
            isChange = true;
        }

        if (data.MaxFundingRate != 0 && Funding.MaxFundingRate != data.MaxFundingRate)
        {
            Funding.MaxFundingRate = data.MaxFundingRate;
            isChange = true;
        }

        if (data.MinFundingRate != 0 && Funding.MinFundingRate != data.MinFundingRate)
        {
            Funding.MinFundingRate = data.MinFundingRate;
            isChange = true;
        }

        if (Funding.NextFundingTime > new DateTime(1970, 1, 1, 0, 0, 0) &&
                Funding.PreviousFundingTime > new DateTime(1970, 1, 1, 0, 0, 0) &&
                Funding.FundingIntervalHours == 0)
        {
            Funding.NextFundingTime = Funding.NextFundingTime.AddMilliseconds(-Funding.NextFundingTime.Millisecond);
            Funding.PreviousFundingTime = Funding.PreviousFundingTime.AddMilliseconds(-Funding.PreviousFundingTime.Millisecond);

            Funding.FundingIntervalHours = (Funding.NextFundingTime - Funding.PreviousFundingTime).Hours;
            isChange = true;
        }

        if (Funding.FundingIntervalHours == 0 && data.FundingIntervalHours != 0)
        {
            Funding.FundingIntervalHours = data.FundingIntervalHours;
            isChange = true;
        }

        if (isChange)
        {
            if (data.TimeUpdate != new DateTime(1970, 1, 1, 0, 0, 0) && Funding.TimeUpdate != data.TimeUpdate)
            {
                Funding.TimeUpdate = data.TimeUpdate;
            }

            Funding marketData = new()
            {
                SecurityNameCode = Funding.SecurityNameCode,
                CurrentValue = Funding.CurrentValue,
                NextFundingTime = Funding.NextFundingTime,
                FundingIntervalHours = Funding.FundingIntervalHours,
                MaxFundingRate = Funding.MaxFundingRate,
                MinFundingRate = Funding.MinFundingRate,
                TimeUpdate = Funding.TimeUpdate
            };

            FundingChangedEvent?.Invoke(marketData);
        }
    }
}
