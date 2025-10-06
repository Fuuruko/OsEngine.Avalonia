using System;
using System.Collections.Generic;
using System.Linq;
using OsEngine.Language;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;

namespace OsEngine.Models.Market.Servers.Tester;

public partial class TesterServer
{
    private List<Order> _activeOrders = [];

    private int _iteratorNumbersOrders;

    private int _iteratorNumbersMyTrades;

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

        for (int i = 0; i < _activeOrders.Count; i++)
        {
            if (_activeOrders[i].NumberUser == order.NumberUser)
            {
                SendLogMessage(OsLocalization.Market.Message39, LogMessageType.Error);
                FailedOperationOrder(order);
                return;
            }
        }
        // if (_activeOrders.Any(o => o.NumberUser == order.NumberUser))
        // {
        //     SendLogMessage(OsLocalization.Market.Message39, LogMessageType.Error);
        //     FailedOperationOrder(order);
        // }

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
        else if (string.IsNullOrWhiteSpace(order.PortfolioNumber))
        {
            errorMessage = OsLocalization.Market.Message43;
        }
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

        _activeOrders.Add(orderOnBoard);

        NewOrderIncomeEvent?.Invoke(orderOnBoard);

        if (!orderOnBoard.IsStopOrProfit) { return; }

        SecurityTester security = GetMySecurity(order);

