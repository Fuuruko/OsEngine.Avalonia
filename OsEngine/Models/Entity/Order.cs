/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using OsEngine.Models.Market;

namespace OsEngine.Models.Entity;

public class Order
{
    /// <summary>
    /// Order number in the robot
    /// </summary>
    public int NumberUser;

    /// <summary>
    /// Order number on the exchange
    /// </summary>
    // NOTE: Convert to ulong? Or long and emulator orders will be negative
    public string NumberMarket = string.Empty;

    /// <summary>
    /// Instrument code for which the transaction took place
    /// </summary>
    public string SecurityNameCode
    {
        get;
        set => field = string.Intern(value);
    }

    /// <summary>
    /// Code of the class to which the security belongs
    /// </summary>
    public string SecurityClassCode
    {
        get;
        set => field = string.Intern(value);
    }

    /// <summary>
    /// Account number to which the order belongs
    /// </summary>
    public string PortfolioNumber
    {
        get;
        set => field = string.Intern(value);
    }

    /// <summary>
    /// Direction
    /// </summary>
    public Side Side = Side.None;
    public bool IsBuy => Side == Side.Buy;
    public bool IsSell => Side == Side.Sell;

    /// <summary>
    /// Bid price
    /// </summary>
    // NOTE: Make it init?
    public decimal Price { get; set; }

    /// <summary>
    /// Real price
    /// </summary>
    public decimal PriceReal
    {
        get
        {
            if ((State == OrderStateType.None
                || State == OrderStateType.Active
                || State == OrderStateType.Cancel)
                && MyTrades == null)
            {
                return 0;
            }

            if (MyTrades == null)
            {
                return Price;
            }
            decimal price = 0;

            decimal volumeExecute = 0;

            for (int i = 0; i < MyTrades.Count; i++)
            {
                if (MyTrades[i] == null)
                {
                    continue;
                }

                price += MyTrades[i].Volume * MyTrades[i].Price;
                volumeExecute += MyTrades[i].Volume;
            }

            if (volumeExecute == 0)
            {
                return Price;
            }

            price /= volumeExecute;

            return price;
        }
    }

    public decimal Volume;

    /// <summary>
    /// Execute volume
    /// </summary>
    public decimal VolumeExecute
    {
        get
        {
            if (MyTrades != null && (_volumeExecute == 0 || _volumeExecuteChange))
            {
                _volumeExecute = 0;

                for (int i = 0; i < MyTrades.Count; i++)
                {
                    if (MyTrades[i] == null)
                    {
                        continue;
                    }

                    _volumeExecute += MyTrades[i].Volume;
                }

                _volumeExecuteChange = false;
                return _volumeExecute;
            }
            else
            {
                if (_volumeExecute == 0 && State == OrderStateType.Done)
                {
                    return Volume;
                }
                return _volumeExecute;
            }

        }
        set => _volumeExecute = value;
    }
    private decimal _volumeExecute;
    private bool _volumeExecuteChange;

    /// <summary>
    /// My trades belonging to this order
    /// </summary>
    public List<MyTrade> MyTrades { get; private set; }

    /// <summary>
    /// Order status: None, Pending, Done, Partial, Fail
    /// </summary>
    public OrderStateType State
    {
        get => _state;
        set
        {
            if (value == OrderStateType.Fail
                && MyTrades != null
                && MyTrades.Count > 1)
            {
                return;
            }

            if (value == OrderStateType.Fail
                &&
                (State == OrderStateType.Done
                || State == OrderStateType.Partial
                || State == OrderStateType.Cancel))
            {
                return;
            }

            if ((value == OrderStateType.Active)
                &&
                (_state == OrderStateType.Done
                || _state == OrderStateType.Partial
                )
                )
            {
                return;
            }

            _state = value;
        }
    }

    private OrderStateType _state = OrderStateType.None;

    /// <summary>
    /// Order price type. Limit, Market
    /// </summary>
    public OrderPriceType TypeOrder;

    public bool IsMarket => TypeOrder == OrderPriceType.Market;
    public bool IsLimit => TypeOrder == OrderPriceType.Limit;

