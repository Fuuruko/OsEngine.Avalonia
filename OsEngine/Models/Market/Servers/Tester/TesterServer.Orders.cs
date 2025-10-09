using System;
using System.Collections.Generic;
using System.Linq;
using OsEngine.Language;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;

namespace OsEngine.Models.Market.Servers.Tester;

public partial class TesterServer
{
    // NOTE: ObservableDictonary?
    private Dictionary<int, Order> _activeOrders = [];
    private Queue<Order> _dayLifeOrders = [];

    private int _iteratorNumbersOrders;

    private int _iteratorNumbersMyTrades;

    private DateTime _lastCheckSessionOrdersTime;

    public void ExecuteOrder(Order order)
    {
        order.TimeCreate = ServerTime;

        if (order.PositionConditionType == OrderPositionConditionType.Open
                && OrderCanExecuteByNonTradePeriods(order) == false)
        {
            SendLogMessage("No trading period. Open order cancel", LogMessageType.System);
            FailedOperationOrder(order);
            return;
        }

        if (_activeOrders.ContainsKey(order.NumberUser))
        {
            SendLogMessage(OsLocalization.Market.Message39, LogMessageType.Error);
            FailedOperationOrder(order);
            return;
        }

        if (ServerStatus == ServerConnectStatus.Disconnect)
        {
            SendLogMessage(OsLocalization.Market.Message40, LogMessageType.Error);
            FailedOperationOrder(order);
            return;
        }

        string errorMessage = null;
        if (order.Volume <= 0)
        {
            errorMessage = OsLocalization.Market.Message42 + order.Volume;
        }
        // NOTE: Is it possible that it will not set?
        else if (string.IsNullOrWhiteSpace(order.PortfolioNumber))
        {
            errorMessage = OsLocalization.Market.Message43;
        }
        // NOTE: Is it possible that it will not set?
        else if (string.IsNullOrWhiteSpace(order.SecurityNameCode))
        {
            errorMessage = OsLocalization.Market.Message44;
        }

        if (errorMessage != null)
        {
            SendLogMessage(errorMessage, LogMessageType.Error);
            FailedOperationOrder(order);
            return;
        }

        // NOTE: why make copy?
        Order orderOnBoard = new()
        {
            NumberMarket = _iteratorNumbersOrders++.ToString(),
            NumberUser = order.NumberUser,
            PortfolioNumber = order.PortfolioNumber,
            Price = order.Price,
            SecurityNameCode = order.SecurityNameCode,
            Side = order.Side,
            State = OrderStateType.Active,
            ServerType = order.ServerType,
            TimeCallBack = ServerTime,
            TimeCreate = ServerTime,
            TypeOrder = order.TypeOrder,
            Volume = order.Volume,
            Comment = order.Comment,
            LifeTime = order.LifeTime,
            IsStopOrProfit = order.IsStopOrProfit,
            TimeFrameInTester = order.TimeFrameInTester,
            OrderTypeTime = order.OrderTypeTime
        };

        _activeOrders.Add(orderOnBoard.NumberUser, orderOnBoard);
        if (orderOnBoard.OrderTypeTime == OrderTypeTime.Day)
        {
            _dayLifeOrders.Enqueue(orderOnBoard);
        }

        NewOrderIncomeEvent?.Invoke(orderOnBoard);

        if (!orderOnBoard.IsStopOrProfit) { return; }

        SecurityTester security = GetMySecurity(order);

        if (security.DataType == SecurityTesterDataType.Candle)
        { // testing with using candles / прогон на свечках
            if (CheckOrdersInCandleTest(orderOnBoard, security.LastCandle))
            {
                _activeOrders.Remove(orderOnBoard.NumberUser);
            }
        }
        else if (security.DataType == SecurityTesterDataType.Tick)
        {
            if (CheckOrdersInTickTest(orderOnBoard, security.LastTrade, true, security.IsNewDayTrade))
            {
                _activeOrders.Remove(orderOnBoard.NumberUser);
            }
        }
        // NOTE: Why there is no else if for MarketDepth?
    }

