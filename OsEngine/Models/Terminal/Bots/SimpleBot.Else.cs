using System;
using System.Collections.Generic;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;
using OsEngine.Models.Market.Servers;

namespace OsEngine.Models.Terminal.Bots;

public partial class SimpleBot
{
    /// <summary>
    /// time to close the deal
    /// </summary>
    private DateTime _lastClosingSurplusTime;

    /// <summary>
    /// Cancel orders with expired lifetime
    /// </summary>
    /// <param name="candles">candles</param>
    private void CancelStopOpenerByNewCandle(List<Candle> candles)
    {
        bool needSave = false;

        for (int i = 0; PositionOpenerToStop != null && i < PositionOpenerToStop.Count; i++)
        {
            if (PositionOpenerToStop[i].LifeTimeType == PositionOpenerToStopLifeTimeType.NoLifeTime)
            {
                continue;
            }

            if (PositionOpenerToStop[i].ExpiresBars <= 1)
            {
                PositionOpenerToStop.RemoveAt(i);
                i--;
                needSave = true;
                continue;
            }

            if (candles[^1].TimeStart > PositionOpenerToStop[i].LastCandleTime)
            {
                PositionOpenerToStop[i].LastCandleTime = candles[^1].TimeStart;
                PositionOpenerToStop[i].ExpiresBars = PositionOpenerToStop[i].ExpiresBars - 1;
                needSave = true;
            }
        }

        if (needSave == true)
        {
            UpdateStopLimits();
        }
    }

    public void UpdateStopLimits()
    {
        if (StartProgram != StartProgram.IsOsOptimizer)
        {
            // _chartMaster?.SetStopLimits(PositionOpenerToStop);
        }

        if (StartProgram == StartProgram.IsOsTrader)
        {
            // FIX:
            // Journal?.SetStopLimits(PositionOpenerToStop);
        }
    }