    /// <summary>
    /// Why the order was created in the context of the position. Open is the opening order. Close is the closing order
    /// </summary>
    // NOTE: Use bool instead or maybe it can be removed at all
    public OrderPositionConditionType PositionConditionType;

    public bool IsOpenOrder => PositionConditionType == OrderPositionConditionType.Open;
    // public bool IsCloseOrder => PositionConditionType == OrderPositionConditionType.Close;

    /// <summary>
    /// User comment
    /// </summary>
    public string Comment;

    /// <summary>
    /// Time of the first response from the stock exchange on the order. Server time
    /// </summary>
    [Obsolete($"Use {nameof(CallBackTime)} instead")]
    public DateTime TimeCallBack
    {
        get => CallBackTime;
        set => CallBackTime = value;
    }
    public DateTime CallBackTime = DateTime.MinValue;

    /// <summary>
    /// Time of order removal from the system. Server time
    /// </summary>
    [Obsolete($"Use {nameof(CancelTime)} instead")]
    public DateTime TimeCancel
    {
        get => CancelTime;
        set => CancelTime = value;
    }
    public DateTime CancelTime = DateTime.MinValue;

    /// <summary>
    /// Order execution time. Server time
    /// </summary>
    [Obsolete($"Use {nameof(FillTime)} instead")]
    public DateTime TimeDone
    {
        get => FillTime;
        set => FillTime = value;
    }
    public DateTime FillTime = DateTime.MinValue;

    /// <summary>
    /// Order creation time in OsApi. Server time
    /// </summary>
    [Obsolete($"Use {nameof(CreateTime)} instead")]
    public DateTime TimeCreate
    {
        get => CreateTime;
        set => CreateTime = value;
    }
    public DateTime CreateTime = DateTime.MinValue;

    /// <summary>
    /// Bidding rate
    /// </summary>
    public TimeSpan TimeRoundTrip
    {
        get
        {
            if (TimeCallBack == DateTime.MinValue ||
                TimeCreate == DateTime.MinValue)
            {
                return new TimeSpan(0, 0, 0, 0);
            }

            return TimeCallBack - TimeCreate;
        }
    }

    /// <summary>
    /// Time when the order was the first transaction
    /// if there are no deals on the order yet, it will return the time to create the order
    /// </summary>
    public DateTime TimeExecuteFirstTrade
    {
        get
        {
            if (MyTrades == null ||
                MyTrades.Count == 0)
            {
                return TimeCreate;
            }

            if (MyTrades[0] != null)
            {
                return MyTrades[0].Time;
            }

            return TimeCreate;
        }
    }

    /// <summary>
    /// Lifetime on the exchange, after which the order must be withdrawn
    /// </summary>
    public TimeSpan LifeTime;

    /// <summary>
    /// Order lifetime type
    /// </summary>
    public OrderLifetime OrderTypeTime_;
    [Obsolete($"Use {nameof(OrderTypeTime_)} instead")]
    public OrderTypeTime OrderTypeTime;

    /// <summary>
    /// Flag saying that this order was created to close by stop or profit order
    /// the tester needs to perform it adequately
    /// </summary>
    // NOTE: Probably can be moved or removed
    public bool IsStopOrProfit;

    // NOTE: Used only to find server and execute operation on order
    // Can be simplified and removed
    public string ServerName;

    // NOTE: Used only to find server and execute operation on order
    // Can be simplified and removed
    public ServerType ServerType;

    // NOTE: Probably can be removed
    public TimeFrame TimeFrameInTester;

    // deals with which the order was opened and calculation of the order execution price

    /// <summary>
    /// Order trades
    /// </summary>

