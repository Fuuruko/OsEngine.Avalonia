/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using OsEngine.Language;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;
using OsEngine.Models.Terminal.Bots;

namespace OsEngine.Models.Terminal.Controls;


/// <summary>
/// Manual position support settings
/// </summary>
internal class BotManualControl
{
    // thread work part

    /// <summary>
    /// Revocation thread
    /// </summary>
    private static Task Watcher;

    /// <summary>
    /// Tabs that need to be checked
    /// </summary>
    private static readonly List<BotManualControl> TabsToCheck = [];

    private static readonly object _tabsAddLocker = new();

    private static readonly object _activatorLocker = new();

    /// <summary>
    /// Activate stream to view deals
    /// </summary>
    public static void Activate()
    {
        lock (_activatorLocker)
        {
            if (Watcher != null)
            {
                return;
            }

            Watcher = new Task(WatcherHome);
            Watcher.Start();
        }
    }

    /// <summary>
    /// Place of work thread that monitors the execution of transactions
    /// </summary>
    public static async void WatcherHome()
    {
        while (MainWindow.ProccesIsWorked)
        {
            await Task.Delay(1000);

            for (int i = 0; i < TabsToCheck.Count; i++)
            {
                if (TabsToCheck[i] == null)
                {
                    continue;
                }
                TabsToCheck[i].CheckPositions();
            }
        }
    }

    private readonly string _name;

    /// <summary>
    /// Stop is enabled
    /// </summary>
    public bool StopIsOn = false;

    /// <summary>
    /// Distance from entry to stop 
    /// </summary>
    public decimal StopDistance = 30;

    /// <summary>
    /// Slippage for stop
    /// </summary>
    public decimal StopSlippage = 5;

    /// <summary>
    /// Profit is enabled
    /// </summary>
    public bool ProfitIsOn = false;

    /// <summary>
    /// Distance from trade entry to order profit
    /// </summary>
    public decimal ProfitDistance = 30;

    /// <summary>
    /// Slippage
    /// </summary>
    public decimal ProfitSlippage = 5;

    /// <summary>
    /// Open orders life time is enabled
    /// </summary>
    public bool SecondToOpenIsOn = true;

    /// <summary>
    /// Time to open a position in seconds, after which the order will be recalled
    /// </summary>
    public TimeSpan SecondToOpen
    {
        get => SecondToOpenIsOn ? _secondToOpen : new TimeSpan(1, 0, 0, 0);
        set => _secondToOpen = value;
    }
    private TimeSpan _secondToOpen = new(0, 0, 0, 50);

    /// <summary>
    /// Closed orders life time is enabled
    /// </summary>
    public bool SecondToCloseIsOn = true;

    /// <summary>
    /// Time to close a position in seconds, after which the order will be recalled
    /// </summary>
    public TimeSpan SecondToClose
    {
        get => SecondToCloseIsOn ? _secondToClose : new TimeSpan(1, 0, 0, 0);
        set => _secondToClose = value;
    }
    private TimeSpan _secondToClose = new(0, 0, 0, 50);

    /// <summary>
    /// Whether re-issuance of the request for closure is included if the first has been withdrawn
    /// </summary>
    public bool DoubleExitIsOn = true;

    /// <summary>
    /// Type of re-request for closure
    /// </summary>
    public OrderPriceType TypeDoubleExitOrder;

    /// <summary>
    /// Slip to re-close
    /// </summary>
    public decimal DoubleExitSlippage = 10;

    /// <summary>
    /// Is revocation of orders for opening on price rollback included
    /// </summary>
    public bool SetbackToOpenIsOn = false;

    /// <summary>
    /// Maximum rollback from order price when opening a position
    /// </summary>
    public decimal SetbackToOpenPosition = 10;

    /// <summary>
    /// Whether revocation of orders for closing on price rollback is included
    /// </summary>
    public bool SetbackToCloseIsOn = false;

    /// <summary>
    /// Maximum rollback from order price when opening a position
    /// </summary>
    public decimal SetbackToClosePosition = 10;

