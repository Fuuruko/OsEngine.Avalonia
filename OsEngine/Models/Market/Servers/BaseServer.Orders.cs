using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using OsEngine.Language;
using OsEngine.Models.Entity;
using OsEngine.Models.Entity.Server;
using OsEngine.Models.Logging;

namespace OsEngine.Models.Market.Servers;

// NOTE: Probably can be splitted to another class
public partial class BaseServer
{
    private OrdersHub _ordersHub;

    private List<string> _cancelledOrdersNumbers = [];

    /// <summary>
    /// array for storing orders to be sent to the exchange
    /// </summary>
    private ConcurrentQueue<OrderAserverSender> _ordersToExecute = new();

    private List<OrderCounter> _canceledOrders = [];
    // private LinkedList<OrderCounter> _canceledOrders = [];

    /// <summary>
    /// waiting time after server start, after which it is possible to place orders
    /// </summary>
    public double WaitTimeToTradeAfterFirstStart
    {
        get
        {
            if (_alreadyLoadAwaitInfoFromServerPermission == false)
            {
                _alreadyLoadAwaitInfoFromServerPermission = true;

                if (Permissions != null)
                {
                    field = Permissions.SecondsAfterStartSendOrders;
                }
            }

            return field;
        }
        set;
    } = 60;

    private bool _alreadyLoadAwaitInfoFromServerPermission = false;

    /// <summary>
    /// does the server support order price change
    /// </summary>
    public bool IsCanChangeOrderPrice
    {
        get
        {
            if (ServerType == ServerType.None) { return false; }

            return Permissions?.CanChangeOrderPrice == true;
        }
    }

    /// <summary>
    /// work place of thred on the queues of ordr execution and order cancellation 
    /// </summary>
    private async void ExecutorOrdersThreadArea()
    {
        while (true)
        {
            try
            {
                if (_ordersToExecute.IsEmpty == true)
                {
                    await Task.Delay(1);
                    continue;
                }

                if (LastStartServerTime.AddSeconds(WaitTimeToTradeAfterFirstStart) > DateTime.Now)
                {
                    await Task.Delay(1000);
                    continue;
                }

                if (!_ordersToExecute.TryDequeue(out OrderAserverSender order))
                {
                    continue;
                }

                if (order.OrderSendType == OrderSendType.Execute)
                {
                    ServerRealization.SendOrder(order.Order);
                }
                else if (order.OrderSendType == OrderSendType.Cancel)
                {
                    if (IsAlreadyCancelled(order.Order) != false)
                    {
                        continue;
                    }
                    if (ServerRealization.CancelOrder(order.Order))
                    {
                        if (string.IsNullOrEmpty(order.Order.NumberMarket))
                        {
                            continue;
                        }
                        _cancelledOrdersNumbers.Add(order.Order.NumberMarket);

                        if (_cancelledOrdersNumbers.Count > 100)
                        {
                            _cancelledOrdersNumbers.RemoveAt(0);
                        }
                    }
                    else
                    {
                        CancelOrderFailEvent?.Invoke(order.Order);
                    }
                }
                else if (order.OrderSendType == OrderSendType.ChangePrice
                         && IsCanChangeOrderPrice)
                {
                    ServerRealization.ChangeOrderPrice(order.Order, order.NewPrice);
                }
            }
            catch (Exception error)
            {
                OnLogRecieved(error.ToString(), LogMessageType.Error);
            }
        }
    }

    private bool IsAlreadyCancelled(Order order) =>
        !string.IsNullOrEmpty(order.NumberMarket)
        && _cancelledOrdersNumbers.Find(o => o == order.NumberMarket) != null;