    /// <summary>
    /// Check whether it is time to open positions on stop openings
    /// </summary>
    private void CheckStopOpener(decimal price)
    {
        if (ServerStatus != ServerConnectStatus.Connect
                || Security == null
                || Portfolio == null)
        {
            return;
        }

        bool needSave = false;

        try
        {
            if (PositionOpenerToStop == null) { return; }

            for (int i = 0; i < PositionOpenerToStop.Count; i++)
            {
                PositionOpenerToStopLimit stop = PositionOpenerToStop[i];
                if (
                    (stop.ActivateType != StopActivateType.HigherOrEqual
                     || price < stop.PriceRedLine)
                    &&
                    (stop.ActivateType != StopActivateType.LowerOrEqual
                     || price > stop.PriceRedLine)
                   )
                {
                    continue;
                }

                Side direction = stop.Side;
                if (direction == Side.None)
                {
                    i--;
                    continue;
                }


                Position pos = null;

                if (stop.PositionNumber == 0)
                {
                    pos = Create(direction, stop.PriceOrder,
                            stop.Volume,
                            stop.OrderPriceType,
                            true);

                    if (pos != null
                            && !string.IsNullOrEmpty(stop.SignalType))
                    {
                        pos.SignalTypeOpen = stop.SignalType;
                    }
                }
                else
                {
                    List<Position> openPoses = PositionsOpenAll;

                    for (int f = 0; f < openPoses.Count; f++)
                    {
                        if (openPoses[f].Number == stop.PositionNumber)
                        {
                            pos = openPoses[f];
                            break;
                        }
                    }

                    if (pos != null)
                    {
                        if (pos.Direction == direction)
                        {
                            Update(direction, pos,
                                    stop.PriceOrder,
                                    stop.Volume, ManualPositionSupport.SecondToOpen, true,
                                    stop.OrderPriceType, false);
                        }
                        // NOTE: Can be removed if there is no None Direction
                        else if (pos.Direction != Side.None)
                        {
                            ClosePeaceOfDeal(pos,
                                    stop.OrderPriceType,
                                    stop.PriceOrder, ManualPositionSupport.SecondToClose,
                                    stop.Volume, true, true);
                        }
                    }
                }

                if (PositionOpenerToStop.Count == 0)
                { // the user can remove himself from the layer when he sees that the deal is opening
                    return;
                }

                PositionOpenerToStop.RemoveAt(i);
                i = -1;

                if (pos != null)
                {
                    if (direction == Side.Buy)
                    {
                        PositionBuyAtStopActivateEvent?.Invoke(pos);
                    }
                    else
                    {
                        PositionSellAtStopActivateEvent?.Invoke(pos);
                    }

                }
                needSave = true;
            }

            if (needSave == true)
            {
                UpdateStopLimits();
            }
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// Check if the trade has a stop or profit
    /// </summary>
    // NOTE: function always return false
    private bool CheckStop(Position position, decimal lastTrade)
    {
        try
        {
            if (ServerStatus != ServerConnectStatus.Connect ||
                    Security == null ||
                    Portfolio == null)
            {
                return false;
            }

            if (!position.StopOrderIsActive && !position.ProfitOrderIsActive)
            {
                return false;
            }

            if (position.StopOrderIsActive)
            {

                if (position.Direction == Side.Buy &&
                        position.StopOrderRedLine >= lastTrade)
                {
                    position.ProfitOrderIsActive = false;
                    position.StopOrderIsActive = false;

                    if (string.IsNullOrEmpty(position.SignalTypeStop) == false)
                    {
                        position.SignalTypeClose = position.SignalTypeStop;
                    }

                    OnLogRecieved(
                            "Close Position at Stop. StopPrice: " +
                            position.StopOrderRedLine
                            + " LastMarketPrice: " + lastTrade,
                            LogMessageType.System);

                    if (position.StopIsMarket == false)
                    {
                        CloseDeal(position, OrderPriceType.Limit, position.StopOrderPrice, true, true);
                    }
                    else
                    {
                        CloseDeal(position, OrderPriceType.Market, position.StopOrderPrice, true, true);
                    }

                    PositionStopActivateEvent?.Invoke(position);
                    return true;
                }

                if (position.Direction == Side.Sell &&
                        position.StopOrderRedLine <= lastTrade)
                {
                    position.StopOrderIsActive = false;
                    position.ProfitOrderIsActive = false;

                    if (string.IsNullOrEmpty(position.SignalTypeStop) == false)
                    {
                        position.SignalTypeClose = position.SignalTypeStop;
                    }

                    OnLogRecieved(
                            "Close Position at Stop. StopPrice: " +
                            position.StopOrderRedLine
                            + " LastMarketPrice: " + lastTrade,
                            LogMessageType.System);

                    if (position.StopIsMarket == false)
                    {
                        CloseDeal(position, OrderPriceType.Limit, position.StopOrderPrice, true, true);
                    }
                    else
                    {
                        CloseDeal(position, OrderPriceType.Market, position.StopOrderPrice, true, true);
                    }

                    PositionStopActivateEvent?.Invoke(position);
                    return true;
                }
            }

            if (position.ProfitOrderIsActive)
            {
                if (position.Direction == Side.Buy &&
                        position.ProfitOrderRedLine <= lastTrade)
                {
                    position.StopOrderIsActive = false;
                    position.ProfitOrderIsActive = false;

                    if (string.IsNullOrEmpty(position.SignalTypeProfit) == false)
                    {
                        position.SignalTypeClose = position.SignalTypeProfit;
                    }

                    OnLogRecieved(
                            "Close Position at Profit. ProfitPrice: " +
                            position.ProfitOrderRedLine
                            + " LastMarketPrice: " + lastTrade,
                            LogMessageType.System);

                    if (position.ProfitIsMarket == false)
                    {
                        CloseDeal(position, OrderPriceType.Limit, position.ProfitOrderPrice, true, true);
                    }
                    else
                    {
                        CloseDeal(position, OrderPriceType.Market, position.ProfitOrderPrice, true, true);
                    }

                    PositionProfitActivateEvent?.Invoke(position);
                    return true;
                }

                if (position.Direction == Side.Sell &&
                        position.ProfitOrderRedLine >= lastTrade)
                {
                    position.StopOrderIsActive = false;
                    position.ProfitOrderIsActive = false;

                    if (string.IsNullOrEmpty(position.SignalTypeProfit) == false)
                    {
                        position.SignalTypeClose = position.SignalTypeProfit;
                    }

                    OnLogRecieved(
                            "Close Position at Profit. ProfitPrice: " +
                            position.ProfitOrderRedLine
                            + " LastMarketPrice: " + lastTrade,
                            LogMessageType.System);

                    if (position.ProfitIsMarket == false)
                    {
                        CloseDeal(position, OrderPriceType.Limit, position.ProfitOrderPrice, true, true);
                    }
                    else
                    {
                        CloseDeal(position, OrderPriceType.Market, position.ProfitOrderPrice, true, true);
                    }

                    PositionProfitActivateEvent?.Invoke(position);
                    return true;
                }
            }
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
        return false;
    }


    /// <summary>
    /// check whether it is not necessary to close the transactions for which the search was at the close
    /// </summary>
    private void CheckSurplusPositions()
    {
        if (StartProgram == StartProgram.IsOsTrader && _lastClosingSurplusTime != DateTime.MinValue &&
            _lastClosingSurplusTime.AddSeconds(10) > DateTime.Now)
        {
            return;
        }

        _lastClosingSurplusTime = DateTime.Now;

        if (Positions == null)
        {
            return;
        }

        if (ServerStatus != ServerConnectStatus.Connect ||
            Security == null ||
            Portfolio == null)
        {
            return;
        }

        bool haveSurplusPos = false;

        List<Position> positions = Positions;

        if (positions == null ||
            positions.Count == 0)
        {
            return;
        }

        for (int i = positions.Count - 1; i > -1 && i > positions.Count - 10; i--)
        {
            if (positions[i].State == PositionStateType.ClosingSurplus)
            {
                haveSurplusPos = true;
                break;
            }
        }

        if (haveSurplusPos == false)
        {
            return;
        }

        positions = Positions.FindAll(position =>
                                      position.State == PositionStateType.ClosingSurplus
                                      || position.OpenVolume < 0);

        if (positions.Count == 0)
        {
            return;
        }
        bool haveOpenOrders = false;

        for (int i = 0; i < positions.Count; i++)
        {
            Position position = positions[i];

            if (position.CloseActive)
            {
                CloseAllOrderToPosition(position);
                haveOpenOrders = true;
            }
        }

        if (haveOpenOrders)
        {
            return;
        }

        for (int i = 0; i < positions.Count; i++)
        {
            Position position = positions[i];

            if (position.OpenOrders.Count > 20)
            {
                continue;
            }

            if (position.Direction == Side.None) { continue; }

            if (position.OpenVolume < 0)
            {
                decimal price;
                if (position.Direction == Side.Buy)
                {
                    price = PriceBestAsk + Security.PriceStep * 30;
                }
                else
                {
                    price = PriceBestBid - Security.PriceStep * 30;
                }
                Update(position.Direction, position, price,
                       -position.OpenVolume, new TimeSpan(0, 0, 1, 0), false, OrderPriceType.Limit, true);
            }
        }
    }
}
