/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Market.Connectors;

public class OrderExecutionEmulator
{
    // NOTE: Class should be disposable
    // so emulator can be removed from _emulators
    private static readonly List<OrderExecutionEmulator> _emulators = [];

    private static async void WatcherThread()
    {
        while (true)
        {
            try
            {
                await Task.Delay(250);

                if (MainWindow.ProccesIsWorked == false)
                {
                    return;
                }

                for (int i = 0; i < _emulators.Count; i++)
                {
                    if (_emulators[i] == null)
                    {
                        continue;
                    }
                    _emulators[i].CheckOrders();
                }
            }
            catch (Exception e)
            {
                // FIX:
                // ServerMaster.SendNewLogMessage(e.ToString(),Logging.LogMessageType.Error);
                await Task.Delay(2000);
            }
        }
    }

    private decimal _bestBuy;

    private decimal _bestSell;

    private DateTime _serverTime;

    private readonly ConcurrentQueue<Order> _ordersToSend = new();

    private readonly ConcurrentQueue<MyTrade> _myTradesToSend = new();

    // private List<Order> ordersOnBoard = [];
    // NOTE: Replace List with ConcurrentDictionary
    // and remove lock's and search to delete element
    private readonly Dictionary<int, Order> ordersOnBoard2 = [];

    // NOTE: Not sure but maybe it not needed
    private readonly Lock _lock = new();


    public OrderExecutionEmulator()
    {
        _emulators.Add(this);

        if (_emulators.Count == 1)
        {
            Task.Run(WatcherThread);
        }
    }

    /// <summary>
    /// place an order on the virtual exchange
    /// </summary>
    public void OrderExecute(Order order)
    {
        if (order.SecurityNameCode != null)
        {
            order.SecurityNameCode += " TestPaper";
        }
        else
        {
            order.SecurityNameCode = "TestPaper";
        }

        order.PortfolioNumber = "Emulator";

        ActivateSimple(order);

        lock (_lock)
        {
            // ordersOnBoard.Add(order);
            ordersOnBoard2.Add(order.NumberUser, order);
        }

        CheckExecution(true, order);
    }

    /// <summary>
    /// change the order price on the virtual exchange
    /// </summary>
    /// <param name="order">an order that will have a new price</param>
    /// <param name="newPrice">new price</param>
    public bool ChangeOrderPrice(Order order, decimal newPrice)
    {
        lock (_lock)
        {
            if (ordersOnBoard2.TryGetValue(order.NumberUser, out Order o))
            {
                o.Price = newPrice;
                order.Price = newPrice;
                return true;
            }
            return false;
            // foreach (Order o in ordersOnBoard)
            // {
            //     if (o.NumberUser == order.NumberUser)
            //     {
            //         o.Price = newPrice;
            //         order.Price = newPrice;
            //         return true;
            //     }
            // }
            //
            // return false;
        }
    }

    /// <summary>
    /// cancel an order on a virtual exchange
    /// </summary>
    public void OrderCancel(Order order)
    {
        lock (_lock)
        {
            ordersOnBoard2.Remove(order.NumberUser);
            // for (int i = 0; i < ordersOnBoard.Count; i++)
            // {
            //     if (ordersOnBoard[i].NumberUser == order.NumberUser)
            //     {
            //         ordersOnBoard.RemoveAt(i);
            //         break;
            //     }
            // }
        }

        Order newOrder = new()
        {
            PortfolioNumber = "Emulator",
            ServerType = order.ServerType,
            NumberMarket = order.NumberMarket,
            NumberUser = order.NumberUser,
            State = OrderStateType.Cancel,
            Volume = order.Volume,
            VolumeExecute = order.VolumeExecute,
            Price = order.Price,
            TypeOrder = order.TypeOrder,
            OrderTypeTime = order.OrderTypeTime,

            TimeCreate = order.TimeCreate
        };

        if (string.IsNullOrEmpty(order.SecurityNameCode) == false
            && order.SecurityNameCode.EndsWith(" TestPaper") == false)
        {
            newOrder.SecurityNameCode = order.SecurityNameCode + " TestPaper";
        }
        else
        {
            newOrder.SecurityNameCode = order.SecurityNameCode;
        }

        if (_serverTime > newOrder.TimeCreate)
        {
            newOrder.TimeCallBack = _serverTime;
        }
        else
        {
            newOrder.TimeCallBack = newOrder.TimeCreate;
        }

        newOrder.TimeCancel = newOrder.TimeCallBack;

        _ordersToSend.Enqueue(newOrder);
    }

