/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
 */

// using OsEngine.Market.Servers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using OsEngine.Language;
using OsEngine.Models.Market;

namespace OsEngine.Models.Entity;

public partial class Position
{
    public Position() { }

    public Position(Order openOrder)
    {
        OpenOrders = [openOrder];
    }
    /// <summary>
    /// List of orders involved in opening a position
    /// </summary>
    public List<Order> OpenOrders { get; private set; }

    /// <summary>
    /// Load a new order to open a position
    /// </summary>
    /// <param name="openOrder"></param>
    public void AddNewOpenOrder(Order openOrder)
    {
        if (OpenOrders == null)
        {
            OpenOrders = [openOrder];
            return;
        }

        if (string.IsNullOrEmpty(SecurityName)
            || SecurityName.EndsWith("TestPaper"))
        {
            OpenOrders.Add(openOrder);
        }
        else if (SecurityName == openOrder.SecurityNameCode)
        {
            OpenOrders.Add(openOrder);
        }

        State = PositionStateType.Opening;
    }

    /// <summary>
    /// List of orders involved in closing a position
    /// </summary>
    // TODO: set default List and delete all null comparison
    // or replace with count
    public List<Order> CloseOrders { get; private set; }

    /// <summary>
    /// Trades of this position
    /// </summary>
    public List<MyTrade> MyTrades
    {
        get
        {
            List<MyTrade> trades = _userTrades;
            if (trades != null)
            {
                return trades;
            }
            trades = [];

            for (int i = 0; OpenOrders != null && i < OpenOrders.Count; i++)
            {
                List<MyTrade> newTrades = OpenOrders[i].MyTrades;
                if (newTrades != null &&
                        newTrades.Count != 0)
                {
                    trades.AddRange(newTrades);
                }
            }

            for (int i = 0; CloseOrders != null && i < CloseOrders.Count; i++)
            {
                List<MyTrade> newTrades = CloseOrders[i].MyTrades;
                if (newTrades != null &&
                        newTrades.Count != 0)
                {
                    trades.AddRange(newTrades);
                }
            }

            _userTrades = trades;
            return trades;
        }
    }

    private List<MyTrade> _userTrades;

    /// <summary>
    /// Load a new order to a position
    /// </summary>
    /// <param name="closeOrder"></param>
    public void AddNewCloseOrder(Order closeOrder)
    {
        if (CloseOrders == null)
        {
            CloseOrders = [closeOrder];
        }
        else
        {
            if (string.IsNullOrEmpty(SecurityName) == false
                    && SecurityName.EndsWith("TestPaper") == false)
            {
                if (SecurityName == closeOrder.SecurityNameCode)
                {
                    CloseOrders.Add(closeOrder);
                }
            }
            else
            {
                CloseOrders.Add(closeOrder);
            }
        }

        State = PositionStateType.Closing;
    }

