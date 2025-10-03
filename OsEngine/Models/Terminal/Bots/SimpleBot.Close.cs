using System;
using System.Collections.ObjectModel;
using OsEngine.Language;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;

namespace OsEngine.Models.Terminal.Bots;

public partial class SimpleBot
{
    public CloseOperations Close;

    // NOTE: It cancel all order rather that close
    public void CloseAllOrderToPosition(Position position)
    {
        try
        {
            position.StopOrderIsActive = false;
            position.ProfitOrderIsActive = false;


            if (position.OpenOrders != null)
            {
                for (int i = 0; i < position.OpenOrders.Count; i++)
                {
                    Order order = position.OpenOrders[i];
                    if (order.State == OrderStateType.Active)
                    {
                        _connector.OrderCancel(position.OpenOrders[i]);
                    }
                }
            }


            if (position.CloseOrders != null)
            {
                for (int i = 0; i < position.CloseOrders.Count; i++)
                {
                    Order closeOrder = position.CloseOrders[i];

                    if (closeOrder.State == OrderStateType.Active)
                    {
                        _connector.OrderCancel(closeOrder);
                    }
                }
            }
        }
        catch (Exception error)
        {
            SetNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }


    private void CloseAtMarket(Position position, decimal volume, string signalType = "")
    {
        try
        {
            if (Connector.IsConnected == false
                    || Connector.IsReadyToTrade == false)
            {
                OnLogRecieved(OsLocalization.Trader.Label191, LogMessageType.Error);
                return;
            }

            position.ProfitOrderIsActive = false;
            position.StopOrderIsActive = false;

            if (volume <= 0 || position.OpenVolume <= 0)
            {
                return;
            }
            decimal price = Connector.BestAsk;

            if (price == 0)
            {
                OnLogRecieved(OsLocalization.Trader.Label290, LogMessageType.System);
                return;
            }
            if (StartProgram == StartProgram.IsOsTrader)
            {
                if (!Connector.EmulatorIsOn)
                {
                    if (position.Direction == Side.Buy)
                    {
                        // NOTE: Use Security.PriceHighLimit/PriceLowLimit?
                        price = Connector.BestBid - Security.PriceStep * 40;
                    }
                    else //if (position.Direction == Side.Sell)
                    {
                        price += Security.PriceStep * 40;
                    }

                }
            }

            if (Connector.MarketOrdersIsSupport)
            {
                if (position.OpenVolume <= volume)
                {
                    CloseDeal(position, OrderPriceType.Market, price, false, true);
                }
                else if (position.OpenVolume > volume)
                {
                    ClosePeaceOfDeal(position, OrderPriceType.Market, price, ManualPositionSupport.SecondToClose, volume, true, false);
                }
            }
            else
            {
                CloseAtLimit(position, price, volume);
            }
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    private void CloseAllMarket(string signalType = "")
    {
        try
        {
            ObservableCollection<Position> positions = Journal.OpenPositions;

            if (positions == null) { return; }

            for (int i = 0; i < positions.Count; i++)
            {
                CloseAtMarket(positions[i], positions[i].OpenVolume, signalType);
            }
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    private void CloseAtLimit(Position position, decimal priceLimit, decimal volume, string signalType = "")
    {
        if (volume <= 0 || position.OpenVolume <= 0) { return; }

        try
        {
            if (position.OpenVolume <= volume)
            {
                CloseDeal(position, OrderPriceType.Limit, priceLimit, false, true);
            }
            else if (position.OpenVolume > volume)
            {
                ClosePeaceOfDeal(position, OrderPriceType.Limit, priceLimit, ManualPositionSupport.SecondToClose, volume, true, false);
            }

            if (position.CloseOrders[^1].State == OrderStateType.None)
            {

            }
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    public void CloseAtProfit(Position position, decimal priceActivation, decimal priceOrder, string signalType = "")
    {
        if (position == null
                || position.State == PositionStateType.Done
                || position.State == PositionStateType.OpeningFail
                || (position.ProfitOrderIsActive
                    && position.ProfitOrderPrice == priceOrder
                    && position.ProfitOrderRedLine == priceActivation)
                || position.OpenVolume == 0)
        {
            return;
        }

        try
        {
            if (StartProgram == StartProgram.IsOsOptimizer
                    || StartProgram == StartProgram.IsTester)
            {
                // check that the profit is no further than the activation price deep in the market

                decimal lastBid = PriceBestBid;
                decimal lastAsk = PriceBestAsk;

                if (lastAsk != 0 && lastBid != 0)
                {
                    if (position.Direction == Side.Buy &&
                            priceActivation < lastBid)
                    {
                        // priceActivation = lastBid;
                        // SetNewLogMessage(
                        //    OsLocalization.Trader.Label181
                        //    , LogMessageType.Error);
                    }
                    if (position.Direction == Side.Sell &&
                            priceActivation > lastAsk)
                    {
                        // priceActivation = lastAsk;
                        //SetNewLogMessage(
                        //   OsLocalization.Trader.Label181
                        //   , LogMessageType.Error);
                    }
                }

                priceOrder = priceActivation;
                position.ProfitOrderPrice = priceActivation;
            }
            else
            {
                position.ProfitOrderPrice = priceOrder;
            }

            position.ProfitOrderRedLine = priceActivation;
            position.ProfitOrderIsActive = true;
            if (signalType != "")
            {
                position.Signals.Profit = signalType;
            }

            // _chartMaster.SetPosition(_journal.AllPosition);
            // _journal.PaintPosition(position);
            // _journal.Save();

        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// Place profit market order for a position
    /// </summary>
    /// <param name="position">position to be closed</param>
    /// <param name="priceActivation">the price of the stop order, after reaching which the order is placed</param>
    public void CloseAtProfitMarket(Position position, decimal priceActivation, string signalType = "")
    {
        if (position == null
                || position.State == PositionStateType.Done
                || position.State == PositionStateType.OpeningFail
                || (position.ProfitOrderIsActive
                    && position.ProfitOrderPrice == priceActivation
                    && position.ProfitOrderRedLine == priceActivation
                    && position.ProfitIsMarket == true)
                || position.OpenVolume == 0)
        {
            return;
        }

        position.ProfitOrderPrice = priceActivation;
        position.ProfitOrderRedLine = priceActivation;
        position.ProfitIsMarket = true;

        position.ProfitOrderIsActive = true;
        if (signalType != "")
        {
            position.Signals.Profit = signalType;
        }

        try
        {
            // _chartMaster.SetPosition(_journal.AllPosition);
            // _journal.PaintPosition(position);
            // _journal.Save();

        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    public void CloseAtStop(Position position, decimal priceActivation, decimal priceOrder, string signalType = "")
    {
        if (position == null
                || position.State == PositionStateType.Done
                || position.State == PositionStateType.OpeningFail
                || (position.StopOrderIsActive
                    && position.StopOrderPrice == priceOrder
                    && position.StopOrderRedLine == priceActivation)
                || position.OpenVolume == 0)
        {
            return;
        }

        try
        {
            if (StartProgram == StartProgram.IsOsOptimizer ||
                    StartProgram == StartProgram.IsTester)
            {
                // check that the stop is no further than the activation price deep in the market

                decimal lastBid = PriceBestBid;
                decimal lastAsk = PriceBestAsk;

                if (lastAsk != 0 && lastBid != 0)
                {
                    if (position.Direction == Side.Buy &&
                            priceActivation > lastAsk)
                    {
                        //priceActivate = lastAsk;
                        //SetNewLogMessage(
                        //    OsLocalization.Trader.Label180
                        //    , LogMessageType.Error);
                    }
                    if (position.Direction == Side.Sell &&
                            priceActivation < lastBid)
                    {
                        // priceActivate = lastBid;
                        //SetNewLogMessage(
                        //    OsLocalization.Trader.Label180
                        //    , LogMessageType.Error);
                    }
                }
                position.StopOrderPrice = priceActivation;
            }
            else
            {
                position.StopOrderPrice = priceOrder;
            }

            position.StopOrderRedLine = priceActivation;
            position.StopOrderIsActive = true;

            // _chartMaster.SetPosition(_journal.AllPosition);
            // _journal.PaintPosition(position);
            // _journal.Save();
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    public void CloseAtStopMarket(Position position, decimal priceActivation, string signalType = "")
    {
        if (position == null
                || position.State == PositionStateType.Done
                || position.State == PositionStateType.OpeningFail
                || (position.StopOrderIsActive
                    && position.StopOrderPrice == priceActivation
                    && position.StopOrderRedLine == priceActivation
                    && position.StopIsMarket == true)
                || position.OpenVolume == 0)
        {
            return;
        }

        position.StopIsMarket = true;
        position.StopOrderPrice = priceActivation;
        position.StopOrderRedLine = priceActivation;
        position.StopOrderIsActive = true;

        try
        {
            // _chartMaster.SetPosition(_journal.AllPosition);
            // _journal.PaintPosition(position);
            // _journal.Save();
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    public void CloseAtTrailingStop(Position position, decimal priceActivation, decimal priceOrder, string signalType = "")
    {
        if (position.StopOrderIsActive
            && (position.IsBuy
                && position.StopOrderPrice > priceOrder
                || position.IsSell
                && position.StopOrderPrice < priceOrder))
        {
            return;
        }

        CloseAtStop(position, priceActivation, priceOrder);
    }

    public void CloseAtTrailingStopMarket(Position position, decimal priceActivation, string signalType = "")
    {
        if (position.StopOrderIsActive &&
                position.Direction == Side.Buy &&
                position.StopOrderRedLine > priceActivation || position.StopOrderIsActive &&
                position.Direction == Side.Sell &&
                position.StopOrderRedLine < priceActivation || position.OpenVolume == 0)
        {
            return;
        }

        position.StopIsMarket = true;
        position.StopOrderPrice = priceActivation;
        position.StopOrderRedLine = priceActivation;
        position.StopOrderIsActive = true;

        // _chartMaster.SetPosition(_journal.AllPosition);
        // _journal.PaintPosition(position);
        // _journal.Save();
    }

    public void CloseAtIceberg(Position position, decimal price, decimal volume, int ordersCount, string signalType = "")
    {
        if (volume <= 0 || position.OpenVolume <= 0) { return; }

        if (StartProgram != StartProgram.IsOsTrader || ordersCount <= 1)
        {
            CloseAtLimit(position, price, volume);
            return;
        }

        Side side = position.IsBuy ? Side.Sell : Side.Buy;
        OpenIcebergToPosition(side, position, price, volume, ordersCount);
    }

    public void CloseAtIcebergMarket(Position position, decimal volume, int ordersCount, int minMillisecondsDistance, string signalType = "")
    {
        if (volume <= 0 || position.OpenVolume <= 0) { return; }

        if (StartProgram != StartProgram.IsOsTrader || ordersCount <= 1)
        {
            CloseAtMarket(position, volume);
            return;
        }

        Side side = position.IsBuy ? Side.Sell : Side.Buy;
        OpenIcebergToPositionMarket(side, position, volume, ordersCount, minMillisecondsDistance);
    }

    /// <summary>
    /// Close the position in Fake mode
    /// </summary>
    /// <param name="position">position to be closed</param>
    // private Position CloseFake(Position position, decimal volume, decimal price, DateTime time, string signalType = "")
    public void CloseAtFake(Position position, decimal volume, decimal price, DateTime time)
    {
        try
        {
            if (Connector.IsConnected == false
                    || Connector.IsReadyToTrade == false)
            {
                OnLogRecieved(OsLocalization.Trader.Label191, LogMessageType.Error);
                return;
            }

            if (volume <= 0 || position.OpenVolume <= 0)
            {
                return;
            }

            if (position == null)
            {
                return;
            }

            position.ProfitOrderIsActive = false;
            position.StopOrderIsActive = false;

            for (int i = 0; position.CloseOrders != null && i < position.CloseOrders.Count; i++)
            {
                if (position.CloseOrders[i].State == OrderStateType.Active)
                {
                    Connector.OrderCancel(position.CloseOrders[i]);
                }
            }

            for (int i = 0; position.OpenOrders != null && i < position.OpenOrders.Count; i++)
            {
                if (position.OpenOrders[i].State == OrderStateType.Active)
                {
                    Connector.OrderCancel(position.OpenOrders[i]);
                }
            }

            if (Security == null)
            {
                return;
            }

            Side sideCloseOrder = Side.Buy;

            if (position.Direction == Side.Buy)
            {
                sideCloseOrder = Side.Sell;
            }

            price = RoundPrice(price);

            if (position.State == PositionStateType.Done &&
                    position.OpenVolume == 0)
            {
                return;
            }

            position.State = PositionStateType.Closing;

            // Order closeOrder
            //     = _dealCreator.CreateCloseOrderForDeal(Security, position, price,
            //             OrderPriceType.Limit, new TimeSpan(1, 1, 1, 1), 
            //             StartProgram, ManualPositionSupport.OrderTypeTime, _connector.ServerFullName);

            Order closeOrder = new()
            {
                NumberUser = NumberGen.GetNumberOrder(StartProgram),
                Side = position.IsBuy ? Side.Sell : Side.Buy,
                Price = price,
                Volume = position.OpenVolume,
                TypeOrder = OrderPriceType.Limit,
                LifeTime = new TimeSpan(1, 1, 1, 1),
                PositionConditionType = OrderPositionConditionType.Close,
                SecurityNameCode = Security.Name,
                SecurityClassCode = Security.NameClass,
                OrderTypeTime = ManualPositionSupport.OrderTypeTime,
                ServerName = Connector.ServerFullName,
                PortfolioNumber = Portfolio.Number
            };

            if (volume < position.OpenVolume &&
                    closeOrder.Volume != volume)
            {
                closeOrder.Volume = volume;
            }

            position.AddNewCloseOrder(closeOrder);

            FakeExecute(closeOrder, time);

        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }



    private void CloseDeal(Position position, OrderPriceType priceType, decimal price, bool isStopOrProfit, bool safeRegime = true, decimal? volume = null)
    {
        if (position == null)
        {
            return;
        }
        try
        {
            if (safeRegime)
            {
                for (int i = 0; position.CloseOrders != null && i < position.CloseOrders.Count; i++)
                {
                    if (position.CloseOrders[i].State == OrderStateType.Active)
                    {
                        Connector.OrderCancel(position.CloseOrders[i]);
                    }
                }

                for (int i = 0; position.OpenOrders != null && i < position.OpenOrders.Count; i++)
                {
                    if (position.OpenOrders[i].State == OrderStateType.Active)
                    {
                        Connector.OrderCancel(position.OpenOrders[i]);
                    }
                }
            }

            if (Security == null) { return; }

            if (position.State == PositionStateType.Done
                    && position.OpenVolume == 0)
            {
                return;
            }

            price = RoundPrice(price);

            position.State = PositionStateType.Closing;

            // Order closeOrder = _dealCreator.CreateCloseOrderForDeal(Security, position, price,
            //         priceType, lifeTime, StartProgram, 
            //         ManualPositionSupport.OrderTypeTime, _connector.ServerFullName);

            if (position.OpenVolume == 0)
            {
                position.State = PositionStateType.OpeningFail;
                return;
            }

            Order closeOrder = new()
            {
                NumberUser = NumberGen.GetNumberOrder(StartProgram),
                Side = position.IsBuy ? Side.Sell : Side.Buy,
                Price = price,
                Volume = volume ?? position.OpenVolume,
                TypeOrder = priceType,
                LifeTime = ManualPositionSupport.SecondToClose,
                PositionConditionType = OrderPositionConditionType.Close,
                SecurityNameCode = Security.Name,
                SecurityClassCode = Security.NameClass,
                OrderTypeTime = ManualPositionSupport.OrderTypeTime,
                ServerName = Connector.ServerFullName,
                PortfolioNumber = Portfolio.Number
            };

            if (isStopOrProfit)
            {
                closeOrder.IsStopOrProfit = true;
            }
            position.AddNewCloseOrder(closeOrder);

            if (position.OpenOrders[0].SecurityNameCode.EndsWith(" TestPaper"))
            {
                Connector.OrderExecute(closeOrder, true);
            }
            else
            {
                Connector.OrderExecute(closeOrder);
            }
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    // TODO: Can be unified with CloseDeal
    private void ClosePeaceOfDeal(Position position, OrderPriceType priceType,
            decimal price, TimeSpan lifeTime, decimal volume,
            bool safeRegime, bool isStopOrProfit)
    {
        if (position == null) { return; }

        try
        {
            if (safeRegime == true)
            {
                if (position.CloseOrders != null)
                {
                    for (int i = 0; i < position.CloseOrders.Count; i++)
                    {
                        Order order = position.CloseOrders[i];
                        if (order.State != OrderStateType.Done && order.State != OrderStateType.Cancel)
                        {
                            Connector.OrderCancel(order);
                        }
                    }
                }

                if (position.OpenOrders != null &&
                        position.OpenOrders.Count > 0)
                {
                    for (int i = 0; i < position.OpenOrders.Count; i++)
                    {
                        if (position.OpenOrders[i].State == OrderStateType.Active)
                        {
                            Connector.OrderCancel(position.OpenOrders[i]);
                        }
                    }
                }
            }

            if (Security == null) { return; }

            if (position.OpenVolume == 0)
            {
                position.State = PositionStateType.OpeningFail;
                return;
            }

            price = RoundPrice(price);

            // Order closeOrder = _dealCreator.CreateCloseOrderForDeal(Security, position, price,
            //         priceType, lifeTime, StartProgram, ManualPositionSupport.OrderTypeTime, _connector.ServerFullName);

            Order closeOrder = new()
            {
                NumberUser = NumberGen.GetNumberOrder(StartProgram),
                Side = position.IsBuy ? Side.Sell : Side.Buy,
                Price = price,
                Volume = volume,
                TypeOrder = priceType,
                LifeTime = lifeTime,
                PositionConditionType = OrderPositionConditionType.Close,
                SecurityNameCode = Security.Name,
                SecurityClassCode = Security.NameClass,
                OrderTypeTime = ManualPositionSupport.OrderTypeTime,
                ServerName = Connector.ServerFullName,
                IsStopOrProfit = isStopOrProfit,
            };

            position.AddNewCloseOrder(closeOrder);

            if (position.OpenOrders[0].SecurityNameCode.EndsWith(" TestPaper"))
            {
                Connector.OrderExecute(closeOrder, true);
            }
            else
            {
                Connector.OrderExecute(closeOrder);
            }
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }














    public sealed class CloseOperations
    {
        private readonly SimpleBot bot;

        internal CloseOperations(SimpleBot bot)
        {
            this.bot = bot;
        }

        public void AllAtMarket(string signalType = "")
        {
            bot.CloseAllMarket(signalType);
        }

        public void Market(Position position, decimal volume, string signalType = "")
        {
            bot.CloseAtMarket(position, volume, signalType);
        }

        public void Limit(Position position, decimal price, decimal volume, string signalType = "")
        {
            bot.CloseAtLimit(position, price, volume, signalType);
        }

        public void Profit(Position position, decimal activationPrice, decimal orderPrice, string signalType = "")
        {
            bot.CloseAtProfit(position, activationPrice, orderPrice, signalType);
        }

        public void ProfitMarket(Position position, decimal activationPrice, string signalType = "")
        {
            bot.CloseAtProfitMarket(position, activationPrice, signalType);
        }

        public void Stop(Position position, decimal activationPrice, decimal orderPrice, string signalType = "")
        {
            bot.CloseAtStop(position, activationPrice, orderPrice, signalType);
        }

        public void StopMarket(Position position, decimal activationPrice, string signalType = "")
        {

            bot.CloseAtStopMarket(position, activationPrice, signalType);
        }

        public void TrailingStop(Position position, decimal activationPrice, decimal orderPrice, string signalType = "")
        {
            bot.CloseAtTrailingStop(position, activationPrice, orderPrice, signalType);
        }

        public void TrailingStopMarket(Position position, decimal activationPrice, string signalType = "")
        {
            bot.CloseAtTrailingStopMarket(position, activationPrice, signalType);
        }

        public void Iceberg(Position position, decimal price, decimal volume, int ordersNumber, string signalType = "")
        {
            bot.CloseAtIceberg(position, price, volume, ordersNumber, signalType);
        }

        public void IcebergMarket(Position position, decimal volume, int ordersNumber, int minMillisecondsDistance, string signalType = "")
        {
            bot.CloseAtIcebergMarket(position, volume, ordersNumber, minMillisecondsDistance, signalType);
        }

        public void Fake(Position position, decimal volume, decimal price, DateTime time, string signalType = "")
        {
            bot.CloseAtFake(position, volume, price, time);
        }
    }
}