    /// <summary>
    /// Heck the ownership of the transaction to this order
    /// </summary>
    public void SetTrade(MyTrade trade)
    {
        if (trade.NumberOrderParent != NumberMarket)
        {
            return;
        }

        if (MyTrades != null)
        {
            for (int i = 0; i < MyTrades.Count; i++)
            {
                if (MyTrades[i] == null)
                {
                    continue;
                }
                if (MyTrades[i].NumberTrade == trade.NumberTrade)
                {
                    return;
                }
            }
        }
        else
        {
            MyTrades = [];
        }

        MyTrades.Add(trade);

        _volumeExecuteChange = true;

        if (Volume == VolumeExecute)
        {
            State = OrderStateType.Done;
        }

        if (State == OrderStateType.Fail)
        {
            State = OrderStateType.Partial;
        }
    }

    /// <summary>
    /// Take the average order execution price
    /// </summary>
    private decimal GetMiddlePrice()
    {
        if (MyTrades == null)
        {
            return Price;
        }
        decimal price = 0;

        decimal volumeExecute = 0;

        for (int i = 0; i < MyTrades.Count; i++)
        {
            if (MyTrades[i] == null)
            {
                continue;
            }

            price += MyTrades[i].Volume * MyTrades[i].Price;
            volumeExecute += MyTrades[i].Volume;
        }

        if (volumeExecute == 0)
        {
            return Price;
        }

        price /= volumeExecute;

        return price;
    }

    /// <summary>
    /// Take the time of execution of the last trade on the order
    /// </summary>
    public DateTime GetLastTradeTime()
    {
        if (MyTrades == null)
        {
            return TimeCallBack;
        }
        if (MyTrades.Count == 0)
        {
            return TimeCallBack;
        }
        if (MyTrades[0] == null)
        {
            return TimeCallBack;
        }
        return MyTrades[^1].Time;
    }

    /// <summary>
    /// Whether the trades of this order came to the array
    /// </summary>
    public bool TradesIsComing => MyTrades != null && MyTrades.Count != 0;

    // NOTE: Useless
    private static readonly CultureInfo CultureInfo = new("ru-RU");

    /// <summary>
    /// Take the string to save
    /// </summary>
    public StringBuilder GetStringForSave()
    {
        if (_saveString != null)
        {
            return _saveString;
        }

        StringBuilder result = new();

        result.Append(NumberUser + "@");

        result.Append(ServerType + "@");

        result.Append(NumberMarket.ToString(CultureInfo) + "@");
        result.Append(Side + "@");
        result.Append(Price.ToString(CultureInfo) + "@");
        result.Append(PriceReal.ToString(CultureInfo) + "@");
        result.Append(Volume.ToString(CultureInfo) + "@");
        result.Append(VolumeExecute.ToString(CultureInfo) + "@");
        result.Append(State + "@");
        result.Append(TypeOrder + "@");
        result.Append(TimeCallBack.ToString(CultureInfo) + "@");
        result.Append(SecurityNameCode + "@");

        if (PortfolioNumber != null)
        {
            result.Append(PortfolioNumber.Replace('@', '%') + "@");
        }
        else
        {
            result.Append("" + "@");
        }

        result.Append(TimeCreate.ToString(CultureInfo) + "@");
        result.Append(TimeCancel.ToString(CultureInfo) + "@");
        result.Append(TimeCallBack.ToString(CultureInfo) + "@");

        result.Append(LifeTime + "@");

        // deals with which the order was opened and the order execution price was calculated

        if (MyTrades == null)
        {
            result.Append("null");
        }
        else
        {
            for (int i = 0; i < MyTrades.Count; i++)
            {
                if (MyTrades[i] == null)
                {
                    continue;
                }

                result.Append(MyTrades[i].GetStringFofSave() + "*");
            }
        }
        result.Append("@");

        result.Append(Comment + "@");

        result.Append(TimeDone.ToString(CultureInfo) + "@");

        result.Append(OrderTypeTime + "@");

        result.Append(ServerName + "@");

        if (State == OrderStateType.Done && Volume == VolumeExecute &&
            MyTrades != null && MyTrades.Count > 0)
        {
            _saveString = result;
        }

        return result;
    }

    private StringBuilder _saveString;