        if (security.DataType == SecurityTesterDataType.Candle)
        { // testing with using candles / прогон на свечках
            if (CheckOrdersInCandleTest(orderOnBoard, security.LastCandle))
            {
                _activeOrders.Remove(orderOnBoard);
            }
        }
        else if (security.DataType == SecurityTesterDataType.Tick)
        {
            if (CheckOrdersInTickTest(orderOnBoard, security.LastTrade, true, security.IsNewDayTrade))
            {
                _activeOrders.Remove(orderOnBoard);
            }
        }
    }

    private void CheckOrders()
    {
        if (_activeOrders.Count == 0) { return; }

        CheckRejectOrdersOnClearing(_activeOrders, ServerTime);

        for (int i = 0; i < _activeOrders.Count; i++)
        {
            Order order = _activeOrders[i];
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
                    i--;
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
                    i--;
                }
            }
            else if (security.DataType == SecurityTesterDataType.MarketDepth)
            {
                // HERE!!!!!!!!!!!! / ЗДЕСЬ!!!!!!!!!!!!!!!!!!!!
                MarketDepth depth = security.LastMarketDepth;

                if (CheckOrdersInMarketDepthTest(order, depth))
                {
                    i--;
                }
            }
        }
    }

    private bool CheckOrdersInCandleTest(Order order, Candle lastCandle)
    {
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
            int slippage = 0;

            if (_slippageToStopOrder > 0)
            {
                slippage = _slippageToStopOrder;
            }

            decimal realPrice = order.Price;

            if (order.Side == Side.Buy)
            {
                if (minPrice > realPrice)
                {
                    realPrice = lastCandle.Open;
                }
            }
            if (order.Side == Side.Sell)
            {
                if (maxPrice < realPrice)
                {
                    realPrice = lastCandle.Open;
                }
            }

            ExecuteOnBoardOrder(order, realPrice, time, slippage);

            for (int i = 0; i < _activeOrders.Count; i++)
            {
                if (_activeOrders[i].NumberUser == order.NumberUser)
                {
                    _activeOrders.RemoveAt(i);
                    break;
                }
            }

            return true;
        }

        if (order.TypeOrder == OrderPriceType.Market)
        {
            if (order.TimeCreate >= lastCandle.TimeStart)
            {
                return false;
            }

            decimal realPrice = lastCandle.Open;

            ExecuteOnBoardOrder(order, realPrice, time, 0);

            for (int i = 0; i < _activeOrders.Count; i++)
            {
                if (_activeOrders[i].NumberUser == order.NumberUser)
                {
                    _activeOrders.RemoveAt(i);
                    break;
                }
            }

            return true;
        }

        // check whether the order passed / проверяем, прошёл ли ордер
        if (order.Side == Side.Buy)
        {
            if ((OrderExecutionType == OrderExecutionType.Intersection && order.Price > minPrice)
                    ||
                    (OrderExecutionType == OrderExecutionType.Touch && order.Price >= minPrice)
                    ||
                    (OrderExecutionType == OrderExecutionType.FiftyFifty &&
                     _lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Intersection &&
                     order.Price > minPrice)
                    ||
                    (OrderExecutionType == OrderExecutionType.FiftyFifty &&
                     _lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Touch &&
                     order.Price >= minPrice)
               )
            {// execute / исполняем

                decimal realPrice = order.Price;

                if (realPrice > openPrice && order.IsStopOrProfit == false)
                {
                    // if order is not quotation and put into the market / если заявка не котировачная и выставлена в рынок
                    realPrice = openPrice;
                }
                else if (order.IsStopOrProfit
                        && order.Price > maxPrice)
                {
                    realPrice = maxPrice;
                }

                int slippage = 0;

                if (order.IsStopOrProfit && _slippageToStopOrder > 0)
                {
                    slippage = _slippageToStopOrder;
                }
                else if (order.IsStopOrProfit == false && _slippageToSimpleOrder > 0)
                {
                    slippage = _slippageToSimpleOrder;
                }

                if (realPrice > maxPrice)
                {
                    realPrice = maxPrice;
                }

                ExecuteOnBoardOrder(order, realPrice, time, slippage);

                for (int i = 0; i < _activeOrders.Count; i++)
                {
                    if (_activeOrders[i].NumberUser == order.NumberUser)
                    {
                        _activeOrders.RemoveAt(i);
                        break;
                    }
                }

                if (OrderExecutionType == OrderExecutionType.FiftyFifty)
                {
                    if (_lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Touch)
                    { _lastOrderExecutionTypeInFiftyFiftyType = OrderExecutionType.Intersection; }
                    else
                    { _lastOrderExecutionTypeInFiftyFiftyType = OrderExecutionType.Touch; }
                }

                return true;
            }
        }

        if (order.Side == Side.Sell)
        {
            if ((OrderExecutionType == OrderExecutionType.Intersection && order.Price < maxPrice)
                    ||
                    (OrderExecutionType == OrderExecutionType.Touch && order.Price <= maxPrice)
                    ||
                    (OrderExecutionType == OrderExecutionType.FiftyFifty &&
                     _lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Intersection &&
                     order.Price < maxPrice)
                    ||
                    (OrderExecutionType == OrderExecutionType.FiftyFifty &&
                     _lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Touch &&
                     order.Price <= maxPrice)
               )
            {
                // execute / исполняем
                decimal realPrice = order.Price;

                if (realPrice < openPrice && order.IsStopOrProfit == false)
                {
                    // if order is not quotation and put into the market / если заявка не котировачная и выставлена в рынок
                    realPrice = openPrice;
                }
                else if (order.IsStopOrProfit && order.Price < minPrice)
                {
                    realPrice = minPrice;
                }

                int slippage = 0;
                if (order.IsStopOrProfit && _slippageToStopOrder > 0)
                {
                    slippage = _slippageToStopOrder;
                }
                else if (order.IsStopOrProfit == false && _slippageToSimpleOrder > 0)
                {
                    slippage = _slippageToSimpleOrder;
                }

                if (realPrice < minPrice)
                {
                    realPrice = minPrice;
                }

                ExecuteOnBoardOrder(order, realPrice, time, slippage);

                for (int i = 0; i < _activeOrders.Count; i++)
                {
                    if (_activeOrders[i].NumberUser == order.NumberUser)
                    {
                        _activeOrders.RemoveAt(i);
                        break;
                    }
                }

                if (OrderExecutionType == OrderExecutionType.FiftyFifty)
                {
                    if (_lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Touch)
                    { _lastOrderExecutionTypeInFiftyFiftyType = OrderExecutionType.Intersection; }
                    else
                    { _lastOrderExecutionTypeInFiftyFiftyType = OrderExecutionType.Touch; }
                }

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
        SecurityTester security = SecuritiesTester.Find(tester => tester.Security.Name == order.SecurityNameCode);

        if (security == null)
        {
            return false;
        }

        if (order.IsStopOrProfit)
        {
            int slippage = 0;
            if (_slippageToStopOrder > 0)
            {
                slippage = _slippageToStopOrder;
            }
            decimal realPrice = order.Price;

            if (isNewDay == true)
            {
                realPrice = lastTrade.Price;
            }

            ExecuteOnBoardOrder(order, realPrice, lastTrade.Time, slippage);

            for (int i = 0; i < _activeOrders.Count; i++)
            {
                if (_activeOrders[i].NumberUser == order.NumberUser)
                {
                    _activeOrders.RemoveAt(i);
                    break;
                }
            }

            return true;
        }

        if (order.TypeOrder == OrderPriceType.Market)
        {
            if (order.TimeCreate > lastTrade.Time)
            {
                return false;
            }

            int slippage = 0;
            if (_slippageToSimpleOrder > 0)
            {
                slippage = _slippageToSimpleOrder;
            }

            decimal realPrice = lastTrade.Price;

            ExecuteOnBoardOrder(order, realPrice, lastTrade.Time, slippage);

            for (int i = 0; i < _activeOrders.Count; i++)
            {
                if (_activeOrders[i].NumberUser == order.NumberUser)
                {
                    _activeOrders.RemoveAt(i);
                    break;
                }
            }

            return true;
        }

        // check whether the order passed/проверяем, прошёл ли ордер
        if (order.Side == Side.Buy)
        {
            if ((OrderExecutionType == OrderExecutionType.Intersection && order.Price > lastTrade.Price)
                    ||
                    (OrderExecutionType == OrderExecutionType.Touch && order.Price >= lastTrade.Price)
                    ||
                    (OrderExecutionType == OrderExecutionType.FiftyFifty &&
                     _lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Intersection &&
                     order.Price > lastTrade.Price)
                    ||
                    (OrderExecutionType == OrderExecutionType.FiftyFifty &&
                     _lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Touch &&
                     order.Price >= lastTrade.Price)
               )
            {// execute/исполняем
                int slippage = 0;

                if (order.IsStopOrProfit && _slippageToStopOrder > 0)
                {
                    slippage = _slippageToStopOrder;
                }
                else if (order.IsStopOrProfit == false && _slippageToSimpleOrder > 0)
                {
                    slippage = _slippageToSimpleOrder;
                }

                ExecuteOnBoardOrder(order, lastTrade.Price, ServerTime, slippage);

                for (int i = 0; i < _activeOrders.Count; i++)
                {
                    if (_activeOrders[i].NumberUser == order.NumberUser)
                    {
                        _activeOrders.RemoveAt(i);
                        break;
                    }
                }

                if (OrderExecutionType == OrderExecutionType.FiftyFifty)
                {
                    if (_lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Touch)
                    { _lastOrderExecutionTypeInFiftyFiftyType = OrderExecutionType.Intersection; }
                    else
                    { _lastOrderExecutionTypeInFiftyFiftyType = OrderExecutionType.Touch; }
                }

                return true;
            }
        }

        if (order.Side == Side.Sell)
        {
            if ((OrderExecutionType == OrderExecutionType.Intersection && order.Price < lastTrade.Price)
                    ||
                    (OrderExecutionType == OrderExecutionType.Touch && order.Price <= lastTrade.Price)
                    ||
                    (OrderExecutionType == OrderExecutionType.FiftyFifty &&
                     _lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Intersection &&
                     order.Price < lastTrade.Price)
                    ||
                    (OrderExecutionType == OrderExecutionType.FiftyFifty &&
                     _lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Touch &&
                     order.Price <= lastTrade.Price)
               )
            {// execute/исполняем
                int slippage = 0;

                if (order.IsStopOrProfit && _slippageToStopOrder > 0)
                {
                    slippage = _slippageToStopOrder;
                }
                else if (order.IsStopOrProfit == false && _slippageToSimpleOrder > 0)
                {
                    slippage = _slippageToSimpleOrder;
                }

                ExecuteOnBoardOrder(order, lastTrade.Price, ServerTime, slippage);

                for (int i = 0; i < _activeOrders.Count; i++)
                {
                    if (_activeOrders[i].NumberUser == order.NumberUser)
                    {
                        _activeOrders.RemoveAt(i);
                        break;
                    }
                }

                if (OrderExecutionType == OrderExecutionType.FiftyFifty)
                {
                    if (_lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Touch)
                    { _lastOrderExecutionTypeInFiftyFiftyType = OrderExecutionType.Intersection; }
                    else
                    { _lastOrderExecutionTypeInFiftyFiftyType = OrderExecutionType.Touch; }
                }

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
        if (lastMarketDepth == null)
        {
            return false;
        }
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
            int slippage = 0;
            if (_slippageToStopOrder > 0)
            {
                slippage = _slippageToStopOrder;
            }

            decimal realPrice = order.Price;
            ExecuteOnBoardOrder(order, realPrice, time, slippage);

            for (int i = 0; i < _activeOrders.Count; i++)
            {
                if (_activeOrders[i].NumberUser == order.NumberUser)
                {
                    _activeOrders.RemoveAt(i);
                    break;
                }
            }

            return true;
        }

        if (order.TypeOrder == OrderPriceType.Market)
        {
            if (order.TimeCreate >= lastMarketDepth.Time)
            {
                return false;
            }

            decimal realPrice = 0;

            if (order.Side == Side.Buy)
            {
                realPrice = sellBestPrice;
            }
            else //if(order.Side == Side.Sell)
            {
                realPrice = buyBestPrice;
            }

            int slippage = 0;
            if (_slippageToSimpleOrder > 0)
            {
                slippage = _slippageToSimpleOrder;
            }

            ExecuteOnBoardOrder(order, realPrice, lastMarketDepth.Time, slippage);

            for (int i = 0; i < _activeOrders.Count; i++)
            {
                if (_activeOrders[i].NumberUser == order.NumberUser)
                {
                    _activeOrders.RemoveAt(i);
                    break;
                }
            }

            return true;
        }

        // check whether the order passed / проверяем, прошёл ли ордер
        if (order.Side == Side.Buy)
        {
            if ((OrderExecutionType == OrderExecutionType.Intersection && order.Price > sellBestPrice)
                    ||
                    (OrderExecutionType == OrderExecutionType.Touch && order.Price >= sellBestPrice)
                    ||
                    (OrderExecutionType == OrderExecutionType.FiftyFifty &&
                     _lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Intersection &&
                     order.Price > sellBestPrice)
                    ||
                    (OrderExecutionType == OrderExecutionType.FiftyFifty &&
                     _lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Touch &&
                     order.Price >= sellBestPrice)
               )
            {
                decimal realPrice = order.Price;

                if (realPrice > sellBestPrice)
                {
                    realPrice = sellBestPrice;
                }

                int slippage = 0;

                if (order.IsStopOrProfit && _slippageToStopOrder > 0)
                {
                    slippage = _slippageToStopOrder;
                }
                else if (order.IsStopOrProfit == false && _slippageToSimpleOrder > 0)
                {
                    slippage = _slippageToSimpleOrder;
                }

                ExecuteOnBoardOrder(order, realPrice, time, slippage);

                for (int i = 0; i < _activeOrders.Count; i++)
                {
                    if (_activeOrders[i].NumberUser == order.NumberUser)
                    {
                        _activeOrders.RemoveAt(i);
                        break;
                    }
                }

                if (OrderExecutionType == OrderExecutionType.FiftyFifty)
                {
                    if (_lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Touch)
                    { _lastOrderExecutionTypeInFiftyFiftyType = OrderExecutionType.Intersection; }
                    else
                    { _lastOrderExecutionTypeInFiftyFiftyType = OrderExecutionType.Touch; }
                }
                return true;
            }
        }

        if (order.Side == Side.Sell)
        {
            if ((OrderExecutionType == OrderExecutionType.Intersection && order.Price < buyBestPrice)
                    ||
                    (OrderExecutionType == OrderExecutionType.Touch && order.Price <= buyBestPrice)
                    ||
                    (OrderExecutionType == OrderExecutionType.FiftyFifty &&
                     _lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Intersection &&
                     order.Price < buyBestPrice)
                    ||
                    (OrderExecutionType == OrderExecutionType.FiftyFifty &&
                     _lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Touch &&
                     order.Price <= buyBestPrice)
               )
            {
                // execute / исполняем
                decimal realPrice = order.Price;

                if (realPrice < buyBestPrice)
                {
                    realPrice = buyBestPrice;
                }

                int slippage = 0;

                if (order.IsStopOrProfit && _slippageToStopOrder > 0)
                {
                    slippage = _slippageToStopOrder;
                }
                else if (order.IsStopOrProfit == false && _slippageToSimpleOrder > 0)
                {
                    slippage = _slippageToSimpleOrder;
                }

                ExecuteOnBoardOrder(order, realPrice, time, slippage);

                for (int i = 0; i < _activeOrders.Count; i++)
                {
                    if (_activeOrders[i].NumberUser == order.NumberUser)
                    {
                        _activeOrders.RemoveAt(i);
                        break;
                    }
                }

                if (OrderExecutionType == OrderExecutionType.FiftyFifty)
                {
                    if (_lastOrderExecutionTypeInFiftyFiftyType == OrderExecutionType.Touch)
                    { _lastOrderExecutionTypeInFiftyFiftyType = OrderExecutionType.Intersection; }
                    else
                    { _lastOrderExecutionTypeInFiftyFiftyType = OrderExecutionType.Touch; }
                }
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
            if (NonTradePeriods[i].IsOn == false)
            {
                continue;
            }

            DateTime timeStart = NonTradePeriods[i].StartDate;
            DateTime timeEnd = NonTradePeriods[i].EndDate;

            if (order.TimeCreate > timeStart
                    && order.TimeCreate < timeEnd)
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
        Order orderToClose = null;

        if (_activeOrders.Count != 0)
        {
            for (int i = 0; i < _activeOrders.Count; i++)
            {
                if (_activeOrders[i].NumberUser == order.NumberUser)
                {
                    orderToClose = _activeOrders[i];
                }
            }
        }

        if (orderToClose == null)
        {
            SendLogMessage(OsLocalization.Market.Message46, LogMessageType.Error);
            FailedOperationOrder(order);
            return;
        }

        for (int i = 0; i < _activeOrders.Count; i++)
        {
            if (_activeOrders[i].NumberUser == order.NumberUser)
            {
                _activeOrders.RemoveAt(i);
                break;
            }
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
        decimal realPrice = price;

        if (order.Volume == order.VolumeExecute ||
                order.State == OrderStateType.Done)
        {
            return;
        }


        if (slippage != 0)
        {
            Security mySecurity = GetSecurityForName(order.SecurityNameCode, "");
            if (mySecurity != null && mySecurity.PriceStep != 0)
            {
                if (order.Side == Side.Buy)
                {
                    realPrice += mySecurity.PriceStep * slippage;
                }
                else
                {
                    realPrice -= mySecurity.PriceStep * slippage;
                }
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

    private DateTime _lastCheckSessionOrdersTime;

    private DateTime _lastCheckDayOrdersTime;

    private void CheckRejectOrdersOnClearing(List<Order> orders, DateTime timeOnMarket)
    {
        if (orders.Count == 0) { return; }

        List<Order> dayLifeOrders = [];

        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i].OrderTypeTime == OrderTypeTime.Day)
            {
                dayLifeOrders.Add(orders[i]);
            }
        }

        if (ClearingTimes.Count != 0)
        {
            CheckOrderBySessionLife(dayLifeOrders, timeOnMarket);
        }
        else
        {
            CheckOrderByDayLife(dayLifeOrders, timeOnMarket);
        }
    }

    private void CheckOrderBySessionLife(List<Order> orders, DateTime timeOnMarket)
    {
        if (ClearingTimes.Count == 0
                || orders.Count == 0)
        {
            _lastCheckSessionOrdersTime = timeOnMarket;
            return;
        }

        for (int i = 0; i < ClearingTimes.Count; i++)
        {
            if (ClearingTimes[i].IsOn == false) { continue; }

            if (_lastCheckSessionOrdersTime.TimeOfDay < ClearingTimes[i].Time
                    &&
                    timeOnMarket.TimeOfDay >= ClearingTimes[i].Time)
            {
                Order[] ordersToCancel = orders.ToArray();

                for (int j = 0; j < ordersToCancel.Length; j++)
                {
                    CancelOnBoardOrder(ordersToCancel[j]);
                }

                _lastCheckSessionOrdersTime = timeOnMarket;
                return;
            }
        }

        _lastCheckSessionOrdersTime = timeOnMarket;
    }

    private void CheckOrderByDayLife(List<Order> orders, DateTime timeOnMarket)
    {
        if (orders.Count == 0
                || _lastCheckDayOrdersTime == DateTime.MinValue
                || _lastCheckDayOrdersTime.Date == timeOnMarket.Date)
        {
            _lastCheckDayOrdersTime = timeOnMarket;
            return;
        }

        Order[] ordersToCancel = orders.ToArray();

        for (int j = 0; j < ordersToCancel.Length; j++)
        {
            CancelOnBoardOrder(ordersToCancel[j]);
        }

        _lastCheckDayOrdersTime = timeOnMarket;
    }

    public event Action<Order> NewOrderIncomeEvent;

    public event Action<Order> CancelOrderFailEvent;
}
