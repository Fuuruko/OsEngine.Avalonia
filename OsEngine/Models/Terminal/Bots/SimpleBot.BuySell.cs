using System;
using System.Globalization;
using OsEngine.Language;
using OsEngine.Models.Entity;
using OsEngine.Models.Entity.Server;
using OsEngine.Models.Logging;
using OsEngine.Models.Market.Servers;

namespace OsEngine.Models.Terminal.Bots;

public partial class SimpleBot
{
    public BuySellOperations Buy;
    public BuySellOperations Sell;

    /// <summary>
    /// Order price change
    /// </summary>
    /// <param name="order">An order that will have a new price</param>
    /// <param name="newPrice">New price</param>
    public void ChangeOrderPrice(Order order, decimal newPrice)
    {
        if (order == null)
        {
            return;
        }

        // NOTE: Strange restriction
        if (StartProgram != StartProgram.IsOsTrader)
        {
            OnLogRecieved(OsLocalization.Trader.Label371, LogMessageType.Error);
            return;
        }

        if (IsConnected == false ||
                IsReadyToTrade == false)
        {
            OnLogRecieved(OsLocalization.Trader.Label372, LogMessageType.Error);
            return;
        }

        if (Server == null)
        {
            return;
        }

        if (Server.ServerStatus == ServerConnectStatus.Disconnect)
        {
            OnLogRecieved(OsLocalization.Market.Message2, LogMessageType.Error);
            return;
        }

        if (order.Volume == order.VolumeExecute
                || order.State == OrderStateType.Done
                || order.State == OrderStateType.Fail)
        {
            return;
        }

        // NOTE: How inline here?
        Connector.ChangeOrderPrice(order, newPrice);

        // try
        // {
        //     if (EmulatorIsOn)
        //     {
        //         if (_emulator.ChangeOrderPrice(order, newPrice))
        //         {
        //             if (_isDelete)
        //             {
        //                 return;
        //             }
        //             // Order orderInJournal = Journal.IsMyOrder(order);
        //
        //             if (orderInJournal == null)
        //             {
        //                 return;
        //             }
        //             // Journal.SetNewOrder(order);
        //             // NOTE: Why icebergmaker
        //             // _icebergMaker.SetNewOrder(order);
        //
        //             if (OrderUpdateEvent != null)
        //             {
        //                 OrderUpdateEvent(orderInJournal);
        //             }
        //             // _chartMaster.SetPosition(PositionsAll);
        //         }
        //     }
        //     else
        //     {
        //         // NOTE: Can be moved to server side
        //         // but need to preserve SetNewLogMessage to bot rather than server
        //         if (Server.Permissions.CanChangeOrderPrice == false)
        //         {
        //             SetNewLogMessage(OsLocalization.Trader.Label373, LogMessageType.Error);
        //             return;
        //         }
        //
        //         Server.ChangeOrderPrice(order, newPrice);
        //     }
        // }
        // catch (Exception error)
        // {
        //     SetNewLogMessage(error.ToString(), LogMessageType.Error);
        // }
    }

    /// <summary>
    /// Adjust order price to the needs of the exchange
    /// </summary>
    /// <param name="price">the current price at which the high-level interface wanted to close the position</param>
    public decimal RoundPrice(decimal price)
    {
        if (Security.PriceStep == 0) { return price; }

        try
        {
            if (Security.Decimals > 0)
            {
                price = Math.Round(price, Security.Decimals);

                decimal minStep = 0.1m;

                for (int i = 1; i < Security.Decimals; i++)
                {
                    minStep *= 0.1m;
                }

                // NOTE: Can it be done like this: 
                // price -= price % Security.PriceStep;
                while (price % Security.PriceStep != 0)
                {
                    price -= minStep;
                }
            }
            else
            {
                price = Math.Round(price, 0);
                price -= price % Security.PriceStep;
            }

            // NOTE: Should this check be done here or at server side?
            if (price < Security.PriceLowLimit)
            {
                return Security.PriceLowLimit;
            }
            else if (Security.PriceHighLimit < price)
            {
                return Security.PriceHighLimit;
            }
            else
            {
                return price;
            }
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }

        return 0;
    }