    public DistanceType ValuesType;

    /// <summary>
    /// Order lifetime type
    /// </summary>
    // NOTE: Can be moved to SimpleBot probably
    public OrderTypeTime OrderTypeTime;

    /// <summary>
    /// Journal
    /// </summary>
    private SimpleBot _bot;

    public DateTime ServerTime = DateTime.MinValue;

    /// <summary>
    /// Constructor
    /// </summary>
    public BotManualControl(string name, SimpleBot botTab, StartProgram startProgram)
    {
        _name = name;
        _startProgram = startProgram;

        if (Load() == false)
        {
            Save();
        }

        _bot = botTab;

        if (_startProgram != StartProgram.IsOsTrader) { return; }

        if (Watcher == null)
        {
            Activate();
        }

        lock (_tabsAddLocker)
        {
            TabsToCheck.Add(this);
        }
    }

    /// <summary>
    /// Load
    /// </summary>
    private bool Load()
    {
        if (!File.Exists(@"Engine\" + _name + @"StrategSettings.txt"))
        {
            return false;
        }
        try
        {
            using StreamReader reader = new(@"Engine\" + _name + @"StrategSettings.txt");

            StopIsOn = Convert.ToBoolean(reader.ReadLine());
            StopDistance = reader.ReadLine().ToDecimal();
            StopSlippage = reader.ReadLine().ToDecimal();
            ProfitIsOn = Convert.ToBoolean(reader.ReadLine());
            ProfitDistance = reader.ReadLine().ToDecimal();
            ProfitSlippage = reader.ReadLine().ToDecimal();
            TimeSpan.TryParse(reader.ReadLine(), out _secondToOpen);
            TimeSpan.TryParse(reader.ReadLine(), out _secondToClose);

            DoubleExitIsOn = Convert.ToBoolean(reader.ReadLine());

            SecondToOpenIsOn = Convert.ToBoolean(reader.ReadLine());
            SecondToCloseIsOn = Convert.ToBoolean(reader.ReadLine());

            SetbackToOpenIsOn = Convert.ToBoolean(reader.ReadLine());
            SetbackToOpenPosition = reader.ReadLine().ToDecimal();
            SetbackToCloseIsOn = Convert.ToBoolean(reader.ReadLine());
            SetbackToClosePosition = reader.ReadLine().ToDecimal();

            DoubleExitSlippage = reader.ReadLine().ToDecimal();
            Enum.TryParse(reader.ReadLine(), out TypeDoubleExitOrder);
            Enum.TryParse(reader.ReadLine(), out ValuesType);
            Enum.TryParse(reader.ReadLine(), out OrderTypeTime);

            reader.Close();
        }
        catch (Exception)
        {
            // ignore
        }
        return true;
    }

    /// <summary>
    /// Save
    /// </summary>
    public void Save()
    {
        if (_startProgram == StartProgram.IsOsOptimizer)
        {
            return;
        }

        try
        {
            using StreamWriter writer = new(@"Engine\" + _name + @"StrategSettings.txt", false);
            CultureInfo myCultureInfo = new("ru-RU");
            writer.WriteLine(StopIsOn);
            writer.WriteLine(StopDistance.ToString(myCultureInfo));
            writer.WriteLine(StopSlippage.ToString(myCultureInfo));
            writer.WriteLine(ProfitIsOn.ToString(myCultureInfo));
            writer.WriteLine(ProfitDistance.ToString(myCultureInfo));
            writer.WriteLine(ProfitSlippage.ToString(myCultureInfo));
            writer.WriteLine(SecondToOpen.ToString());
            writer.WriteLine(SecondToClose.ToString());

            writer.WriteLine(DoubleExitIsOn);

            writer.WriteLine(SecondToOpenIsOn);
            writer.WriteLine(SecondToCloseIsOn);

            writer.WriteLine(SetbackToOpenIsOn);
            writer.WriteLine(SetbackToOpenPosition);
            writer.WriteLine(SetbackToCloseIsOn);
            writer.WriteLine(SetbackToClosePosition);
            writer.WriteLine(DoubleExitSlippage);
            writer.WriteLine(TypeDoubleExitOrder);
            writer.WriteLine(ValuesType);
            writer.WriteLine(OrderTypeTime);
            writer.Close();
        }
        catch (Exception)
        {
            // ignore
        }
    }

    /// <summary>
    /// Delete
    /// </summary>
    public void Delete()
    {
        try
        {
            if (File.Exists(@"Engine\" + _name + @"StrategSettings.txt"))
            {
                File.Delete(@"Engine\" + _name + @"StrategSettings.txt");
            }

            if (TabsToCheck != null)
            {
                lock (_tabsAddLocker)
                {
                    for (int i = 0; i < TabsToCheck.Count; i++)
                    {
                        if (TabsToCheck[i]._name == _name)
                        {
                            TabsToCheck.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            if (_bot != null)
            {
                _bot = null;
            }
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// Show settings
    /// </summary>
    public void ShowDialog(StartProgram startProgram)
    {
        // BotManualControlUi ui = new BotManualControlUi(this, startProgram);
        // ui.ShowDialog();
    }

    /// <summary>
    /// Program that created the robot
    /// </summary>
    public StartProgram _startProgram;

    /// <summary>
    /// Disable all support functions
    /// </summary>
    public void DisableManualSupport()
    {
        bool valueIsChanged = false;

        if (DoubleExitIsOn == true)
        {
            DoubleExitIsOn = false;
            valueIsChanged = true;
        }

        if (ProfitIsOn == true)
        {
            ProfitIsOn = false;
            valueIsChanged = true;
        }

        if (SecondToCloseIsOn == true)
        {
            SecondToCloseIsOn = false;
            valueIsChanged = true;
        }

        if (SecondToOpenIsOn == true)
        {
            SecondToOpenIsOn = false;
            valueIsChanged = true;
        }

        if (SetbackToCloseIsOn == true)
        {
            SetbackToCloseIsOn = false;
            valueIsChanged = true;
        }

        if (SetbackToOpenIsOn == true)
        {
            SetbackToOpenIsOn = false;
            valueIsChanged = true;
        }

        if (StopIsOn == true)
        {
            StopIsOn = false;
            valueIsChanged = true;
        }

        if (valueIsChanged == true)
        {
            Save();
        }
    }


    /// <summary>
    /// The method in which the thread monitors the execution of orders
    /// </summary>
    private void CheckPositions()
    {
        if (MainWindow.ProccesIsWorked == false
                || ServerTime == DateTime.MinValue
                || _startProgram != StartProgram.IsOsTrader)
        {
            return;
        }

        if (_bot == null)
        {
            return;
        }

        List<Position> openDeals = _bot.PositionsOpenAll;

        if (openDeals == null)
        {
            return;
        }

        try
        {
            for (int i = 0; i < openDeals.Count; i++)
            {
                Position position = null;
                try
                {
                    position = openDeals[i];
                }
                catch
                {
                    continue;
                    // ignore
                }


                for (int i2 = 0; position.OpenOrders != null && i2 < position.OpenOrders.Count; i2++)
                {
                    // open orders
                    Order openOrder = position.OpenOrders[i2];

                    if (openOrder.State != OrderStateType.Active &&
                            openOrder.State != OrderStateType.Partial)
                    {
                        continue;
                    }

                    if (IsInArray(openOrder))
                    {
                        continue;
                    }

                    if (openOrder.OrderTypeTime == OrderTypeTime.Specified)
                    {
                        if (SecondToOpenIsOn &&
                                openOrder.TimeCreate.Add(openOrder.LifeTime) < ServerTime)
                        {
                            SendNewLogMessage(OsLocalization.Trader.Label70 + openOrder.NumberMarket,
                                    LogMessageType.Trade);
                            SendOrderToClose(openOrder, position);
                        }
                    }

                    if (!SetbackToOpenIsOn) { continue; }

                    decimal maxSpread = GetMaxSpread(openOrder);
                    decimal bestPrice;
                    if (openOrder.IsBuy)
                    {
                        bestPrice = _bot.PriceBestBid;
                    }
                    else
                    {
                        bestPrice = _bot.PriceBestAsk;
                    }

                    if (Math.Abs(bestPrice - openOrder.Price) > maxSpread)
                    {
                        SendNewLogMessage(OsLocalization.Trader.Label157 + openOrder.NumberMarket,
                                LogMessageType.Trade);
                        SendOrderToClose(openOrder, position);
                    }
                }

                for (int i2 = 0; position.CloseOrders != null && i2 < position.CloseOrders.Count; i2++)
                {
                    // close orders
                    Order closeOrder = position.CloseOrders[i2];

                    if (closeOrder == null)
                    {
                        continue;
                    }

                    if (closeOrder.State != OrderStateType.Active &&
                                closeOrder.State != OrderStateType.Partial)
                    {
                        continue;
                    }

                    if (IsInArray(closeOrder))
                    {
                        continue;
                    }

                    if (closeOrder.OrderTypeTime == OrderTypeTime.Specified)
                    {
                        if (SecondToCloseIsOn &&
                                closeOrder.TimeCreate.Add(closeOrder.LifeTime) < ServerTime)
                        {
                            SendNewLogMessage(OsLocalization.Trader.Label70 + closeOrder.NumberMarket,
                                    LogMessageType.Trade);
                            SendOrderToClose(closeOrder, position);
                        }
                    }

                    if (SetbackToCloseIsOn &&
                            closeOrder.Side == Side.Buy)
                    {
                        decimal priceRedLine = closeOrder.Price -
                            _bot.Security.PriceStep * SetbackToClosePosition;

                        if (_bot.PriceBestBid <= priceRedLine)
                        {
                            SendNewLogMessage(OsLocalization.Trader.Label157 + closeOrder.NumberMarket,
                                    LogMessageType.Trade);
                            SendOrderToClose(closeOrder, position);
                        }
                    }

                    if (SetbackToCloseIsOn &&
                            closeOrder.Side == Side.Sell)
                    {
                        decimal priceRedLine = closeOrder.Price +
                            _bot.Security.PriceStep * SetbackToClosePosition;

                        if (_bot.PriceBestAsk >= priceRedLine)
                        {
                            SendNewLogMessage(OsLocalization.Trader.Label157 + closeOrder.NumberMarket,
                                    LogMessageType.Trade);
                            SendOrderToClose(closeOrder, position);
                        }
                    }
                }
            }
        }
        catch
            (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// Try to set a stop and profit
    /// </summary>
    public void TryReloadStopAndProfit(Position position)
    {
        if (StopIsOn)
        {
            decimal priceRedLine = position.EntryPrice;
            decimal priceOrder;
            if (position.IsBuy)
            {
                priceRedLine -= GetDistance(position, StopDistance);
                priceOrder = priceRedLine - GetDistance(position, StopSlippage);
            }
            else
            {
                priceRedLine += GetDistance(position, StopDistance);
                priceOrder = priceRedLine + GetDistance(position, StopSlippage);

            }
            _bot.CloseAtStop(position, priceRedLine, priceOrder);
        }

        if (ProfitIsOn)
        {
            decimal priceRedLine = position.EntryPrice;
            decimal priceOrder;
            if (position.IsBuy)
            {
                priceRedLine += GetDistance(position, ProfitDistance);
                priceOrder = priceRedLine - GetDistance(position, ProfitSlippage);
            }
            else
            {
                priceRedLine -= GetDistance(position, ProfitDistance);
                priceOrder = priceRedLine + GetDistance(position, ProfitSlippage);

            }
            _bot.CloseAtProfit(position, priceRedLine, priceOrder);
        }
    }

    /// <summary>
    /// Attempt to close the position
    /// </summary>
    public void TryEmergencyClosePosition(Position position)
    {
        if (TypeDoubleExitOrder == OrderPriceType.Market)
        {
            _bot.Close.Market(position, position.OpenVolume, OsLocalization.Trader.Label410);
        }
        else if (TypeDoubleExitOrder == OrderPriceType.Limit)
        {
            decimal price;
            decimal exitDistance = ValuesType switch
            {
                DistanceType.MinPriceStep => _bot.Security.PriceStep * DoubleExitSlippage,
                DistanceType.Absolute => DoubleExitSlippage,
                DistanceType.Percent => DoubleExitSlippage / 100
                    * (position.IsBuy ? _bot.PriceBestBid : _bot.PriceBestAsk),
                _ => 0,
            };

            if (position.IsBuy)
            {
                price = _bot.PriceBestBid - exitDistance;
            }
            else
            {
                price = _bot.PriceBestAsk + exitDistance;
            }

            _bot.Close.Limit(position, price, position.OpenVolume, OsLocalization.Trader.Label410);
        }
    }

    /// <summary>
    /// Get slippage
    /// </summary>
    private decimal GetEmergencyExitDistance(Position position)
    {
        return ValuesType switch
        {
            DistanceType.MinPriceStep => _bot.Security.PriceStep * DoubleExitSlippage,
            DistanceType.Absolute => DoubleExitSlippage,
            DistanceType.Percent => DoubleExitSlippage / 100
                * (position.IsBuy ? _bot.PriceBestBid : _bot.PriceBestAsk),
            _ => 0,
        };
    }

    /// <summary>
    /// Get slippage for profit
    /// </summary>
    public decimal GetDistance(Position position, decimal value)
    {
        return ValuesType switch
        {
            DistanceType.MinPriceStep => _bot.Security.PriceStep * value,
            DistanceType.Absolute => value,
            DistanceType.Percent => position.EntryPrice * value / 100,
            _ => 0,
        };
    }

    /// <summary>
    /// Get the maximum spread
    /// </summary>
    private decimal GetMaxSpread(Order order)
    {
        if (_bot == null)
        {
            return 0;
        }
        decimal maxSpread = 0;

        if (ValuesType == DistanceType.MinPriceStep)
        {
            maxSpread = _bot.Security.PriceStep * SetbackToOpenPosition;
        }
        else if (ValuesType == DistanceType.Absolute)
        {
            maxSpread = SetbackToOpenPosition;
        }
        else if (ValuesType == DistanceType.Percent)
        {
            maxSpread = order.Price * SetbackToOpenPosition / 100;
        }

        return maxSpread;
    }

    /// <summary>
    /// Orders already sent for closure
    /// </summary>
    private readonly List<Order> _ordersToClose = [];

    /// <summary>
    /// Send a review order
    /// </summary>
    /// <param name="order">order</param>
    /// <param name="deal">position of which order belongs</param>
    private void SendOrderToClose(Order order, Position deal)
    {
        if (IsInArray(order))
        {
            return;
        }

        _ordersToClose.Add(order);

        DontOpenOrderDetectedEvent?.Invoke(order, deal);
    }

    /// <summary>
    /// Is this order already sent for review?
    /// </summary>
    private bool IsInArray(Order order)
    {
        for (int i = 0; i < _ordersToClose.Count; i++)
        {
            if (_ordersToClose[i].NumberUser == order.NumberUser)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// New order for withdrawal event
    /// </summary>
    public event Action<Order, Position> DontOpenOrderDetectedEvent;

    // log

    /// <summary>
    /// Send a new log message
    /// </summary>
    private void SendNewLogMessage(string message, LogMessageType type)
    {
        LogMessageEvent?.Invoke(message, type);
    }

    /// <summary>
    /// Outgoing message for log
    /// </summary>
    public event Action<string, LogMessageType> LogMessageEvent;

    /// <summary>
    /// Type of variables for distance calculation
    /// </summary>
    public enum DistanceType : byte
    {
        /// <summary>
        /// Minimum instrument price step
        /// </summary>
        MinPriceStep,

        /// <summary>
        /// Absolute values
        /// </summary>
        Absolute,

        /// <summary>
        /// %
        /// </summary>
        Percent
    }
}