    private void CheckOrders()
    {
        if (_activeOrders.Count == 0) { return; }

        CheckRejectOrdersOnClearing(ServerTime);

        foreach (Order order in _activeOrders.Values)
        {
            // check availability of securities on the market / проверяем наличие инструмента на рынке

            SecurityTester security = GetMySecurity(order);

            if (security == null) { continue; }

            if (security.DataType == SecurityTesterDataType.Tick)
            { // test with using ticks / прогон на тиках

                List<Trade> lastTrades = security.LastTradeSeries;

                if (lastTrades != null
                        && lastTrades.Count != 0
                        && CheckOrdersInTickTest(order, lastTrades[^1], false, security.IsNewDayTrade))
                {
                    _activeOrders.Remove(order.NumberUser);
                    // NOTE: Why break?
                    break;
                }
            }
            else if (security.DataType == SecurityTesterDataType.Candle)
            { // test with using candles / прогон на свечках
                Candle lastCandle = security.LastCandle;

                if (order.Price == 0)
                {
                    order.Price = lastCandle.Open;
                }

                if (CheckOrdersInCandleTest(order, lastCandle))
                {
                    _activeOrders.Remove(order.NumberUser);
                }
            }
            else if (security.DataType == SecurityTesterDataType.MarketDepth)
            {
                // HERE!!!!!!!!!!!! / ЗДЕСЬ!!!!!!!!!!!!!!!!!!!!
                MarketDepth depth = security.LastMarketDepth;

                if (CheckOrdersInMarketDepthTest(order, depth))
                {
                    _activeOrders.Remove(order.NumberUser);
                }
            }
        }
    }

    private bool CheckOrdersInCandleTest(Order order, Candle lastCandle)
    {
        // NOTE: Why minPrice is max and maxPrice is 0?
        decimal minPrice = decimal.MaxValue;
        decimal maxPrice = 0;
        decimal openPrice = 0;
        DateTime time = ServerTime;

        if (lastCandle != null)
        {
            minPrice = lastCandle.Low;
            maxPrice = lastCandle.High;
            openPrice = lastCandle.Open;
            time = lastCandle.TimeStart;
        }

        if (time <= order.TimeCallBack
                && order.IsStopOrProfit != true)
        {
            //CanselOnBoardOrder(order);
            return false;
        }

        if (order.IsStopOrProfit)
        {
            decimal realPrice = order.Price;
            int slippage;

            if (order.Side == Side.Buy)
            {
                slippage = _slippageToStopOrder;
                if (minPrice > realPrice)
                {
                    realPrice = lastCandle.Open;
                }
            }
            else
            {
                slippage = -_slippageToStopOrder;
                if (maxPrice < realPrice)
                {
                    realPrice = lastCandle.Open;
                }
            }

            ExecuteOnBoardOrder(order, realPrice, time, slippage);

            return true;
        }

        if (order.IsMarket)
        {
            if (order.TimeCreate >= lastCandle.TimeStart) { return false; }

            decimal realPrice = lastCandle.Open;

            // NOTE: Why slippage is 0?
            ExecuteOnBoardOrder(order, realPrice, time, 0);

            return true;
        }

        // check whether the order passed / проверяем, прошёл ли ордер
        if (order.IsBuy)
        {
            if ((OrderExecutionType == OrderExecutionType.Intersection && order.Price > minPrice)
                || (OrderExecutionType == OrderExecutionType.Touch && order.Price >= minPrice))
            {

                decimal realPrice = order.Price;

                if (realPrice > openPrice)
                {
                    // if order is not quotation and put into the market / если заявка не котировачная и выставлена в рынок
                    realPrice = openPrice;
                }

                ExecuteOnBoardOrder(order, realPrice, time, _slippageToSimpleOrder);

                return true;
            }
        }
        else
        {
            if ((OrderExecutionType == OrderExecutionType.Intersection
                 && order.Price < maxPrice)
                || (OrderExecutionType == OrderExecutionType.Touch
                    && order.Price <= maxPrice))
            {
                decimal realPrice = order.Price;

                if (realPrice < openPrice)
                {
                    // if order is not quotation and put into the market / если заявка не котировачная и выставлена в рынок
                    realPrice = openPrice;
                }

                ExecuteOnBoardOrder(order, realPrice, time, -_slippageToSimpleOrder);

                return true;
            }
        }

        // order didn't execute. check if it's time to recall / ордер не `исполнился. проверяем, не пора ли отзывать

        if (order.OrderTypeTime == OrderTypeTime.Specified
                && order.TimeCallBack.Add(order.LifeTime) <= ServerTime)
        {
            CancelOnBoardOrder(order);
            return true;
        }

        return false;
    }

