using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;

namespace OsEngine.Models.Terminal;

public partial class Journal
{
    private static readonly List<Journal> _journals = [];

    private readonly StartProgram _startProgram;
    private bool _needToSave = false;
    private List<PositionOpenerToStopLimit> _actualStopLimits;

    public readonly string Name;
    public decimal PositionMultiplier = 100;

    public ObservableCollection<Position> AllPositions { get; private set; } = [];

    public ObservableCollection<Position> OpenPositions { get; private set; } = [];
    public ObservableCollection<Position> OpenLongPositions { get; } = [];
    public ObservableCollection<Position> OpenShortPositions { get; } = [];


    public ObservableCollection<Position> ClosedPositions { get; } = [];
    // NOTE: Maybe it would be better to make it lazy?
    public List<Position> ClosedShortPositions { get; } = [];
    public List<Position> ClosedLongPositions { get; } = [];

    public Position LastPosition => AllPositions.LastOrDefault();

    public required Commission Commission;

    public CommissionType CommissionType
    {
        get;
        set
        {
            if (value == field) { return; }
            field = value;

            foreach (Position p in AllPositions)
            {
                p.CommissionType = field;
            }

            _needToSave = true;
        }
    }

    public decimal CommissionValue
    {
        get;
        set
        {
            if (value == field) { return; }
            field = value;

            // NOTE: CommissionValue as well as CommissionType
            // can be replaced with commission class
            // that assigned by journal and will be shared between positions 
            foreach (Position p in AllPositions)
            {
                p.CommissionValue = field;
            }

            _needToSave = true;
        }

    }

    public Journal(string name, StartProgram startProgram)
    {
        Name = name;
        _startProgram = startProgram;

        if (_startProgram != StartProgram.IsOsOptimizer)
        {
            _journals.Add(this);
            Load();
        }
    }

    #region Working with a position

    public void SetNewPosition(Position newPosition)
    {
        if (newPosition == null) { return; }
        // saving
        // сохраняем

        newPosition.Commission = Commission;

        AllPositions.Add(newPosition);

        OpenPositions.Add(newPosition);

        if (newPosition.Direction == Side.Buy)
            OpenLongPositions.Add(newPosition);
        else
            OpenShortPositions.Add(newPosition);

        _needToSave = true;
    }

    public void DeletePosition(Position position)
    {
        if (_deals.Count == 0)
        {
            return;
        }

        // убираем в общем хранилище

        // NOTE: If position get from somewhere here
        // it probably can be simplified to just .Remove(position)
        var pos = _deals.FirstOrDefault(p => p.Number == position.Number);
        _deals.Remove(pos);

        OpenPositions.Remove(pos);
        OpenLongPositions.Remove(pos);
        OpenShortPositions.Remove(pos);

        ClosedPositions.Remove(pos);
        ClosedLongPositions.Remove(pos);
        ClosedShortPositions.Remove(pos);

        /* for (int i = 0; i < _deals.Count; i++)
        {
            if (_deals[i].Number == position.Number)
            {
                _deals.RemoveAt(i);
                break;
            }
        }

        // убираем в хранилищах открытых позиций

        for (int i = 0; i < _openPositions.Count; i++)
        {
            if (_openPositions[i].Number == position.Number)
            {
                _openPositions.RemoveAt(i);
                break;
            }
        }

        for (int i = 0; _openLongPosition != null && i < _openLongPosition.Count; i++)
        {
            if (_openLongPosition[i].Number == position.Number)
            {
                _openLongPosition.RemoveAt(i);
                break;
            }
        }

        for (int i = 0; _openShortPositions != null && i < _openShortPositions.Count; i++)
        {
            if (_openShortPositions[i].Number == position.Number)
            {
                _openShortPositions.RemoveAt(i);
                break;
            }
        }

        // убираем из хранилищь закрытых позиций

        for (int i = 0; _closePositions != null && i < _closePositions.Count; i++)
        {
            if (_closePositions[i].Number == position.Number)
            {
                _closePositions.RemoveAt(i);
                break;
            }
        }

        for (int i = 0; _closeLongPositions != null && i < _closeLongPositions.Count; i++)
        {
            if (_closeLongPositions[i].Number == position.Number)
            {
                _closeLongPositions.RemoveAt(i);
                break;
            }
        }

        for (int i = 0; _closeShortPositions != null && i < _closeShortPositions.Count; i++)
        {
            if (_closeShortPositions[i].Number == position.Number)
            {
                _closeShortPositions.RemoveAt(i);
                break;
            }
        }

        _openLongChanged = true;
        _openShortChanged = true;
        _closePositionChanged = true;
        _closeShortChanged = true;
        _closeLongChanged = true;

        ProcessPosition(position); */

        _needToSave = true;
    }