    /// <summary>
    /// Are there any active orders to open a position
    /// </summary>
    public bool OpenActive
    {
        get
        {
            if (OpenOrders == null ||
                    OpenOrders.Count == 0)
            {
                return false;
            }

            if (OpenOrders.Find(order => order.State is OrderStateType.Active
                        or OrderStateType.Pending
                        or OrderStateType.None
                        or OrderStateType.Partial) != null)
            {
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Are there any active orders to close a position
    /// </summary>
    public bool CloseActive
    {
        get
        {
            if (CloseOrders == null ||
                    CloseOrders.Count == 0)
            {
                return false;
            }

            if (CloseOrders.Find(order => order.State is OrderStateType.Active
                        or OrderStateType.Pending
                        or OrderStateType.None
                        or OrderStateType.Partial) != null
               )
            {
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Whether stop is active
    /// </summary>
    public bool StopOrderIsActive;

    /// <summary>
    /// Order price stop order
    /// </summary>
    public decimal StopOrderPrice;

    /// <summary>
    /// Stop - the price, the price after which the order will be entered into the system
    /// </summary>
    public decimal StopOrderRedLine;

    /// <summary>
    /// Whether the position will be closed by a stop using a market order
    /// </summary>
    public bool StopIsMarket;

    /// <summary>
    /// Is a profit active order
    /// </summary>
    public bool ProfitOrderIsActive;

    /// <summary>
    /// Order price order profit
    /// </summary>
    public decimal ProfitOrderPrice;

    /// <summary>
    /// Profit - the price, the price after which the order will be entered into the system
    /// </summary>
    public decimal ProfitOrderRedLine;

    /// <summary>
    /// Whether the position will be closed by a profit using a market order
    /// </summary>
    public bool ProfitIsMarket;

    /// <summary>
    /// Buy / sell direction
    /// </summary>
    public Side Direction;

    // NOTE: can be simplified to just bool value
    // that assingn when direction set
    public bool IsBuy => Direction == Side.Buy;
    public bool IsSell => Direction == Side.Sell;

    /// <summary>
    /// Transaction status Open / Close / Opening
    /// </summary>
    public PositionStateType State { get; set; } = PositionStateType.None;

    /// <summary>
    /// Position number
    /// </summary>
    public int Number;

    /// <summary>
    /// Tool code for which the position is open
    /// </summary>
    public string SecurityName
    {
        get
        {
            if (OpenOrders != null && OpenOrders.Count != 0)
            {
                return OpenOrders[0].SecurityNameCode;
            }
            return _securityName;
        }
        set
        {
            if (OpenOrders != null && OpenOrders.Count != 0)
            {
                return;
            }
            _securityName = value;
        }
    }
    private string _securityName;

    /// <summary>
    /// Name of the bot who owns the deal
    /// </summary>
    // NOTE: Doesnt belong here
    public string NameBot;

    /// <summary>
    /// unique server name in multi-connection mode
    /// </summary>
    // NOTE: Doesnt belong here
    public string ServerName
    {
        get
        {
            if (OpenOrders == null
                    || OpenOrders.Count == 0)
            {
                return null;
            }
            else
            {
                return OpenOrders[0].ServerName;
            }
        }
    }

    /// <summary>
    /// The amount of profit on the operation in percent
    /// </summary>
    public decimal ProfitOperationPercent;

    /// <summary>
    /// The amount of profit on the operation in absolute terms
    /// </summary>
    public decimal ProfitOperationAbs;

    /// <summary>
    /// Comment
    /// </summary>
    // NOTE: Doesnt belong here
    public string Comment;

    public Signals Signals { get => field ??= new(); }

    /// <summary>
    /// Maximum volume by position
    /// </summary>
    public decimal MaxVolume
    {
        get
        {
            decimal volume = 0;

            for (int i = 0; OpenOrders != null && i < OpenOrders.Count; i++)
            {
                volume += OpenOrders[i].VolumeExecute;
            }

            return volume;
        }
    }

    /// <summary>
    /// Number of contracts open per trade
    /// </summary>
    public decimal OpenVolume
    {
        get
        {
            // if (CloseOrders == null)
            // {
            //     decimal volume = 0;
            //
            //     for (int i = 0; OpenOrders != null && i < OpenOrders.Count; i++)
            //     {
            //         volume += OpenOrders[i].VolumeExecute;
            //     }
            //     return volume;
            // }

            decimal volumeOpen = OpenOrders?.Sum(o => o.VolumeExecute) ?? 0;

            // for (int i = 0; OpenOrders != null && i < OpenOrders.Count; i++)
            // {
            //     volumeOpen += OpenOrders[i].VolumeExecute;
            // }

            if (CloseOrders == null) { return volumeOpen; }

            decimal volumeClose = CloseOrders.Sum(o => o.VolumeExecute);

            // for (int i = 0; i < CloseOrders.Count; i++)
            // {
            //     valueClose += CloseOrders[i].VolumeExecute;
            // }


            return volumeOpen - volumeClose;
        }
    }

    /// <summary>
    /// Number of contracts awaiting opening
    /// </summary>
    public decimal WaitVolume
    {
        get
        {
            if (OpenOrders == null) { return 0; }

            decimal volumeWait = 0;

            foreach (Order o in OpenOrders)
            {
                if (o.State == OrderStateType.Active
                        || o.State == OrderStateType.Partial)
                {
                    volumeWait += o.Volume - o.VolumeExecute;
                }
            }

            return volumeWait;
        }
    }

    /// <summary>
    /// Position opening price
    /// </summary>
    public decimal EntryPrice
    {
        get
        {
            if (OpenOrders == null ||
                    OpenOrders.Count == 0)
            {
                return 0;
            }

            decimal price = 0;
            decimal volume = 0;
            for (int i = 0; i < OpenOrders.Count; i++)
            {
                decimal volumeEx = OpenOrders[i].VolumeExecute;
                if (volumeEx != 0)
                {
                    volume += volumeEx;
                    price += volumeEx * OpenOrders[i].PriceReal;
                }
            }
            if (volume == 0)
            {
                return OpenOrders[0].Price;
            }

            return price / volume;
        }
    }

    /// <summary>
    /// Position closing price
    /// </summary>
    public decimal ClosePrice
    {
        get
        {
            if (CloseOrders == null ||
                    CloseOrders.Count == 0)
            {
                return 0;
            }

            decimal price = 0;
            decimal volume = 0;
            for (int i = 0; i < CloseOrders.Count; i++)
            {
                if (CloseOrders[i] == null)
                {
                    continue;
                }

                decimal volumeEx = CloseOrders[i].VolumeExecute;
                if (volumeEx != 0)
                {
                    volume += CloseOrders[i].VolumeExecute;
                    price += CloseOrders[i].VolumeExecute * CloseOrders[i].PriceReal;
                }
            }
            if (volume == 0)
            {
                return 0;
            }

            return price / volume;
        }
    }

    /// <summary>
    /// Check the incoming order for this transaction
    /// </summary>
    public void SetOrder(Order newOrder)
    {
        Order openOrder = null;
        if (OpenOrders != null)
        {
            for (int i = 0; i < OpenOrders.Count; i++)
            {
                if (OpenOrders[i].NumberUser != newOrder.NumberUser)
                {
                    continue;
                }
                if ((State == PositionStateType.Done || State == PositionStateType.OpeningFail)
                        &&
                        ((OpenOrders[i].State == OrderStateType.Fail && newOrder.State == OrderStateType.Fail) ||
                         (OpenOrders[i].State == OrderStateType.Cancel && newOrder.State == OrderStateType.Cancel)))
                {
                    return;
                }
                openOrder = OpenOrders[i];
                break;
            }
        }

        if (openOrder != null)
        {
            if (newOrder.State == OrderStateType.Fail &&
                    (openOrder.State == OrderStateType.Partial
                     || openOrder.State == OrderStateType.Done
                     || openOrder.State == OrderStateType.Cancel))
            {// the order was definitely previously placed on the exchange
             // and received the statuses executed.
                return;
            }

            if (openOrder.State != OrderStateType.Done
                    || openOrder.Volume != openOrder.VolumeExecute)    //AVP 
            {
                openOrder.State = newOrder.State;     //AVP 
            }
            openOrder.NumberMarket = newOrder.NumberMarket;

            if (openOrder.TimeCallBack == DateTime.MinValue)
            {
                openOrder.TimeCallBack = newOrder.TimeCallBack;
            }

            openOrder.TimeCancel = newOrder.TimeCancel;

            if (openOrder.MyTrades == null ||
                    openOrder.MyTrades.Count == 0)
            { // если трейдов ещё нет, допускается установка значение исполненного объёма по записи в ордере
              //openOrder.VolumeExecute = newOrder.VolumeExecute;
            }

            // if (OpenVolume == 0)
            // {
            //
            // }
            // else
            // {
            //
            // }
            if (openOrder.State == OrderStateType.Done
                    && openOrder.TradesIsComing
                    && OpenVolume != 0 && !CloseActive)
            {
                State = PositionStateType.Open;
            }
            else if ((newOrder.State == OrderStateType.Fail
                        || newOrder.State == OrderStateType.Cancel)
                    && newOrder.VolumeExecute == 0
                    && OpenVolume == 0
                    && MaxVolume == 0
                    && CloseActive == false
                    && OpenActive == false)
            {
                State = PositionStateType.OpeningFail;
            }
            else if ((newOrder.State == OrderStateType.Cancel
                        || newOrder.State == OrderStateType.Fail)
                    && OpenVolume != 0)
            {
                State = PositionStateType.Open;
            }
            else if (newOrder.State == OrderStateType.Done
                    && OpenVolume == 0
                    && CloseOrders != null
                    && CloseOrders.Count > 0
                    && CloseActive == false
                    && OpenActive == false)
            {
                State = PositionStateType.Done;
            }
            else if (OpenVolume == 0
                    && CloseActive == false
                    && OpenActive == false)
            {
                State = PositionStateType.Done;
            }
        }

        Order closeOrder = null;

        if (CloseOrders != null)
        {
            for (int i = 0; i < CloseOrders.Count; i++)
            {
                if (CloseOrders[i].NumberUser != newOrder.NumberUser)
                {
                    continue;
                }
                if (State == PositionStateType.ClosingFail
                    && CloseOrders[i].State == newOrder.State
                    && (newOrder.State is OrderStateType.Fail
                        or OrderStateType.Cancel))
                {
                    return;
                }
                closeOrder = CloseOrders[i];

                break;
            }
        }

        if (closeOrder != null)
        {
            if (closeOrder.State != OrderStateType.Done
                    || closeOrder.Volume != closeOrder.VolumeExecute)    //AVP 
            {
                closeOrder.State = newOrder.State;
            }

            closeOrder.NumberMarket = newOrder.NumberMarket;

            if (closeOrder.TimeCallBack == DateTime.MinValue)
            {
                closeOrder.TimeCallBack = newOrder.TimeCallBack;
            }
            closeOrder.TimeCancel = newOrder.TimeCancel;

            if (closeOrder.MyTrades == null ||
                    closeOrder.MyTrades.Count == 0)
            { // если трейдов ещё нет, допускается установка значение исполненного объёма по записи в ордере
                closeOrder.VolumeExecute = newOrder.VolumeExecute;
            }

            if (OpenVolume == 0
                    && CloseActive == false
                    && OpenActive == false)
            {
                State = PositionStateType.Done;
            }
            else if (closeOrder.State == OrderStateType.Fail
                    && CloseActive == false
                    && OpenVolume != 0)
            {
                //AlertMessageManager.ThrowAlert(null, "Fail", "");
                State = PositionStateType.ClosingFail;
            }
            else if (closeOrder.State == OrderStateType.Cancel
                    && CloseActive == false
                    && OpenVolume != 0)
            {
                // if not fully closed and this is the last order in the closing orders
                //AlertMessageManager.ThrowAlert(null, "Cancel", "");
                State = PositionStateType.ClosingFail;
            }
            else if (closeOrder.State == OrderStateType.Done
                    && OpenVolume < 0)
            {
                State = PositionStateType.ClosingSurplus;
            }

            if (State == PositionStateType.Done
                    && CloseOrders != null)
            {
                CalculateProfitToPosition();
            }
        }
    }

    /// <summary>
    /// calculates the values of the fields ProfitOperationPersent and ProfitOperationPunkt
    /// </summary>
    private void CalculateProfitToPosition()
    {
        decimal entryPrice = EntryPrice;
        decimal closePrice = ClosePrice;

        if (entryPrice == 0 || closePrice == 0) { return; }

        if (IsBuy)
        {
            ProfitOperationAbs = closePrice - entryPrice;
            ProfitOperationPercent = closePrice / entryPrice * 100 - 100;
        }
        else
        {
            ProfitOperationAbs = entryPrice - closePrice;
            ProfitOperationPercent = -(closePrice / entryPrice * 100 - 100);
        }
    }

    /// <summary>
    /// Check incoming trade for this trade
    /// </summary>
    public void SetTrade(MyTrade trade)
    {
        _userTrades = null;

        if (OpenOrders != null)
        {
            for (int i = 0; i < OpenOrders.Count; i++)
            {
                Order curOrdOpen = OpenOrders[i];

                if (curOrdOpen == null)
                {
                    continue;
                }

                if (curOrdOpen.NumberMarket == trade.NumberOrderParent
                        && curOrdOpen.SecurityNameCode == trade.SecurityNameCode)
                {
                    trade.NumberPosition = Number.ToString();
                    curOrdOpen.SetTrade(trade);

                    if (OpenVolume != 0 &&
                            State == PositionStateType.Opening)
                    {
                        State = PositionStateType.Open;
                    }
                    else if (OpenVolume == 0
                            && OpenActive == false && CloseActive == false)
                    {
                        curOrdOpen.TimeDone = trade.Time;
                        State = PositionStateType.Done;
                    }
                }
            }
        }

        if (CloseOrders != null)
        {
            for (int i = 0; i < CloseOrders.Count; i++)
            {
                Order curOrdClose = CloseOrders[i];

                if (curOrdClose == null)
                {
                    continue;
                }

                if (curOrdClose.NumberMarket == trade.NumberOrderParent
                        && curOrdClose.SecurityNameCode == trade.SecurityNameCode)
                {
                    trade.NumberPosition = Number.ToString();
                    curOrdClose.SetTrade(trade);

                    if (OpenVolume == 0
                            && OpenActive == false && CloseActive == false)
                    {
                        State = PositionStateType.Done;
                        curOrdClose.TimeDone = trade.Time;
                    }
                    else if (OpenVolume < 0)
                    {
                        State = PositionStateType.ClosingSurplus;
                    }
                }
            }
        }

        if (State == PositionStateType.Done && CloseOrders != null)
        {
            decimal entryPrice = EntryPrice;
            decimal closePrice = ClosePrice;

            if (entryPrice != 0 && closePrice != 0)
            {
                if (Direction == Side.Buy)
                {
                    ProfitOperationPercent = closePrice / entryPrice * 100 - 100;
                    ProfitOperationAbs = closePrice - entryPrice;
                }
                else
                {
                    ProfitOperationAbs = entryPrice - closePrice;
                    ProfitOperationPercent = -(closePrice / entryPrice * 100 - 100);
                }
            }
        }
    }

    /// <summary>
    /// Load bid with ask into the trade to recalculate the profit
    /// </summary>
    // NOTE: Can be done by sharabled class
    // and event when parameter change in this class
    public void SetBidAsk(decimal bid, decimal ask)
    {
        if (State is not PositionStateType.Open
                and not PositionStateType.Closing
                and not PositionStateType.ClosingFail
                || OpenOrders == null
                || OpenOrders.Count == 0
                || ClosePrice != 0)
        {
            return;
        }

        decimal entryPrice = EntryPrice;
        if (entryPrice == 0) { return; }

        if (IsBuy && ask != 0)
        {
            ProfitOperationPercent = ask / entryPrice * 100 - 100;
            ProfitOperationAbs = ask - entryPrice;
        }
        else if (bid != 0)
        {
            ProfitOperationPercent = -(bid / entryPrice * 100 - 100);
            ProfitOperationAbs = entryPrice - bid;
        }
    }

    /// <summary>
    /// Take the string to save
    /// </summary>
    public StringBuilder GetStringForSave()
    {
        StringBuilder result = new();

        result.Append(Direction + "#");

        result.Append(State + "#");

        result.Append(NameBot + "#");

        result.Append(ProfitOperationPercent.ToString(new CultureInfo("ru-RU")) + "#");

        result.Append(ProfitOperationAbs.ToString(new CultureInfo("ru-RU")) + "#");

        if (OpenOrders == null)
        {
            result.Append("null" + "#");
        }
        else
        {
            for (int i = 0; i < OpenOrders.Count; i++)
            {
                result.Append(OpenOrders[i].GetStringForSave() + "^");
            }
            result.Append("#");
        }

        result.Append(Number + "#");

        result.Append(Comment + "#");

        result.Append(StopOrderIsActive + "#");
        result.Append(StopOrderPrice + "#");
        result.Append(StopOrderRedLine + "#");

        result.Append(ProfitOrderIsActive + "#");
        result.Append(ProfitOrderPrice + "#");

        result.Append(Lots + "#");
        result.Append(PriceStepCost + "#");
        result.Append(PriceStep + "#");
        result.Append(PortfolioValueOnOpenPosition + "#");

        result.Append(ProfitOrderRedLine + "#");
        result.Append(SignalTypeOpen + "#");
        result.Append(SignalTypeClose + "#");

        result.Append(CommissionValue + "#");
        result.Append(CommissionType);

        if (CloseOrders != null)
        {
            for (int i = 0; i < CloseOrders.Count; i++)
            {
                result.Append("#" + CloseOrders[i].GetStringForSave());
            }
        }

        result.Append("#" + StopIsMarket);
        result.Append("#" + ProfitIsMarket);
        result.Append("#" + SecurityName);

        return result;
    }

    /// <summary>
    /// Load trade from incoming line
    /// </summary>
    public void SetDealFromString(string save)
    {
        string[] arraySave = save.Split('#');

        Enum.TryParse(arraySave[0], true, out Direction);

        NameBot = arraySave[2];

        ProfitOperationPercent = arraySave[3].ToDecimal();

        ProfitOperationAbs = arraySave[4].ToDecimal();

        if (arraySave[5] == null)
        {
            return;
        }

        if (arraySave[5] != "null")
        {
            string[] ordersOpen = arraySave[5].Split('^');
            if (ordersOpen.Length != 1)
            {
                OpenOrders = [];
                for (int i = 0; i < ordersOpen.Length - 1; i++)
                {
                    OpenOrders.Add(new Order());
                    OpenOrders[i].SetOrderFromString(ordersOpen[i]);
                }
            }
        }

        Number = Convert.ToInt32(arraySave[6]);
        Comment = arraySave[7];

        StopOrderIsActive = Convert.ToBoolean(arraySave[8]);
        StopOrderPrice = arraySave[9].ToDecimal();
        StopOrderRedLine = arraySave[10].ToDecimal();

        ProfitOrderIsActive = Convert.ToBoolean(arraySave[11]);
        ProfitOrderPrice = arraySave[12].ToDecimal();

        Lots = arraySave[13].ToDecimal();
        PriceStepCost = arraySave[14].ToDecimal();
        PriceStep = arraySave[15].ToDecimal();
        PortfolioValueOnOpenPosition = arraySave[16].ToDecimal();

        ProfitOrderRedLine = arraySave[17].ToDecimal();

        SignalTypeOpen = arraySave[18];
        SignalTypeClose = arraySave[19];

        CommissionValue = arraySave[20].ToDecimal();
        Enum.TryParse(arraySave[21], out CommissionType commissionType);
        CommissionType = commissionType;

        for (int i = 22; i < arraySave.Length - 3; i++)
        {
            if (i == arraySave.Length - 3)
            {
                break;
            }
            string saveOrd = arraySave[i];

            if (saveOrd.Split('@').Length < 3)
            {
                continue;
            }

            Order newOrder = new();
            newOrder.SetOrderFromString(saveOrd);
            AddNewCloseOrder(newOrder);
        }

        if (arraySave[^3] == "True"
                || arraySave[^3] == "False"
                || arraySave[^3] == "true"
                || arraySave[^3] == "false")
        {
            StopIsMarket = Convert.ToBoolean(arraySave[^3]);
            ProfitIsMarket = Convert.ToBoolean(arraySave[^2]);
        }

        SecurityName = arraySave[^1];

        Enum.TryParse(arraySave[1], true, out PositionStateType state);
        State = state;
    }

    /// <summary>
    /// Position creation time
    /// </summary>
    public DateTime TimeCreate
    {
        get
        {
            if (_timeCreate == DateTime.MinValue &&
                    OpenOrders != null
                    && OpenOrders.Count > 0)
            {
                _timeCreate = OpenOrders[0].GetLastTradeTime();
            }

            return _timeCreate;
        }
    }

    private DateTime _timeCreate;

    /// <summary>
    /// Position closing time
    /// </summary>
    // TODO: Rename CloseTime
    public DateTime TimeClose
    {
        get
        {
            if (CloseOrders != null
                    && CloseOrders.Count != 0)
            {
                for (int i = CloseOrders.Count - 1; i > -1 && i < CloseOrders.Count; i--)
                {
                    if (CloseOrders[i].State != OrderStateType.Done
                            && CloseOrders[i].State != OrderStateType.Partial)
                    {
                        continue;
                    }

                    DateTime time = CloseOrders[i].GetLastTradeTime();
                    if (time != DateTime.MinValue)
                    {
                        return time;
                    }
                }
            }
            return TimeCreate;
        }
    }

    /// <summary>
    /// Position opening time. The time when the first transaction on our position passed on the exchange
    /// if the transaction is not open yet, it will return the time to create the position
    /// </summary>
    // TODO: Rename OpenTime
    public DateTime TimeOpen
    {
        get
        {
            if (OpenOrders == null || OpenOrders.Count == 0)
            {
                return TimeCreate;
            }

            DateTime timeOpen = DateTime.MaxValue;

            for (int i = 0; i < OpenOrders.Count; i++)
            {
                if (OpenOrders[i].TradesIsComing &&
                        OpenOrders[i].TimeExecuteFirstTrade < timeOpen)
                {
                    timeOpen = OpenOrders[i].TimeExecuteFirstTrade;
                }
            }

            if (timeOpen == DateTime.MaxValue)
            {
                return TimeCreate;
            }

            return TimeCreate;
        }
    }

    public string PositionSpecification
    {
        get
        {
            string result = "";

            result += OsLocalization.Trader.Label225 + ": " + Number + ", "
                + OsLocalization.Trader.Label224 + ": " + State + ", "
                + OsLocalization.Trader.Label228 + ": " + Direction + "\n";

            result += OsLocalization.Trader.Label102 + ": " + SecurityName + "\n";

            if (ProfitPortfolioAbs != 0)
            {
                decimal profit = Math.Round(ProfitPortfolioAbs, 10);

                result += OsLocalization.Trader.Label404 + ": " + profit.ToStringWithNoEndZero() + "\n";
            }

            if (State != PositionStateType.OpeningFail)
            {
                decimal entryPrice = Math.Round(EntryPrice, 10);

                result += OsLocalization.Trader.Label400 + ": " + entryPrice.ToStringWithNoEndZero();

                if (State == PositionStateType.Done)
                {
                    decimal closePrice = Math.Round(ClosePrice, 10);

                    result += ", " + OsLocalization.Trader.Label401 + ": " + closePrice.ToStringWithNoEndZero() + " ";
                }

                result += "\n";

                result += OsLocalization.Trader.Label421 + ": " + TimeOpen.ToString(OsLocalization.CurCulture);

                if (State == PositionStateType.Done)
                {
                    result += ", " + OsLocalization.Trader.Label420 + ": " + TimeClose.ToString(OsLocalization.CurCulture) + " ";
                }

                result += "\n";


            }

            if (OpenVolume == 0)
            {
                result += OsLocalization.Trader.Label402 + ": " + MaxVolume + "\n";
            }
            else
            {
                result += OsLocalization.Trader.Label403 + ": " + OpenVolume + "\n";
            }

            if (string.IsNullOrEmpty(SignalTypeOpen) == false)
            {
                result += OsLocalization.Trader.Label405 + ": " + SignalTypeOpen + "\n";
            }

            if (State == PositionStateType.Done
                    && string.IsNullOrEmpty(SignalTypeClose) == false)
            {
                result += OsLocalization.Trader.Label406 + ": " + SignalTypeClose + "\n";
            }

            return result;
        }
    }

    // profit for the portfolio

    /// <summary>
    /// The amount of profit relative to the portfolio in percentage
    /// </summary>
    public decimal ProfitPortfolioPercent
    {
        get
        {
            if (PortfolioValueOnOpenPosition == 0)
            {
                return 0;
            }

            return ProfitPortfolioAbs / PortfolioValueOnOpenPosition * 100;
        }
    }

    public Commission Commission;

    /// <summary>
    /// the amount of profit relative to the portfolio in absolute terms
    /// taking into account the commission and the price step
    /// </summary>
    public decimal ProfitPortfolioAbs
    {
        get
        {
            decimal volume = 0;

            for (int i = 0; OpenOrders != null && i < OpenOrders.Count; i++)
            {
                volume += OpenOrders[i].VolumeExecute;
            }

            if (PriceStepCost == 0)
            {
                PriceStepCost = 1;
            }

            if (volume == 0 ||
                    PriceStepCost == 0 ||
                    MaxVolume == 0)
            {
                return 0;
            }

            if (Lots == 0)
            {
                Lots = 1;
            }

            if (ProfitOperationAbs == 0)
            {
                CalculateProfitToPosition();
            }

            decimal absProfit = ProfitOperationAbs * PriceStepCost * MaxVolume;

            if (PriceStep != 0)
            {
                absProfit /= PriceStep;
            }

            if (IsLotServer())
            {
                absProfit *= Lots;
            }

            return absProfit - CommissionTotal();
        }
    }

    /// <summary>
    /// Determines whether the exchange supports multiple securities in one lot.
    /// </summary>
    private bool IsLotServer()
    {
        if (OpenOrders == null || OpenOrders.Count == 0)
        {
            return false;
        }

        ServerType serverType = OpenOrders[0].ServerType;

        if (serverType == ServerType.Tester ||
                serverType == ServerType.None ||
                serverType == ServerType.Optimizer)
        {
            return true;
        }

        if (serverType == ServerType.QuikDde
                || serverType == ServerType.QuikLua
                || serverType == ServerType.Plaza)
        {
            return true;
        }

        // FIX: Connect servers

        // IServerPermission permission = ServerMaster.GetServerPermission(serverType);
        //
        // if (permission == null)
        // {
        //     return false;
        // }

        // return permission.IsUseLotToCalculateProfit;
        return false;
    }

    /// <summary>
    /// The amount of total position's commission
    /// </summary>
    public decimal CommissionTotal()
    {
        if (CommissionType == CommissionType.None
                || CommissionValue == 0
                || EntryPrice == 0)
        {
            return 0;
        }

        decimal volume = MaxVolume;

        if (Lots != 0 && IsLotServer())
        {
            volume *= Lots;
        }

        if (CommissionType == CommissionType.Percent)
        {
            return volume * (EntryPrice + ClosePrice) * (CommissionValue / 100);
        }
        else
        {
            if (ClosePrice == 0)
            {
                return volume * CommissionValue;
            }
            else
            {
                return volume * CommissionValue * 2;
            }
        }
    }

    /// <summary>
    /// The number of lots in one transaction
    /// </summary>
    public decimal Lots;

    public decimal PriceStepCost;

    public decimal PriceStep;

    /// <summary>
    /// Portfolio size at the time of opening the portfolio
    /// </summary>
    // TODO: That parameter probably should not defined here
    [JsonIgnore]
    public decimal PortfolioValueOnOpenPosition;

    public string PortfolioName
    {
        get
        {
            if (OpenOrders != null
                    && OpenOrders.Count > 0)
            {
                return OpenOrders[0].PortfolioNumber;
            }

            return null;
        }

    }
}


/// <summary>
/// Way to open a deal
/// </summary>
public enum PositionOpenType : byte
{
    /// <summary>
    /// Bid at a certain price
    /// </summary>
    Limit,

    /// <summary>
    /// Application at any price
    /// </summary>
    Market,

    /// <summary>
    /// Iceberg application. Application consisting of several limit orders
    /// </summary>
    Iceberg
}

/// <summary>
/// Transaction status
/// </summary>
// NOTE: Maybe Flags will suit better
public enum PositionStateType
{
    None,

    Opening,

    /// <summary>
    /// Closed
    /// </summary>
    Done,
    Closed = Done,

    /// <summary>
    /// Error
    /// </summary>
    OpeningFail,
    Error = OpeningFail,

    /// <summary>
    /// Opened
    /// </summary>
    Open,
    Opened = Open,

    // NOTE: Very stange as position can close one
    // and open other so it not closing really
    // Probably better to delete
    Closing,

    ClosingFail,

    /// <summary>
    /// Brute force during closing
    /// </summary>
    // NOTE: Not sure how or why it can be
    ClosingSurplus,

    // NOTE: Not sure if this feature needed
    Deleted
}

/// <summary>
/// Transaction direction
/// </summary>
public enum Side : byte
{
    // NOTE: Practically not used except some cases
    None,
    Buy,
    Sell
}

public enum CommissionType
{
    None,

    /// <summary>
    /// In percentage terms
    /// </summary>
    Percent,

    /// <summary>
    /// Fixed value per lot
    /// </summary>
    OneLotFix,
    Fixed = OneLotFix,
}

public class Commission
{
    public decimal Value { get; internal set; }
    public CommissionType Type { get; internal set; }
}

public class Signals
{
    public string Open;
    public string Close;
    public string Profit;
    public string Stop;
}