    // NOTE: Why not just Invoke event directly
    private void CheckOrders()
    {
        while (!_ordersToSend.IsEmpty)
        {

            _ordersToSend.TryDequeue(out Order order);

            if (order == null) { continue; }

            OrderChangeEvent?.Invoke(order);
        }

        while (!_myTradesToSend.IsEmpty)
        {

            _myTradesToSend.TryDequeue(out MyTrade trade);

            if (trade == null) { continue; }

            MyTradeEvent?.Invoke(trade);
        }
    }

    private bool CheckExecution(bool isFirstTime, Order order)
    {
        if (order.NumberMarket == "" || order.Side == Side.None) { return false; }

        lock (_lock)
        {

            decimal price;
            // NOTE: What if order is Iceberg?
            if (order.Side == Side.Buy)
            {
                if (order.TypeOrder == OrderPriceType.Market)
                {
                    price = _bestSell == 0 ? order.Price : _bestSell;
                }
                else if (order.Price >= _bestSell && _bestSell != 0)
                {
                    price = isFirstTime ? _bestSell : order.Price;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (order.TypeOrder == OrderPriceType.Market)
                {
                    price = _bestBuy == 0 ? order.Price : _bestBuy;
                }
                else if (order.Price <= _bestBuy && _bestBuy != 0)
                {
                    price = isFirstTime ? _bestBuy : order.Price;
                }
                else
                {
                    return false;
                }
            }


            ExecuteSimple(order, price);

            ordersOnBoard2.Remove(order.NumberUser);

            // for (int i = 0; i < ordersOnBoard.Count; i++)
            // {
            //     if (ordersOnBoard[i].NumberUser == order.NumberUser)
            //     {
            //         ordersOnBoard.RemoveAt(i);
            //         break;
            //     }
            // }

            // return true;



            // if (order.TypeOrder == OrderPriceType.Market)
            // {
            //     if (order.Side == Side.Buy)
            //     {
            //         decimal price = _bestSell;
            //
            //         if (price == 0)
            //         {
            //             price = order.Price;
            //         }
            //
            //         ExecuteSimple(order, price);
            //
            //         for (int i = 0; i < ordersOnBoard.Count; i++)
            //         {
            //             if (ordersOnBoard[i].NumberUser == order.NumberUser)
            //             {
            //                 ordersOnBoard.RemoveAt(i);
            //                 break;
            //             }
            //         }
            //
            //         return true;
            //     }
            //     else if (order.Side == Side.Sell)
            //     {
            //         decimal price = _bestBuy;
            //
            //         if (price == 0)
            //         {
            //             price = order.Price;
            //         }
            //
            //         ExecuteSimple(order, price);
            //
            //         for (int i = 0; i < ordersOnBoard.Count; i++)
            //         {
            //             if (ordersOnBoard[i].NumberUser == order.NumberUser)
            //             {
            //                 ordersOnBoard.RemoveAt(i);
            //                 break;
            //             }
            //         }
            //
            //         return true;
            //     }
            // }
            // else //if (order.TypeOrder == OrderPriceType.Limit)
            // {
            //     decimal price;
            //     if (order.Side == Side.Buy &&
            //             order.Price >= _bestSell && _bestSell != 0)
            //     {
            //         price = isFirstTime ? _bestSell : order.Price;
            //     }
            //     else if (order.Side == Side.Sell &&
            //             order.Price <= _bestBuy && _bestBuy != 0)
            //     {
            //         price = isFirstTime ? _bestBuy : order.Price;
            //     }
            //     else
            //     {
            //         return false;
            //     }
            //
            //     ExecuteSimple(order, price);
            //
            //     for (int i = 0; i < ordersOnBoard.Count; i++)
            //     {
            //         if (ordersOnBoard[i].NumberUser == order.NumberUser)
            //         {
            //             ordersOnBoard.RemoveAt(i);
            //             break;
            //         }
            //     }
            //
            //     // return true;
            //
            //     if (order.Side == Side.Buy &&
            //         order.Price >= _bestSell && _bestSell != 0)
            //     {
            //         decimal price = isFirstTime ? _bestSell : order.Price;
            //
            //         ExecuteSimple(order, price);
            //
            //         for (int i = 0; i < ordersOnBoard.Count; i++)
            //         {
            //             if (ordersOnBoard[i].NumberUser == order.NumberUser)
            //             {
            //                 ordersOnBoard.RemoveAt(i);
            //                 break;
            //             }
            //         }
            //
            //         return true;
            //     }
            //     else if (order.Side == Side.Sell &&
            //              order.Price <= _bestBuy && _bestBuy != 0)
            //     {
            //         decimal price = isFirstTime ? _bestBuy : order.Price;
            //
            //         ExecuteSimple(order, price);
            //
            //         for (int i = 0; i < ordersOnBoard.Count; i++)
            //         {
            //             if (ordersOnBoard[i].NumberUser == order.NumberUser)
            //             {
            //                 ordersOnBoard.RemoveAt(i);
            //                 break;
            //             }
            //         }
            //
            //         return true;
            //     }
            // }
        }

        return false;
    }

    private void ExecuteSimple(Order order, decimal price)
    {
        DateTime callBack;
        if (_serverTime > order.TimeCreate)
        {
            callBack = _serverTime;
        }
        else
        {
            callBack = order.TimeCreate;
        }

        Order newOrder = new()
        {
            NumberMarket = order.NumberMarket,
            NumberUser = order.NumberUser,
            State = OrderStateType.Done,
            Volume = order.Volume,
            VolumeExecute = order.Volume,
            Price = order.Price,
            TimeCreate = order.TimeCreate,
            TypeOrder = order.TypeOrder,
            OrderTypeTime = order.OrderTypeTime,
            TimeCallBack = callBack,
            TimeDone = callBack,
            Side = order.Side,
            SecurityNameCode = order.SecurityNameCode,
            PortfolioNumber = "Emulator",
            ServerType = order.ServerType
        };

        _ordersToSend.Enqueue(newOrder);

        MyTrade trade = new()
        {
            Volume = order.Volume,
            Price = price,
            SecurityNameCode = order.SecurityNameCode,
            NumberTrade = "emu" + order.NumberMarket,
            Side = order.Side,
            NumberOrderParent = newOrder.NumberMarket,
            Time = _serverTime,
        };

        // NOTE: By logic it always set to server time
        // but maybe it should be TimeCreate
        // if (_serverTime > trade.Time)
        // {
        //     trade.Time = _serverTime;
        // }
        // else
        // {
        //     trade.Time = newOrder.TimeCreate;
        // }


        _myTradesToSend.Enqueue(trade);

    }

    private void ActivateSimple(Order order)
    {
        if (order.TimeCreate == DateTime.MinValue)
        {
            order.TimeCreate = _serverTime;
        }

        Order newOrder = new()
        {
            NumberMarket = DateTime.Now.ToString(new CultureInfo("ru-RU")) + order.NumberUser,
            NumberUser = order.NumberUser,
            State = OrderStateType.Active,
            Volume = order.Volume,
            VolumeExecute = 0,
            Price = order.Price,
            TimeCreate = order.TimeCreate,
            TypeOrder = order.TypeOrder,
            OrderTypeTime = order.OrderTypeTime,
            Side = order.Side,
            SecurityNameCode = order.SecurityNameCode,
            PortfolioNumber = "Emulator",
            ServerType = order.ServerType
        };

        order.NumberMarket = newOrder.NumberMarket;

        if (_serverTime > newOrder.TimeCreate)
        {
            newOrder.TimeCallBack = _serverTime;
        }
        else
        {
            newOrder.TimeCallBack = newOrder.TimeCreate;
        }


        OrderChangeEvent?.Invoke(newOrder);
    }



    public void ProcessTime(DateTime time)
    {
        _serverTime = time;
    }

    public void ProcessBidAsc(decimal sell, decimal buy)
    {
        if (sell == 0 && buy == 0) { return; }

        if (buy != 0 && sell != 0 && buy > sell)
        {
            _bestBuy = sell;
            _bestSell = buy;
        }
        else
        {
            _bestBuy = buy;
            _bestSell = sell;
        }

        foreach (int key in ordersOnBoard2.Keys)
        {
            CheckExecution(false, ordersOnBoard2[key]);
        }

        // for (int i = 0; i < ordersOnBoard.Count; i++)
        // {
        //     if (CheckExecution(false, ordersOnBoard[i]))
        //     {
        //         i--;
        //     }
        // }
    }

    /// <summary>
    /// my trades are changed
    /// </summary>
    // TODO: Rename events
    public event Action<MyTrade> MyTradeEvent;
    // public event Action<MyTrade> UserTradeExecuted;

    /// <summary>
    /// orders are changed
    /// </summary>
    public event Action<Order> OrderChangeEvent;
    // public event Action<Order> OrderChanged;
}