    public void SetUpdateOrderInPositions(Order updateOrder)
    {
        foreach ( Position position in _deals )
        for (int i = _deals.Count - 1; i > -1; i--)
        {
            Position curPosition;
            try
            {
                curPosition = _deals[i];
            }
            catch
            {
                continue;
            }

            bool isCloseOrder = false;

            if (curPosition.CloseOrders != null
                    && curPosition.CloseOrders.Count > 0)
            {
                var CloseOrder = curPosition.CloseOrders
                    .Find(o => o.NumberUser == updateOrder.NumberUser);
                if (CloseOrder != null) { isCloseOrder = true; }

                /* for (int indexCloseOrders = 0; indexCloseOrders < curPosition.CloseOrders.Count; indexCloseOrders++)
                   {
                   if (curPosition.CloseOrders[indexCloseOrders].NumberUser == updateOrder.NumberUser)
                   {
                   isCloseOrder = true;
                   break;
                   }
                   } */
            }

            bool isOpenOrder = false;

            if (isCloseOrder == false ||
                    curPosition.OpenOrders != null
                    && curPosition.OpenOrders.Count > 0)
            {
                var OpenOrder = curPosition.OpenOrders
                    .Find(o => o.NumberUser == updateOrder.NumberUser);
                if (OpenOrder != null) { isOpenOrder = true; }

                /* for (int indexOpenOrd = 0; indexOpenOrd < curPosition.OpenOrders.Count; indexOpenOrd++)
                {
                    if (curPosition.OpenOrders[indexOpenOrd].NumberUser == updateOrder.NumberUser)
                    {
                        isOpenOrder = true;
                        break;
                    }
                } */
            }

            if (isOpenOrder || isCloseOrder)
            {
                PositionStateType positionState = curPosition.State;
                decimal lastPosVolume = curPosition.OpenVolume;

                curPosition.SetOrder(updateOrder);

                if (positionState != curPosition.State ||
                        lastPosVolume != curPosition.OpenVolume)
                {
                    // _openLongChanged = true;
                    // _openShortChanged = true;
                    // _closePositionChanged = true;
                    // _closeShortChanged = true;
                    // _closeLongChanged = true;

                    UpdateOpenPositionArray(curPosition);
                }

                if (positionState != curPosition.State)
                {
                    PositionStateChangeEvent?.Invoke(curPosition);
                }

                if (lastPosVolume != curPosition.OpenVolume)
                {
                    PositionNetVolumeChangeEvent?.Invoke(curPosition);
                }

                // if (i < _deals.Count)
                // {
                //     ProcessPosition(curPosition);
                // }

                break;
            }
        }
        _needToSave = true;
    }

    public Order IsMyOrder(Order order)
    {
        // open positions. Look All

        ObservableCollection<Position> positionsOpen = OpenPositions;

        if(positionsOpen != null)
        {
            for (int i = positionsOpen.Count - 1; i > -1; i--)
            {
                Position positionCurrent = positionsOpen[i];

                List<Order> openOrders = positionCurrent.OpenOrders;

                if (openOrders != null
                        && openOrders.Find(order1 => order1.NumberUser == order.NumberUser) != null)
                {
                    return openOrders.Find(order1 => order1.NumberUser == order.NumberUser);
                }

                List<Order> closingOrders = positionCurrent.CloseOrders;

                if (closingOrders != null
                        && closingOrders.Find(order1 => order1.NumberUser == order.NumberUser) != null)
                {
                    return closingOrders.Find(order1 => order1.NumberUser == order.NumberUser);
                }
            }
        }

        // historical positions. Look last 100

        ObservableCollection<Position> positions = AllPosition;

        if (positions == null)
        {
            return null;
        }

        for (int i = positions.Count - 1; i > -1 && i > positions.Count - 100; i--)
        {
            Position positionCurrent = positions[i];

            List<Order> openOrders = positionCurrent.OpenOrders;

            if (openOrders != null 
                    && openOrders.Find(order1 => order1.NumberUser == order.NumberUser) != null)
            {
                return openOrders.Find(order1 => order1.NumberUser == order.NumberUser);
            }
            List<Order> closingOrders = positionCurrent.CloseOrders;

            if (closingOrders != null 
                    && closingOrders.Find(order1 => order1.NumberUser == order.NumberUser) != null)
            {
                return closingOrders.Find(order1 => order1.NumberUser == order.NumberUser);
            }
        }

        return null;
    }