    /// <summary>
    /// send order for execution to the trading system
    /// </summary>
    public void ExecuteOrder(Order order)
    {
        try
        {
            if(string.IsNullOrEmpty(order.ServerName))
            {
                order.ServerName = ServerNameAndPrefix;
            }

            UserSetOrderOnExecute?.Invoke(order);
            if (LastStartServerTime.AddSeconds(WaitTimeToTradeAfterFirstStart) > DateTime.Now)
            {
                order.State = OrderStateType.Fail;
                _ordersToSend.Enqueue(order);

                OnLogRecieved(OsLocalization.Market.Message17 + order.NumberUser +
                              OsLocalization.Market.Message18, LogMessageType.Error);
                return;
            }

            if (ServerRealization.ServerStatus == ServerConnectStatus.Disconnect)
            {
                OnLogRecieved($"AServer Error. You can't Execute order when server status Disconnect {order.NumberUser}", LogMessageType.Error);
                order.State = OrderStateType.Fail;
                _ordersToSend.Enqueue(order);

                return;
            }

            if (Portfolios.Count == 0)
            {
                OnLogRecieved("AServer Error. You can't Execute order when Portfolious is null "
                              + order.NumberUser, LogMessageType.Error);
                order.State = OrderStateType.Fail;
                _ordersToSend.Enqueue(order);

                return;
            }

            if (string.IsNullOrEmpty(order.PortfolioNumber) == true)
            {
                OnLogRecieved("AServer Error. You can't Execute order without specifying his portfolio "
                              + order.NumberUser, LogMessageType.Error);
                order.State = OrderStateType.Fail;
                _ordersToSend.Enqueue(order);

                return;
            }

            Portfolio myPortfolio = null;

            for (int i = 0; i < Portfolios.Count; i++)
            {
                if (Portfolios[i].Number == order.PortfolioNumber)
                {
                    myPortfolio = Portfolios[i];
                    break;
                }
            }

            if (myPortfolio == null)
            {
                OnLogRecieved("AServer Error. You can't Execute order. Error portfolio name: "
                              + order.PortfolioNumber, LogMessageType.Error);
                order.State = OrderStateType.Fail;
                _ordersToSend.Enqueue(order);

                return;
            }

            order.TimeCreate = ServerTime;

            OrderAserverSender ord = new()
            {
                Order = order,
                OrderSendType = OrderSendType.Execute
            };

            _ordersHub.SetOrderFromOsEngine(order);

            _ordersToExecute.Enqueue(ord);

            OnLogRecieved(OsLocalization.Market.Message19 + order.Price +
                          OsLocalization.Market.Message20 + order.Side +
                          OsLocalization.Market.Message21 + order.Volume +
                          OsLocalization.Market.Message22 + order.SecurityNameCode +
                          OsLocalization.Market.Message23 + order.NumberUser, LogMessageType.System);

        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// Order price change
    /// </summary>
    /// <param name="order">An order that will have a new price</param>
    /// <param name="newPrice">New price</param>
    public void ChangeOrderPrice(Order order, decimal newPrice)
    {
        try
        {
            if (string.IsNullOrEmpty(order.NumberMarket))
            {
                OnLogRecieved("AServer Error. You can't change order price an order without a stock exchange number "
                              + order.NumberUser, LogMessageType.Error);
                return;
            }

            if (ServerRealization.ServerStatus == ServerConnectStatus.Disconnect)
            {
                OnLogRecieved("AServer Error. You can't change order price when server status Disconnect "
                              + order.NumberUser, LogMessageType.Error);
                return;
            }

            if (order.Price == newPrice)
            {
                return;
            }

            OrderAserverSender ord = new()
            {
                Order = order,
                OrderSendType = OrderSendType.ChangePrice,
                NewPrice = newPrice
            };

            _ordersToExecute.Enqueue(ord);

            OnLogRecieved(OsLocalization.Market.Label120 + order.NumberUser, LogMessageType.System);

        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// cancel order from the trading system
    /// </summary>
    public void CancelOrder(Order order)
    {
        try
        {
            UserSetOrderOnCancel?.Invoke(order);

            if (string.IsNullOrEmpty(order.NumberMarket))
            {
                OnLogRecieved("AServer Error. You can't cancel an order without a stock exchange number "
                              + order.NumberUser, LogMessageType.Error);
                return;
            }

            if (ServerRealization.ServerStatus == ServerConnectStatus.Disconnect)
            {
                OnLogRecieved("AServer Error. You can't cancel order when server status Disconnect "
                              + order.NumberUser, LogMessageType.Error);
                return;
            }

            OrderCounter saveOrder = null;

            for (int i = 0; i < _canceledOrders.Count; i++)
            {
                if (_canceledOrders[i].NumberMarket == order.NumberMarket)
                {
                    saveOrder = _canceledOrders[i];
                    break;
                }
            }

            if (saveOrder == null)
            {
                saveOrder = new OrderCounter
                {
                    NumberMarket = order.NumberMarket
                };
                _canceledOrders.Add(saveOrder);

                if (_canceledOrders.Count > 50)
                {
                    _canceledOrders.RemoveAt(0);
                }
            }

            saveOrder.NumberOfCalls++;

            if (saveOrder.NumberOfCalls >= 5)
            {
                saveOrder.NumberOfErrors++;

                if (saveOrder.NumberOfErrors <= 3)
                {
                    OnLogRecieved(
                                  "AServer Error. You can't cancel order. There have already been five attempts to cancel order. "
                                  + "NumberUser: " + order.NumberUser
                                  + " NumberMarket: " + order.NumberMarket
                                  , LogMessageType.Error);
                }

                return;
            }

            OrderAserverSender ord = new()
            {
                Order = order,
                OrderSendType = OrderSendType.Cancel
            };

            _ordersToExecute.Enqueue(ord);

            OnLogRecieved(OsLocalization.Market.Message24 + order.NumberUser, LogMessageType.System);

        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// cancel all orders from trading system
    /// </summary>
    public void CancelAllOrders(Security security = null)
    {
        try
        {
            if (ServerStatus == ServerConnectStatus.Disconnect)
            {
                OnLogRecieved("AServer Error. You can't cancel all orders when server status Disconnect "
                              , LogMessageType.Error);
                return;
            }

            // NOTE: Use ICancleAllOrders
            if (security == null)
            {
                ServerRealization.CancelAllOrders();
            }
            else
            {
                ServerRealization.CancelAllOrdersToSecurity(security);
            }
        }
        catch (Exception ex)
        {
            OnLogRecieved(
                          $"AServer. CancelAllOrders method error: {ex}",
                          LogMessageType.Error);
        }
    }

    /// <summary>
    /// cancel all orders from trading system to security
    /// </summary>
    [Obsolete($"Use {nameof(CancelAllOrders)} instead")]
    public void CancelAllOrdersToSecurity(Security security)
    {
        try
        {
            if (ServerStatus == ServerConnectStatus.Disconnect)
            {
                OnLogRecieved("AServer Error. You can't cancel orders to Security when server status Disconnect "
                              , LogMessageType.Error);
                return;
            }

            ServerRealization.CancelAllOrdersToSecurity(security);
        }
        catch (Exception ex)
        {
            OnLogRecieved(
                          "AServer. CancelAllOrdersToSecurity method error: " + ex.ToString(),
                          LogMessageType.Error);
        }
    }

    /// <summary>
    /// order changed
    /// </summary>
    public event Action<Order> NewOrderIncomeEvent;

    /// <summary>
    /// external systems requested order execution
    /// </summary>
    public event Action<Order> UserSetOrderOnExecute;

    /// <summary>
    /// external systems requested order cancellation
    /// </summary>
    public event Action<Order> UserSetOrderOnCancel;

    /// <summary>
    /// An attempt to revoke the order ended in an error
    /// </summary>
    public event Action<Order> CancelOrderFailEvent;

    private void _ordersHub_GetAllActiveOrdersOnReconnectEvent()
    {
        try
        {
            if (ServerStatus == ServerConnectStatus.Disconnect)
            {
                return;
            }

            ServerRealization.GetAllActivOrders();
        }
        catch (Exception ex)
        {
            OnLogRecieved(ex.ToString(), LogMessageType.Error);
        }
    }

    private void ActiveStateOrderCheckStatusEvent(Order order)
    {
        try
        {
            if (ServerStatus == ServerConnectStatus.Disconnect)
            {
                return;
            }

            ServerRealization.GetOrderStatus(order);
        }
        catch (Exception ex)
        {
            OnLogRecieved(ex.ToString(), LogMessageType.Error);
        }
    }

    // NOTE: Can be struct
    private class OrderCounter
    {
        public string NumberMarket;

        public int NumberOfCalls;

        public int NumberOfErrors;
    }


    private enum OrderSendType : byte
    {
        Execute,
        Cancel,
        ChangePrice
    }

    // NOTE: can be struct
    // NOTE: probably can be removed?
    private class OrderAserverSender
    {
        public Order Order;

        public OrderSendType OrderSendType;

        public decimal NewPrice;
    }

    // NOTE: I dont think it really needed
    private class AServerAsyncOrderSender
    {
        public AServerAsyncOrderSender(int rateGateLimitMls)
        {
            if(rateGateLimitMls < 0)
            {
                rateGateLimitMls = 0;
            }

            if(rateGateLimitMls > 0)
            {
                _rateGate = new RateGate(1, TimeSpan.FromMilliseconds(rateGateLimitMls));
            }
        }

        private RateGate _rateGate;

        public void ExecuteAsync(OrderAserverSender order)
        {
            _rateGate?.WaitToProceed();

            Task.Run(() => ExecuteOrderInRealizationEvent(order));
        }

        public event Action<OrderAserverSender> ExecuteOrderInRealizationEvent;
    }
}