    private bool CheckOrdersInTickTest(Order order, Trade lastTrade, bool firstTime, bool isNewDay)
    {
        if (order.IsStopOrProfit)
        {
            int slippage = order.IsBuy ? _slippageToStopOrder : -_slippageToStopOrder;
            decimal realPrice = order.Price;
            if (isNewDay == true)
            {
                realPrice = lastTrade.Price;
            }

            ExecuteOnBoardOrder(order, realPrice, lastTrade.Time, slippage);

            return true;
        }

        if (order.IsMarket)
        {
            if (order.TimeCreate > lastTrade.Time) { return false; }

            decimal realPrice = lastTrade.Price;
            int slippage = order.IsBuy ? _slippageToSimpleOrder : -_slippageToSimpleOrder;

            ExecuteOnBoardOrder(order, realPrice, lastTrade.Time, slippage);

            return true;
        }

        // check whether the order passed/проверяем, прошёл ли ордер
        if (order.IsBuy)
        {
            if ((OrderExecutionType == OrderExecutionType.Intersection && order.Price > lastTrade.Price)
                || (OrderExecutionType == OrderExecutionType.Touch && order.Price >= lastTrade.Price))
            {
                decimal realPrice = order.Price;

                if (isNewDay == true)
                {
                    realPrice = lastTrade.Price;
                }

                ExecuteOnBoardOrder(order, realPrice, lastTrade.Time, _slippageToSimpleOrder);

                return true;
            }
        }
        else
        {
            if ((OrderExecutionType == OrderExecutionType.Intersection && order.Price < lastTrade.Price)
                || (OrderExecutionType == OrderExecutionType.Touch && order.Price <= lastTrade.Price))
            {// execute/исполняем

                decimal realPrice = order.Price;
                if (isNewDay == true)
                {
                    realPrice = lastTrade.Price;
                }

                ExecuteOnBoardOrder(order, realPrice, ServerTime, -_slippageToSimpleOrder);

                return true;
            }
        }

        // order is not executed. check if it's time to recall / ордер не исполнился. проверяем, не пора ли отзывать

        if (order.OrderTypeTime == OrderTypeTime.Specified)
        {
            if (order.TimeCallBack.Add(order.LifeTime) <= ServerTime)
            {
                CancelOnBoardOrder(order);
                return true;
            }
        }
        return false;
    }

    private bool CheckOrdersInMarketDepthTest(Order order, MarketDepth lastMarketDepth)
    {
        if (lastMarketDepth == null) { return false; }

        decimal sellBestPrice = lastMarketDepth.Asks[0].Price;
        decimal buyBestPrice = lastMarketDepth.Bids[0].Price;

        DateTime time = lastMarketDepth.Time;

        if (time <= order.TimeCallBack && !order.IsStopOrProfit)
        {
            //CanselOnBoardOrder(order);
            return false;
        }

        if (order.IsStopOrProfit)
        {
            int slippage = order.IsBuy ? _slippageToStopOrder : -_slippageToStopOrder;
            ExecuteOnBoardOrder(order, order.Price, time, slippage);
            return true;
        }

        if (order.IsMarket)
        {
            if (order.TimeCreate >= lastMarketDepth.Time) { return false; }

            decimal realPrice;
            int slippage;
            if (order.IsBuy)
            {
                slippage = _slippageToStopOrder;
                realPrice = sellBestPrice;
            }
            else //if(order.Side == Side.Sell)
            {
                slippage = -_slippageToStopOrder;
                realPrice = buyBestPrice;
            }

            ExecuteOnBoardOrder(order, realPrice, lastMarketDepth.Time, slippage);

            return true;
        }

        // check whether the order passed / проверяем, прошёл ли ордер
        if (order.IsBuy)
        {
            if ((OrderExecutionType == OrderExecutionType.Intersection && order.Price > sellBestPrice)
                 || (OrderExecutionType == OrderExecutionType.Touch && order.Price >= sellBestPrice))
            {
                ExecuteOnBoardOrder(order, sellBestPrice, time, _slippageToSimpleOrder);

                return true;
            }
        }
        else
        {
            if ((OrderExecutionType == OrderExecutionType.Intersection && order.Price < buyBestPrice)
                 || (OrderExecutionType == OrderExecutionType.Touch && order.Price <= buyBestPrice))
            {
                ExecuteOnBoardOrder(order, buyBestPrice, time, -_slippageToSimpleOrder);

                return true;
            }
        }

        // order didn't execute. check if it's time to recall / ордер не `исполнился. проверяем, не пора ли отзывать

        if (order.OrderTypeTime == OrderTypeTime.Specified)
        {
            if (order.TimeCallBack.Add(order.LifeTime) <= ServerTime)
            {
                CancelOnBoardOrder(order);
                return true;
            }
        }
        return false;
    }