    public bool SetNewMyTrade(MyTrade trade)
    {
        for (int i = _deals.Count - 1; i > -1; i--)
        {
            Position position = _deals[i];

            if(position == null)
            {
                continue;
            }

            bool isCloseOrder = false;

            if (position.CloseOrders != null)
            {
                var closeOrder = position.CloseOrders
                    .Find(o => o.NumberMarket == trade.NumberOrderParent);
                if (closeOrder != null) { isCloseOrder = true; }

                /* for (int indexCloseOrd = 0; indexCloseOrd < position.CloseOrders.Count; indexCloseOrd++)
                {
                    Order closeOrder = position.CloseOrders[indexCloseOrd];

                    if(closeOrder == null)
                    {
                        continue;
                    }

                    if (closeOrder.NumberMarket == trade.NumberOrderParent)
                    {
                        isCloseOrder = true;
                        break;
                    }
                } */
            }
            bool isOpenOrder = false;

            if (isCloseOrder == false &&
                    position.OpenOrders != null 
                    && position.OpenOrders.Count > 0)
            {
                var openOrder = position.OpenOrders
                    .Find(o => o.NumberMarket == trade.NumberOrderParent);
                if (openOrder != null) { isOpenOrder = true; }

                /* for (int indOpenOrd = 0; indOpenOrd < position.OpenOrders.Count; indOpenOrd++)
                {
                    Order openOrder = position.OpenOrders[indOpenOrd];

                    if(openOrder == null)
                    {
                        continue;
                    }

                    if (openOrder.NumberMarket == trade.NumberOrderParent)
                    {
                        isOpenOrder = true;
                        break;
                    }
                } */
            }

            if (isOpenOrder || isCloseOrder)
            {
                PositionStateType positionState = position.State;

                decimal lastPosVolume = position.OpenVolume;

                position.SetTrade(trade);

                if (positionState != position.State ||
                        lastPosVolume != position.OpenVolume)
                {
                    UpdateOpenPositionArray(position);
                    // _openLongChanged = true;
                    // _openShortChanged = true;
                    // _closePositionChanged = true;
                    // _closeShortChanged = true;
                    // _closeLongChanged = true;
                }

                if (positionState != position.State)
                {
                    PositionStateChangeEvent?.Invoke(position);
                }

                if (lastPosVolume != position.OpenVolume)
                {
                    PositionNetVolumeChangeEvent?.Invoke(position);
                }

                _needToSave = true;
                return true;
            }
        }
        return false;
    }

    public void SetBidAsk(decimal bid, decimal ask)
    {
        var positions = OpenPositions;

        if (positions.Count == 0) { return; }
        if (_startProgram == StartProgram.IsOsOptimizer) { return; }

        for (int i = positions.Count - 1; i > -1; i--)
        {
            if (positions[i].State == PositionStateType.Open
                    || positions[i].State == PositionStateType.Closing
                    || positions[i].State == PositionStateType.ClosingFail)
            {
                // decimal profitOld = positions[i].ProfitOperationAbs;

                positions[i].SetBidAsk(bid, ask);

                // if (profitOld != positions[i].ProfitOperationAbs)
                // {
                //     ProcessPosition(positions[i]);
                // }
            }
        }
    }

    #endregion

    #region Access to transactions

    private void UpdateOpenPositionArray(Position position)
    {
        if (position.State != PositionStateType.Done
                && position.State != PositionStateType.OpeningFail)
        {
            if (OpenPositions.FirstOrDefault(p => p.Number == position.Number) == null)
            {
                OpenPositions.Add(position);
                if (position.IsBuy)
                    OpenLongPositions.Add(position);
                else
                    OpenShortPositions.Add(position);
            }
        }
        else
        {
            OpenPositions.Remove(position);
            if (position.IsBuy)
                OpenLongPositions.Remove(position);
            else
                OpenShortPositions.Remove(position);
        }
    }

    public Position GetPositionForNumber(int number) =>
        AllPositions.LastOrDefault(p => p.Number == number);

    #endregion

    #region Log

    private void SendNewLogMessage(string message, LogMessageType type)
    {
        if (LogMessageEvent != null)
        {
            LogMessageEvent(message, type);
        }
        else if (type == LogMessageType.Error)
        {
            MessageBox.Show(message);
        }
    }

    public event Action<string, LogMessageType> LogMessageEvent;

    #endregion

    #region Events

    private void OnPositionStateChanged(Position position)
    {
        try
        {
            PositionStateChanged?.Invoke(position);
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    private void OnPositionNetVolumeChanged(Position position)
    {
        try
        {
            PositionNetVolumeChanged?.Invoke(position);
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    private void OnUserSelectedAction(Position pos, SignalType signal)
    {
        try
        {
            UserSelectedAction?.Invoke(pos, signal);
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    public event Action<Position> PositionStateChanged;

    public event Action<Position> PositionNetVolumeChanged;

    public event Action<Position, SignalType> UserSelectedAction;

    #endregion
}

public class BotsGroup
{
    public string Name { get; set; }
    public ObservableCollection<Journal> Bots { get; set; }
}