    /// <summary>
    /// Load order from incoming line
    /// </summary>
    public void SetOrderFromString(string saveString)
    {
        string[] saveArray = saveString.Split('@');
        NumberUser = Convert.ToInt32(saveArray[0]);

        Enum.TryParse(saveArray[1], true, out ServerType);

        NumberMarket = saveArray[2];
        Enum.TryParse(saveArray[3], true, out Side);
        Price = saveArray[4].ToDecimal();

        Volume = saveArray[6].ToDecimal();
        VolumeExecute = saveArray[7].ToDecimal();

        Enum.TryParse(saveArray[8], true, out _state);
        Enum.TryParse(saveArray[9], true, out TypeOrder);
        TimeCallBack = Convert.ToDateTime(saveArray[10], CultureInfo);

        SecurityNameCode = saveArray[11];
        PortfolioNumber = saveArray[12].Replace('%', '@');


        TimeCreate = Convert.ToDateTime(saveArray[13], CultureInfo);
        TimeCancel = Convert.ToDateTime(saveArray[14], CultureInfo);
        TimeCallBack = Convert.ToDateTime(saveArray[15], CultureInfo);

        TimeSpan.TryParse(saveArray[16], out LifeTime);

        // deals with which the order was opened and the order execution price was calculated

        if (saveArray[17] == "null")
        {
            MyTrades = null;
        }
        else
        {
            string[] tradesArray = saveArray[17].Split('*');

            MyTrades = [];

            for (int i = 0; i < tradesArray.Length - 1; i++)
            {
                MyTrades.Add(new MyTrade());
                MyTrades[i].SetTradeFromString(tradesArray[i]);
            }
        }
        Comment = saveArray[18];
        TimeDone = Convert.ToDateTime(saveArray[19], CultureInfo);

        if (saveArray.Length > 21)
        {
            Enum.TryParse(saveArray[20], true, out OrderTypeTime);
        }

        if (saveArray.Length > 22)
        {
            ServerName = saveArray[21];
        }
    }
}

/// <summary>
/// Price type for order
/// </summary>
public enum OrderPriceType : byte
{
    /// <summary>
    /// Limit order. Those. bid at a certain price
    /// </summary>
    Limit,

    /// <summary>
    /// Market application. Those. application at any price
    /// </summary>
    Market,

    /// <summary>
    /// Iceberg application. Those. An application whose volume is not fully visible in the glass.
    /// </summary>
    // NOTE: Not used anywhere
    Iceberg
}

/// <summary>
/// Order status
/// </summary>
// NOTE: Make it Flags
public enum OrderStateType : byte
{
    None = 1,

    /// <summary>
    /// Accepted by the exchange and exhibited in the system
    /// </summary>
    Active = 2,

    /// <summary>
    /// Waiting for registration
    /// </summary>
    Pending = 3,

    Done = 4,

    /// <summary>
    /// Partitial done
    /// </summary>
    Partial = 5,

    /// <summary>
    /// Error
    /// </summary>
    Fail = 6,

    Cancel = 7,

    /// <summary>
    /// Status did not change after Active. Possible error
    /// </summary>
    LostAfterActive = 8,
}

/// <summary>
/// The purpose of the order, opening or closing a position
/// </summary>
// NOTE: For Headge trading? Used in 
public enum OrderPositionConditionType : byte
{
    Open,
    Close
}

public enum OrderLifetime : byte
{
    /// <summary>
    /// Order will be valid for as long as specified in the LifeTime variable
    /// </summary>
    Specified,

    /// <summary>
    ///  Order will be in the queue until it is withdrawn
    /// </summary>
    GTC,

    /// <summary>
    /// Order will be throughout the day. If the exchange has such possibilities
    /// </summary>
    Day
}

[Obsolete($"Use {nameof(OrderLifetime)} instead")]
public enum OrderTypeTime : byte
{
    /// <summary>
    /// Order will be valid for as long as specified in the LifeTime variable
    /// </summary>
    Specified,

    /// <summary>
    ///  Order will be in the queue until it is withdrawn
    /// </summary>
    GTC,

    /// <summary>
    /// Order will be throughout the day. If the exchange has such possibilities
    /// </summary>
    Day
}