    public bool OrderCanExecuteByNonTradePeriods(Order order)
    {
        if (NonTradePeriods.Count == 0) { return true; }

        for (int i = 0; i < NonTradePeriods.Count; i++)
        {
            DateTime timeStart = NonTradePeriods[i].StartDate;
            DateTime timeEnd = NonTradePeriods[i].EndDate;

            if (timeStart < order.TimeCreate && order.TimeCreate < timeEnd)
            {
                return false;
            }
        }

        return true;
    }

    public void CancelOrder(Order order)
    {
        if (ServerStatus == ServerConnectStatus.Disconnect)
        {
            SendLogMessage(OsLocalization.Market.Message45, LogMessageType.Error);
            FailedOperationOrder(order);
            return;
        }

        CancelOnBoardOrder(order);
    }

    private void CancelOnBoardOrder(Order order)
    {
        _activeOrders.Remove(order.NumberUser, out Order orderToClose);

        if (orderToClose == null)
        {
            SendLogMessage(OsLocalization.Market.Message46, LogMessageType.Error);
            FailedOperationOrder(order);
            return;
        }

        orderToClose.State = OrderStateType.Cancel;

        NewOrderIncomeEvent?.Invoke(orderToClose);
    }

    private void FailedOperationOrder(Order order)
    {
        Order orderOnBoard = new()
        {
            NumberMarket = _iteratorNumbersOrders++.ToString(),
            NumberUser = order.NumberUser,
            PortfolioNumber = order.PortfolioNumber,
            Price = order.Price,
            SecurityNameCode = order.SecurityNameCode,
            Side = order.Side,
            State = OrderStateType.Fail,
            TimeCallBack = ServerTime,
            TimeCreate = order.TimeCreate,
            TypeOrder = order.TypeOrder,
            Volume = order.Volume,
            Comment = order.Comment,
            ServerType = order.ServerType
        };

        NewOrderIncomeEvent?.Invoke(orderOnBoard);
    }

    private void ExecuteOnBoardOrder(Order order, decimal price, DateTime time, int slippage)
    {
        if (order.Volume == order.VolumeExecute ||
                order.State == OrderStateType.Done)
        {
            return;
        }

        decimal realPrice = price;

        if (slippage != 0)
        {
            Security mySecurity = GetSecurityForName(order.SecurityNameCode, "");
            if (mySecurity != null && mySecurity.PriceStep != 0)
            {
                realPrice += mySecurity.PriceStep * slippage;
            }
        }

        MyTrade trade = new()
        {
            NumberOrderParent = order.NumberMarket,
            NumberTrade = _iteratorNumbersMyTrades++.ToString(),
            SecurityNameCode = order.SecurityNameCode,
            Volume = order.Volume,
            Time = time,
            Price = realPrice,
            Side = order.Side
        };

        MyTrades.Add(trade);
        NewMyTradeEvent?.Invoke(trade);

        order.State = OrderStateType.Done;
        order.Price = realPrice;

        NewOrderIncomeEvent?.Invoke(order);

        ChangePosition(order);

        CheckWaitOrdersRegime();
    }

    private void CheckRejectOrdersOnClearing(DateTime timeOnMarket)
    {
        if (_dayLifeOrders.Count == 0)
        {
            _lastCheckSessionOrdersTime = timeOnMarket;
            return;
        }

        if (ClearingTimes.Count != 0
                && CheckOrderBySessionLife(timeOnMarket.TimeOfDay)
                || CheckOrderByDayLife(timeOnMarket))
        {
            int count = _dayLifeOrders.Count;
            for (int j = 0; j < count; j++)
            {
                CancelOnBoardOrder(_dayLifeOrders.Dequeue());
            }
        }
        _lastCheckSessionOrdersTime = timeOnMarket;
    }

    private bool CheckOrderBySessionLife(TimeSpan timeOnMarket)
    {
        TimeSpan lastCheck = _lastCheckSessionOrdersTime.TimeOfDay;
        for (int i = 0; i < ClearingTimes.Count; i++)
        {
            if (lastCheck < ClearingTimes[i].Time
                && timeOnMarket >= ClearingTimes[i].Time)
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckOrderByDayLife(DateTime timeOnMarket)
    {
        return _lastCheckSessionOrdersTime.Date != timeOnMarket.Date;
    }

    public event Action<Order> NewOrderIncomeEvent;

    public event Action<Order> CancelOrderFailEvent;
}