    public Position OpenMarket(Side direction, decimal volume, string signalType = "")
    {
        try
        {
            if (Connector.IsConnected == false
                    || Connector.IsReadyToTrade == false)
            {
                OnLogRecieved(OsLocalization.Trader.Label191, LogMessageType.Error);
                return null;
            }
            decimal price = Connector.BestAsk;

            if (price == 0)
            {
                OnLogRecieved(OsLocalization.Trader.Label290, LogMessageType.System);
                return null;
            }

            if (StartProgram == StartProgram.IsOsTrader)
            {
                if (!Connector.EmulatorIsOn)
                {
                    price += Security.PriceStep * 40;
                }
            }

            OrderPriceType type = OrderPriceType.Market;

            if (Connector.MarketOrdersIsSupport)
            {
                return Create(direction, price, volume, type, false);
            }
            else
            {
                return OpenLimit(direction, volume, price);
            }
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }

        return null;

    }

    public void OpenMarketToPosition(Side direction, Position position, decimal volume)
    {
        try
        {
            if (Connector.IsConnected == false
                    || Connector.IsReadyToTrade == false)
            {
                OnLogRecieved(OsLocalization.Trader.Label191, LogMessageType.Error);
                return;
            }

            if (position.Direction == Side.Sell)
            {
                OnLogRecieved(Name + OsLocalization.Trader.Label65, LogMessageType.Error);

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
                    price += Security.PriceStep * 40;
                }
            }

            if (Security != null && Security.PriceStep < 1 && Convert.ToDouble(Security.PriceStep).ToString(new CultureInfo("ru-RU")).Split(',').Length != 1)
            {
                int countPoint = Convert.ToDouble(Security.PriceStep).ToString(new CultureInfo("ru-RU")).Split(',')[1].Length;
                price = Math.Round(price, countPoint);
            }
            else if (Security != null && Security.PriceStep >= 1)
            {
                price = Math.Round(price, 0);
                while (price % Security.PriceStep != 0)
                {
                    price--;
                }
            }

            if (Connector.MarketOrdersIsSupport)
            {
                Update(direction, position, price, volume, ManualPositionSupport.SecondToOpen, false, OrderPriceType.Market, true);
            }
            else
            {
                Update(direction, position, price, volume, ManualPositionSupport.SecondToOpen, false, OrderPriceType.Limit, true);
            }

        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    private Position OpenFake(Side direction, decimal volume, decimal price, DateTime time, string signalType = "")
    {
        string message = null;
        if (volume == 0)
        {
            message = OsLocalization.Trader.Label63;
        }
        else if (price == 0)
        {
            message = OsLocalization.Trader.Label291;
        }
        else if (Security == null || Portfolio == null)
        {
            message = OsLocalization.Trader.Label64;
        }

        if (message != null)
        {
            OnLogRecieved(message, LogMessageType.System);
            return null;
        }

        try
        {
            price = RoundPrice(price);

            // Position newDeal = PositionCreator.CreatePosition(Name, direction, price, volume, OrderPriceType.Limit,
            //         ManualPositionSupport.SecondToOpen, Security, Portfolio, StartProgram, ManualPositionSupport.OrderTypeTime);

            // decimal PortfolioValueOnOpenPosition;
            // if(StartProgram == StartProgram.IsOsTrader)
            // {
            //     PortfolioValueOnOpenPosition = Portfolio.ValueCurrent;
            // }
            // else
            // {// Tester, Optimizer, etc
            //  // NOTE: Why?
            //     PortfolioValueOnOpenPosition = Math.Round(Portfolio.ValueCurrent,2);
            // }

            // XXX: Check correct working
            Position fakePosition = new()
            {
                Number = NumberGen.GetNumberDeal(StartProgram),

                Direction = direction,
                State = PositionStateType.Opening,
                NameBot = Name,
                Lots = Security.Lot,
                PriceStep = Security.PriceStep,
                PriceStepCost = Security.PriceStepCost,
                PortfolioValueOnOpenPosition = Portfolio.ValueCurrent,
                SignalTypeOpen = signalType
            };

            Order fakeOrder = new()
            {
                NumberUser = NumberGen.GetNumberOrder(StartProgram),

                Side = direction,
                Price = price,
                Volume = volume,
                TypeOrder = OrderPriceType.Limit,
                LifeTime = ManualPositionSupport.SecondToOpen,
                PositionConditionType = OrderPositionConditionType.Open,
                SecurityNameCode = Security.Name,
                SecurityClassCode = Security.NameClass,
                OrderTypeTime = ManualPositionSupport.OrderTypeTime,
                ServerName = Portfolio.ServerUniqueName,
                PortfolioNumber = Portfolio.Number,
            };

            fakePosition.AddNewOpenOrder(fakeOrder);

            Journal.SetNewPosition(fakePosition);

            FakeExecute(fakeOrder, time);
            return fakePosition;
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
        return null;
    }

    private Position OpenLimit(Side direction, decimal volume, decimal priceLimit, string signalType = "")
    {
        try
        {
            if (Connector.IsConnected == false
                    || Connector.IsReadyToTrade == false)
            {
                OnLogRecieved(OsLocalization.Trader.Label191, LogMessageType.Error);
                return null;
            }

            Position position = Create(direction, priceLimit, volume, OrderPriceType.Limit, false);
            position.SignalTypeOpen = signalType;
            return position;
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
        return null;
    }

    public void OpenLimitToPosition(Side direction, Position position, decimal priceLimit, decimal volume)
    {
        try
        {
            if (Connector.IsConnected == false
                    || Connector.IsReadyToTrade == false)
            {
                OnLogRecieved(OsLocalization.Trader.Label191, LogMessageType.Error);
                return;
            }

            if (position.Direction == Side.Sell)
            {
                OnLogRecieved(Name + OsLocalization.Trader.Label65, LogMessageType.Error);
                return;
            }

            Update(direction, position, priceLimit, volume, ManualPositionSupport.SecondToOpen, false, OrderPriceType.Limit, true);
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    public void OpenStop(decimal volume, decimal priceLimit, decimal priceRedLine,
            StopActivateType activateType, int expiresBars, string signalType, PositionOpenerToStopLifeTimeType lifeTimeType)
    {
        try
        {
            if (Connector.IsConnected == false
                    || Connector.IsReadyToTrade == false)
            {
                OnLogRecieved(OsLocalization.Trader.Label191, LogMessageType.Error);
                return;
            }

            decimal price = StartProgram == StartProgram.IsOsTrader
                ? priceLimit : priceRedLine;
            PositionOpenerToStopLimit positionOpener = new()
            {
                Volume = volume,
                Security = Security.Name,
                Number = NumberGen.GetNumberDeal(StartProgram),
                ExpiresBars = expiresBars,
                TimeCreate = TimeServerCurrent,
                OrderCreateBarNumber = CandlesFinishedOnly.Count,
                TabName = Name,
                LifeTimeType = lifeTimeType,


                PriceOrder = price,
                PriceRedLine = priceRedLine,
                ActivateType = activateType,
                Side = Side.Buy,
                SignalType = signalType,
                OrderPriceType = OrderPriceType.Limit
            };

            PositionOpenerToStop.Add(positionOpener);
            UpdateStopLimits();
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    public Position OpenIceberg(Side direction, decimal volume, decimal price, int ordersCount)
    {
        try
        {
            if (Connector.IsConnected == false
                    || Connector.IsReadyToTrade == false)
            {
                OnLogRecieved(OsLocalization.Trader.Label191, LogMessageType.Error);
                return null;
            }

            if (StartProgram != StartProgram.IsOsTrader || ordersCount <= 1)
            {
                return OpenLimit(direction, volume, price);
            }

            if (volume == 0)
            {
                OnLogRecieved(OsLocalization.Trader.Label63, LogMessageType.System);
                return null;
            }

            if (price == 0)
            {
                OnLogRecieved(OsLocalization.Trader.Label291, LogMessageType.System);
                return null;
            }

            if (Security == null || Portfolio == null)
            {
                OnLogRecieved(OsLocalization.Trader.Label64, LogMessageType.System);
                return null;
            }

            if (Security != null)
            {
                if (Convert.ToDouble(Security.PriceStep).ToString(new CultureInfo("ru-RU")).Split(',').Length != 1)
                {
                    int point = Convert.ToDouble(Security.PriceStep).ToString(new CultureInfo("ru-RU")).Split(',')[1].Length;
                    price = Math.Round(price, point);
                }
                else
                {
                    price = Math.Round(price, 0);
                    while (price % Security.PriceStep != 0)
                    {
                        price--;
                    }
                }
            }
            else
            {
                decimal lastPrice = Connector.BestBid;
                if (lastPrice.ToString(new CultureInfo("ru-RU")).Split(',').Length != 1)
                {
                    int point = lastPrice.ToString(new CultureInfo("ru-RU")).Split(',')[1].Length;
                    price = Math.Round(price, point);
                }
                else
                {
                    price = Math.Round(price, 0);
                }
            }

            Position position = new()
            {
                Number = NumberGen.GetNumberDeal(StartProgram),
                Direction = Side.Buy,
                State = PositionStateType.Opening,

                NameBot = Name,
                Lots = Security.Lot,
                PriceStepCost = Security.PriceStepCost,
                PriceStep = Security.PriceStep,
                PortfolioValueOnOpenPosition = Portfolio.ValueCurrent
            };

            // if (StartProgram == StartProgram.IsOsTrader)
            // {
            //     position.PortfolioValueOnOpenPosition = Portfolio.ValueCurrent;
            // }
            // else
            // { // Tester, Optimizer, etc
            //     position.PortfolioValueOnOpenPosition = Math.Round(Portfolio.ValueCurrent, 2);
            // }

            Journal.SetNewPosition(position);

            // _icebergMaker.MakeNewIceberg(price, ManualPositionSupport.SecondToOpen,
            //         ordersCount, newDeal, IcebergType.Open, volume, this, OrderPriceType.Limit, 0);

            return position;
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
        return null;
    }

    public Position OpenIcebergMarket(Side direction, decimal volume, int ordersCount, int minMillisecondsDistance)
    {
        try
        {
            if (Connector.IsConnected == false
                    || Connector.IsReadyToTrade == false)
            {
                OnLogRecieved(OsLocalization.Trader.Label191, LogMessageType.Error);
                return null;
            }

            if (StartProgram != StartProgram.IsOsTrader || ordersCount <= 1)
            {
                return OpenMarket(direction, volume);
            }

            if (volume == 0)
            {
                OnLogRecieved(OsLocalization.Trader.Label63, LogMessageType.System);
                return null;
            }

            if (Security == null || Portfolio == null)
            {
                OnLogRecieved(OsLocalization.Trader.Label64, LogMessageType.System);
                return null;
            }

            Position position = new()
            {
                Number = NumberGen.GetNumberDeal(StartProgram),
                Direction = Side.Buy,
                State = PositionStateType.Opening,

                NameBot = Name,
                Lots = Security.Lot,
                PriceStepCost = Security.PriceStepCost,
                PriceStep = Security.PriceStep,
                PortfolioValueOnOpenPosition = Portfolio.ValueCurrent
            };

            // if (StartProgram == StartProgram.IsOsTrader)
            // {
            //     position.PortfolioValueOnOpenPosition = Portfolio.ValueCurrent;
            // }
            // else
            // { // Tester, Optimizer, etc
            //     position.PortfolioValueOnOpenPosition = Math.Round(Portfolio.ValueCurrent, 2);
            // }

            Journal.SetNewPosition(position);

            decimal price = PriceBestAsk;

            // _icebergMaker.MakeNewIceberg(price, ManualPositionSupport.SecondToOpen, ordersCount,
            //         position, IcebergType.Open, volume, this, OrderPriceType.Market, minMillisecondsDistance);

            return position;
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
        return null;
    }

    private void OpenIcebergToPosition(Side direction, Position position, decimal price, decimal volume, int ordersCount)
    {
        try
        {
            if (Connector.IsConnected == false
                    || Connector.IsReadyToTrade == false)
            {
                OnLogRecieved(OsLocalization.Trader.Label191, LogMessageType.Error);
                return;
            }

            if (StartProgram != StartProgram.IsOsTrader || ordersCount <= 1)
            {
                if (position.Direction == Side.Sell)
                {
                    ClosePeaceOfDeal(position, OrderPriceType.Limit, price, ManualPositionSupport.SecondToClose, volume, true, false);
                    // CloseDeal(position, OrderPriceType.Limit, price,
                    //         volume: volume,
                    //         isStopOrProfit: false);

                    return;
                }

                Update(direction, position, price, volume, ManualPositionSupport.SecondToOpen, false, OrderPriceType.Limit, true);
                return;
            }

            if (volume == 0)
            {
                OnLogRecieved(OsLocalization.Trader.Label63, LogMessageType.System);
                return;
            }

            if (price == 0)
            {
                OnLogRecieved(OsLocalization.Trader.Label291, LogMessageType.System);
                return;
            }

            if (Security == null || Portfolio == null)
            {
                OnLogRecieved(OsLocalization.Trader.Label64, LogMessageType.System);
                return;
            }

            // FIX:
            if (Convert.ToDouble(Security.PriceStep).ToString(new CultureInfo("ru-RU")).Split(',').Length != 1)
            {
                int point = Convert.ToDouble(Security.PriceStep).ToString(new CultureInfo("ru-RU")).Split(',')[1].Length;
                price = Math.Round(price, point);
            }
            else
            {
                price = Math.Round(price, 0);
                price -= price % Security.PriceStep;
            }

            // FIX:
            // _icebergMaker.MakeNewIceberg(price, ManualPositionSupport.SecondToOpen, ordersCount,
            //         position, IcebergType.ModifyBuy, volume, this, OrderPriceType.Limit, 0);
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    private void OpenIcebergToPositionMarket(Side direction, Position position, decimal volume, int ordersCount, int minMillisecondsDistance)
    {
        try
        {
            if (Connector.IsConnected == false
                    || Connector.IsReadyToTrade == false)
            {
                OnLogRecieved(OsLocalization.Trader.Label191, LogMessageType.Error);
                return;
            }

            if (StartProgram != StartProgram.IsOsTrader || ordersCount <= 1)
            {
                OpenMarketToPosition(direction, position, volume);
                return;
            }

            if (volume == 0)
            {
                OnLogRecieved(OsLocalization.Trader.Label63, LogMessageType.System);
                return;
            }

            if (Security == null || Portfolio == null)
            {
                OnLogRecieved(OsLocalization.Trader.Label64, LogMessageType.System);
                return;
            }

            // FIX:
            // decimal price = PriceBestAsk;
            //
            // _icebergMaker.MakeNewIceberg(price, ManualPositionSupport.SecondToOpen,
            //         ordersCount, position, IcebergType.ModifyBuy, volume, this, OrderPriceType.Market, minMillisecondsDistance);
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    internal Position Create(Side direction, decimal price, decimal volume, OrderPriceType priceType, bool isStopOrProfit)
    {
        string message = null;
        if (volume == 0)
        {
            message = OsLocalization.Trader.Label63;
        }
        else if (price == 0)
        {
            message = OsLocalization.Trader.Label291;
        }
        else if (Security == null || Portfolio == null)
        {
            message = OsLocalization.Trader.Label64;
        }

        if (message != null)
        {
            OnLogRecieved(message, LogMessageType.System);
            return null;
        }

        try
        {

            price = RoundPrice(price);

            Position newDeal = PositionCreator.CreatePosition(Name, direction, price, volume, priceType,
                    ManualPositionSupport.SecondToOpen, Security, Portfolio, StartProgram, ManualPositionSupport.OrderTypeTime);
            newDeal.OpenOrders[0].IsStopOrProfit = isStopOrProfit;
            Journal.SetNewPosition(newDeal);

            Connector.OrderExecute(newDeal.OpenOrders[0]);

            return newDeal;
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
        return null;
    }

    /// <summary>
    /// Modify position by long order
    /// </summary>
    /// <param name="position">position</param>
    /// <param name="price">order price</param>
    /// <param name="volume">volume</param>
    /// <param name="timeLife">life time</param>
    /// <param name="isStopOrProfit">whether the order is a result of a stop or a profit</param>
    /// <param name="orderType">whether the order is a result of a stop or a profit</param>
    /// <param name="safeRegime">if True - active orders to close the position will be withdrawn.</param>
    // NOTE: timeLife different only for CheckSurplusPositions function
    private void Update(Side direction, Position position, decimal price, decimal volume, TimeSpan timeLife, bool isStopOrProfit, OrderPriceType orderType, bool safeRegime)
    {
        try
        {
            if (volume == 0)
            {
                OnLogRecieved(OsLocalization.Trader.Label63, LogMessageType.System);
                return;
            }

            if (price == 0)
            {
                OnLogRecieved(OsLocalization.Trader.Label291, LogMessageType.System);
                return;
            }

            if (Security == null || Portfolio == null)
            {
                OnLogRecieved(OsLocalization.Trader.Label64, LogMessageType.System);
                return;
            }

            price = RoundPrice(price);

            if (safeRegime == true
                    && position.OpenOrders != null
                    && position.OpenOrders.Count > 0)
            {
                for (int i = 0; i < position.OpenOrders.Count; i++)
                {
                    if (position.OpenOrders[i].State == OrderStateType.Active)
                    {
                        Connector.OrderCancel(position.OpenOrders[i]);
                    }
                }
            }

            Order newOrder = new()
            {
                NumberUser = NumberGen.GetNumberOrder(StartProgram),

                Side = direction,
                Price = price,
                Volume = volume,
                TypeOrder = orderType,
                LifeTime = timeLife,
                PositionConditionType = OrderPositionConditionType.Open,
                SecurityNameCode = Security.Name,
                SecurityClassCode = Security.NameClass,
                OrderTypeTime = ManualPositionSupport.OrderTypeTime,
                ServerName = Connector.ServerFullName,
                IsStopOrProfit = isStopOrProfit
            };

            position.AddNewOpenOrder(newOrder);

            OnLogRecieved(Security.Name + " long position modification \n"
                    + "Order direction: " + Side.Buy.ToString() + "\n"
                    + "Price: " + price.ToString() + "\n"
                    + "Volume: " + volume.ToString() + "\n"
                    + "Position num: " + position.Number.ToString()
                    , LogMessageType.Trade);

            if (position.OpenOrders[0].SecurityNameCode.EndsWith(" TestPaper"))
            {
                Connector.OrderExecute(newOrder, true);
            }
            else
            {
                Connector.OrderExecute(newOrder);
            }
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    private void FakeExecute(Order order, DateTime timeExecute)
    {
        try
        {
            order.TimeCreate = timeExecute;
            order.TimeCallBack = timeExecute;

            Order newOrder = new()
            {
                NumberMarket = "fakeOrder " + NumberGen.GetNumberOrder(StartProgram),
                NumberUser = order.NumberUser,
                State = OrderStateType.Done,
                Volume = order.Volume,
                VolumeExecute = order.Volume,
                Price = order.Price,
                TypeOrder = order.TypeOrder,
                Side = order.Side,
                TimeCreate = timeExecute,
                TimeCallBack = timeExecute,
                OrderTypeTime = order.OrderTypeTime,
                SecurityNameCode = order.SecurityNameCode,
                PortfolioNumber = order.PortfolioNumber,
                ServerType = order.ServerType
            };

            _connector_OrderChangeEvent(newOrder);

            MyTrade trade = new()
            {
                Volume = order.Volume,
                Time = timeExecute,
                Price = order.Price,
                SecurityNameCode = order.SecurityNameCode,
                NumberTrade = "fakeTrade " + NumberGen.GetNumberOrder(StartProgram),
                Side = order.Side,
                NumberOrderParent = newOrder.NumberMarket
            };

            _connector_MyTradeEvent(trade);
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }




    public sealed class BuySellOperations
    {
        private readonly Side direction;
        private readonly SimpleBot bot;

        internal BuySellOperations(SimpleBot bot, Side side)
        {
            direction = side;
            this.bot = bot;
        }

        public Position Market(decimal volume, string signalType = "") =>
            bot.OpenMarket(direction, volume, signalType);

        public Position Limit(decimal volume, decimal priceLimit, string signalType = "") =>
            bot.OpenLimit(direction, volume, priceLimit, signalType);

        public Position Fake(decimal volume, decimal price, DateTime time, string signalType = "") =>
            bot.OpenFake(direction, volume, price, time, signalType);
    }


    PositionCreator _dealCreator = new();
}



/// <summary>
/// Create position
/// </summary>
public class PositionCreator
{
    /// <summary>
    /// Create position
    /// </summary>
    public static Position CreatePosition(string botName, Side direction, decimal priceOrder, 
            decimal volume, OrderPriceType priceType, TimeSpan timeLife,
            Security security, Portfolio portfolio, StartProgram startProgram, OrderTypeTime orderTypeTime)
    {
        decimal PortfolioValueOnOpenPosition;
        if(startProgram == StartProgram.IsOsTrader)
        {
            PortfolioValueOnOpenPosition = portfolio.ValueCurrent;
        }
        else
        {// Tester, Optimizer, etc
         // NOTE: Why?
            PortfolioValueOnOpenPosition = Math.Round(portfolio.ValueCurrent,2);
        }

        Position newDeal = new()
        {
            Number = NumberGen.GetNumberDeal(startProgram),

            Direction = direction,
            State = PositionStateType.Opening,
            NameBot = botName,
            Lots = security.Lot,
            PriceStepCost = security.PriceStepCost,
            PriceStep = security.PriceStep,
            PortfolioValueOnOpenPosition = PortfolioValueOnOpenPosition,
        };

        newDeal.AddNewOpenOrder(CreateOrder(security, direction, priceOrder, volume, 
                    priceType, timeLife, startProgram,OrderPositionConditionType.Open, orderTypeTime, portfolio.ServerUniqueName));
        newDeal.AddNewOpenOrder(
                new Order
                {
                    NumberUser = NumberGen.GetNumberOrder(startProgram),

                    Side = direction,
                    Price = priceOrder,
                    Volume = volume,
                    TypeOrder = priceType,
                    LifeTime = timeLife,
                    PositionConditionType = OrderPositionConditionType.Open,
                    SecurityNameCode = security.Name,
                    SecurityClassCode = security.NameClass,
                    OrderTypeTime = orderTypeTime,
                    ServerName = portfolio.ServerUniqueName,
                    PortfolioNumber = portfolio.Number,
                });

        return newDeal;
    }

    public static Order CreateOrder(Security security,
            Side direction, decimal priceOrder, decimal volume, 
            OrderPriceType priceType, TimeSpan timeLife, StartProgram startProgram,
            OrderPositionConditionType positionConditionType, OrderTypeTime orderTypeTime,
            string serverName)
    {
        return new Order
        {
            NumberUser = NumberGen.GetNumberOrder(startProgram),

            Side = direction,
            Price = priceOrder,
            Volume = volume,
            TypeOrder = priceType,
            LifeTime = timeLife,
            PositionConditionType = positionConditionType,
            SecurityNameCode = security.Name,
            SecurityClassCode = security.NameClass,
            OrderTypeTime = orderTypeTime,
            ServerName = serverName
        };
    }

    /// <summary>
    /// Create closing order
    /// </summary>
    public static Order CreateCloseOrderForDeal(Security security, Position deal, decimal price, 
            OrderPriceType priceType, TimeSpan timeLife, StartProgram startProgram, OrderTypeTime orderTypeTime, string serverName)
    {
        if (deal.OpenVolume == 0) { return null; }

        Order newOrder = new()
        {
            NumberUser = NumberGen.GetNumberOrder(startProgram),
            Side = deal.IsBuy ? Side.Sell : Side.Buy,
            Price = price,
            Volume = deal.OpenVolume,
            TypeOrder = priceType,
            LifeTime = timeLife,
            PositionConditionType = OrderPositionConditionType.Close,
            SecurityNameCode = security.Name,
            SecurityClassCode = security.NameClass,
            OrderTypeTime = orderTypeTime,
            ServerName = serverName
        };

        return newOrder;
    }
}
