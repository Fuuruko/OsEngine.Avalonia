/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;
using OsEngine.Models.Market;
using OsEngine.Models.Market.Servers;
using OsEngine.Models.Market.Servers.Tester;

namespace OsEngine.Models.Candles;

/// <summary>
/// keeper of a series of candles. It is created in the server and participates in the process of subscribing to candles.
/// Stores a series of candles, is responsible for their loading with ticks so that candles are formed in them
/// </summary>
public partial class CandleManager
{
#region Service

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="server">the server from which the candlestick data will go/сервер из которого будут идти данные для создания свечек</param>
    /// <param name="startProgram">the program that created the class object/программа которая создала объект класса</param>
    public CandleManager(IServer server, StartProgram startProgram)
    {
        _server = server;
        _server.NewTradeEvent += OnNewTrade;
        _server.TimeServerChangeEvent += OnTimeServerChanged;
        _server.NewMarketDepthEvent += OnNewMarketDepth;

        _startProgram = startProgram;

        TypeTesterData = TesterDataType.Unknown;
    }

    /// <summary>
    /// exchange connection server
    /// </summary>
    private IServer _server;

    /// <summary>
    /// program to which the object CandleManager belongs
    /// </summary>
    private StartProgram _startProgram;

    /// <summary>
    /// clear data from the object
    /// </summary>
    // NOTE: Clear intead of Dispose(?)
    // Probably can be done without _isDisposed
    public void Dispose()
    {
        _isDisposed = true;

        if (_server != null)
        {
            _server.NewTradeEvent -= OnNewTrade;
            _server.TimeServerChangeEvent -= OnTimeServerChanged;
            _server.NewMarketDepthEvent -= OnNewMarketDepth;
        }

        _server = null;

        try
        {
            foreach (CandleSeries series in _activeSeries)
            {
                series.CandleUpdateEvent -= OnCandleUpdated;
                series.CandleFinishedEvent -= OnCandleFinished;
                series.Clear();
                series.Stop();
            }
        }
        catch
        {
            // ignore
        }

        _activeSeries.Clear();
        _activeSeriesBasedOnMd.Clear();
        _activeSeriesBasedOnTrades.Clear();
    }

    private bool _isDisposed;

    #endregion

    #region Candle series storing and activation

    /// <summary>
    /// start creating candles in a new series of candles
    /// </summary>
    public void StartSeries(CandleSeries series)
    {
        try
        {
            if (_server.ServerType != ServerType.Tester &&
                _server.ServerType != ServerType.Optimizer)
            {
                series.CandleUpdateEvent += OnCandleUpdated;
            }

            series.TypeTesterData = TypeTesterData;
            series.CandleFinishedEvent += OnCandleFinished;

            if (_startProgram == StartProgram.IsOsTrader)
            {
                Task.Run(() => StartTradeSeries(series));
            }
            else
            {
                _activeSeries.Add(series);
                if (series.CandleMarketDataType == CandleMarketDataType.MarketDepth)
                {
                    _activeSeriesBasedOnMd.Add(series);
                }
                else if (series.CandleMarketDataType == CandleMarketDataType.Tick)
                {
                    _activeSeriesBasedOnTrades.Add(series);
                }
                series.IsStarted = true;
            }
        }
        catch (Exception error)
        {
            SendLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// method for operating the thread triggering candle series
    /// </summary>
    private void StartTradeSeries(CandleSeries series)
    {
        try
        {
            if (_isDisposed
                || MainWindow.ProccesIsWorked == false
                || series == null
                || series.IsStarted)
            {
                return;
            }

            ServerType serverType = _server.ServerType;

            // IServerPermission permissions = ServerMaster.GetServerPermission(serverType);
            ServerPermissions permissions = _server.Permissions;

            if (permissions?.UsesStandardCandlesStarter == true)
            {
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // NEW STANDART CANDLE SERIES START 2024
                if (series.CandleCreateMethodType != "Simple" ||
                    series.TimeFrameSpan.TotalMinutes < 1)
                {
                    List<Trade> allTrades = _server.GetAllTradesToSecurity(series.Security);
                    series.PreLoad(allTrades);
                }
                else
                {
                    List<Candle> candles = _server.GetLastCandleHistory(series.Security, series.TimeFrameBuilder);

                    if (candles != null)
                    {
                        series.CandlesAll = candles;
                    }
                }

                series.UpdateAllCandles();
            }
            else if (serverType == ServerType.Plaza
                     || serverType == ServerType.QuikDde
                     || serverType == ServerType.AstsBridge
                     || serverType == ServerType.NinjaTrader
                     || serverType == ServerType.Lmax
                     || serverType == ServerType.MoexFixFastSpot)
            {
                // XXX: Here might be error in original
                // because CandlesAll is set to null and preload try to index it
                series.CandlesAll.Clear();
                // further, we try to load candles with ticks
                // далее, пытаемся пробуем прогрузить свечи при помощи тиков
                List<Trade> allTrades = _server.GetAllTradesToSecurity(series.Security);

                series.PreLoad(allTrades);
                // if there is a preloading of candles on the server and something is downloaded
                // если на сервере есть предзагрузка свечек и что-то скачалось 
                series.UpdateAllCandles();
            }
            else if (serverType == ServerType.Tester ||
                     serverType == ServerType.Optimizer ||
                     serverType == ServerType.BitStamp
                    )
            {

            }
            // FIX: What fix?
            else if (serverType == ServerType.InteractiveBrokers
                     || serverType == ServerType.Bitmax_AscendexFutures
                     || serverType == ServerType.Hitbtc
                     || serverType == ServerType.Zb
                    )
            {
                if (series.CandleCreateMethodType != "Simple" ||
                    series.TimeFrameSpan.TotalMinutes < 1)
                {
                    List<Trade> allTrades = _server.GetAllTradesToSecurity(series.Security);
                    series.PreLoad(allTrades);
                }
                else
                {
                    List<Candle> candles = _server.GetCandleHistory(series.Security.Name,
                                                                    series.TimeFrame);
                    if (candles != null)
                    {
                        series.CandlesAll = candles;
                    }
                }
                series.UpdateAllCandles();
            }
            else if (serverType == ServerType.Exmo)
            {
                List<Trade> allTrades = _server.GetAllTradesToSecurity(series.Security);

                series.PreLoad(allTrades);
                series.UpdateAllCandles();
            }

            series.IsStarted = true;

            _activeSeries.Add(series);
            if (series.CandleMarketDataType == CandleMarketDataType.MarketDepth)
            {
                _activeSeriesBasedOnMd.Add(series);
            }
            else if (series.CandleMarketDataType == CandleMarketDataType.Tick)
            {
                _activeSeriesBasedOnTrades.Add(series);
            }
        }
        catch (Exception error)
        {
            SendLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// stop loading candles by series
    /// </summary>
    public void StopSeries(CandleSeries series)
    {
        try
        {
            if (series == null) { return; }

            series.CandleUpdateEvent -= OnCandleUpdated;
            series.CandleFinishedEvent -= OnCandleFinished;


            var s = _activeSeries.Find(s => s.UID == series.UID);
            if (s == null) { return; }
            _activeSeries.Remove(s);
            _activeSeriesBasedOnMd.Remove(s);
            _activeSeriesBasedOnTrades.Remove(s);
        }
        catch
        {
            //SendLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    private List<CandleSeries> _activeSeries = [];

    /// <summary>
    /// active series collecting candlesticks from the trade tape
    /// </summary>
    private List<CandleSeries> _activeSeriesBasedOnTrades = [];

    /// <summary>
    /// active series collecting candlesticks from the market depth
    /// </summary>
    private List<CandleSeries> _activeSeriesBasedOnMd = [];

    /// <summary>
    /// Number of active candleSeries
    /// </summary>
    /* public int ActiveSeriesCount => 
       _activeSeriesBasedOnMd.Count + _activeSeriesBasedOnTrades.Count; */
    // NOTE: Used only in Tester
    public int ActiveSeriesCount => _activeSeries.Count;

    /// <summary>
    /// returns whether marketdata updates for the specified security are no longer needed
    /// </summary>
    public bool IsSafeToUnsubscribeFromSecurityUpdates(Security security)
    {
        return _activeSeries.Find(
                s => s.Security.Name == security.Name
                && s.Security.NameClass == security.NameClass) == null;
    }

    #endregion

    #region Events from server

    /// <summary>
    /// server time has changed. Inbound event
    /// </summary>
    private void OnTimeServerChanged(DateTime dateTime)
    {
        if (_isDisposed) { return; }

        try
        {
            for (int i = 0; i < _activeSeries.Count; i++)
            {
                _activeSeries[i].SetNewTime(dateTime);
            }
        }
        catch (Exception error)
        {
            SendLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// A new tick appeared in the server. Inbound event
    /// </summary>
    private void OnNewTrade(List<Trade> trades)
    {
        try
        {
            if (_isDisposed) { return; }

            if (TypeTesterData == TesterDataType.Candle
                && (_server.ServerType == ServerType.Tester 
                    || _server.ServerType == ServerType.Optimizer))
            {
                return;
            }

            if (trades == null
                || trades.Count == 0
                || trades[0] == null)
            {
                return;
            }

            // if (_activeSeriesBasedOnTrades == null)
            // {
            //     return;
            // }

            string secCode = trades[0].SecurityNameCode;

            try
            {
                for (int i = 0; i < _activeSeriesBasedOnTrades.Count; i++)
                {
                    if (_activeSeriesBasedOnTrades[i] == null ||
                        _activeSeriesBasedOnTrades[i].Security == null ||
                        _activeSeriesBasedOnTrades[i].TimeFrameBuilder.CandleSeriesRealization == null)
                    {
                        continue;
                    }
                    if (_activeSeriesBasedOnTrades[i].Security.Name == secCode)
                    {
                        _activeSeriesBasedOnTrades[i].SetNewTicks(trades);
                    }
                }
            }
            catch
            {
                // ignore
            }
        }
        catch (Exception error)
        {
            SendLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// from the server came a new market depth
    /// </summary>
    private void OnNewMarketDepth(MarketDepth marketDepth)
    {
        if (_isDisposed
            || _server == null
            || _server.ServerType == ServerType.Tester
            && TypeTesterData == TesterDataType.Candle)
        {
            return;
        }

        try
        {
            for (int i = 0; i < _activeSeriesBasedOnMd.Count; i++)
            {
                if (_activeSeriesBasedOnMd[i] == null ||
                    _activeSeriesBasedOnMd[i].Security == null)
                {
                    continue;
                }

                if (_activeSeriesBasedOnMd[i].Security.Name == marketDepth.SecurityNameCode)
                {
                    _activeSeriesBasedOnMd[i].SetNewMarketDepth(marketDepth);
                }
            }
        }
        catch (Exception error)
        {
            SendLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    #endregion

    /// <summary>
    /// candles were updated in one of the series. Inbound event
    /// </summary>
    private void OnCandleUpdated(CandleSeries series)
    {
        if (_isDisposed) { return; }

        CandleUpdated?.Invoke(series);
    }

    // NOTE: The Same as OnCandleUpdated
    private void OnCandleFinished(CandleSeries series)
    {
        if (_isDisposed) { return; }

        CandleUpdated?.Invoke(series);
    }

    /// <summary>
    /// candle refreshed
    /// обновилась свечка
    /// </summary>
    public event Action<CandleSeries> CandleUpdated;

    #region Tester

    /// <summary>
    /// loading a new candle in the series in the tester 
    /// </summary>
    // NOTE: Used in optimizer and tester
    public void SetNewCandleInSeries(Candle candle, string nameSecurity, TimeSpan timeFrame)
    {
        for (int i = 0; i < _activeSeries.Count; i++)
        {
            if (_activeSeries[i] == null
                || _activeSeries[i].Security == null)
            {
                continue;
            }

            if (_activeSeries[i].Security.Name == nameSecurity
                && _activeSeries[i].TimeFrameSpan == timeFrame)
            {
                _activeSeries[i].SetNewCandleInArray(candle);
            }
        }
    }

    /// <summary>
    /// clear series from old data
    /// </summary>
    // NOTE: Used in optimizer and tester
    public void Clear()
    {
        try
        {
            _activeSeries.ForEach(s => s.Clear());
        }
        catch
        {
            // ignore
        }
    }

    /// <summary>
    /// sync received data
    /// </summary>
    // NOTE: Should be in TesterServer not here
    public void SynhSeries(List<string> nameSecurities)
    {
        if (nameSecurities == null || nameSecurities.Count == 0)
        {
            return;
        }

        List<CandleSeries> mySeries = [];

        for (int i = 0; i < _activeSeriesBasedOnTrades.Count; i++)
        {
            if (nameSecurities.Find(nameSec => nameSec == _activeSeriesBasedOnTrades[i].Security.Name) != null)
            {
                mySeries.Add(_activeSeriesBasedOnTrades[i]);
            }
        }
        _activeSeriesBasedOnTrades = mySeries;

    }

    /// <summary>
    /// data type that tester ordered
    /// </summary>
    public TesterDataType TypeTesterData
    {
        get;
        set
        {
            field = value;
            for (int i = 0; i < _activeSeriesBasedOnTrades.Count; i++)
            {
                _activeSeriesBasedOnTrades[i].TypeTesterData = value;
            }

            for (int i = 0; i < _activeSeriesBasedOnMd.Count; i++)
            {
                _activeSeriesBasedOnMd[i].TypeTesterData = value;
            }
        }

    }

    #endregion

    private void SendLogMessage(string message, LogMessageType type)
    {
        if (LogMessageEvent != null)
        {
            LogMessageEvent(message, type);
        }
        else if (type == LogMessageType.Error
                 && _isDisposed != true)
        {
            MessageBox.Show(message);
        }
    }

    public event Action<string, LogMessageType> LogMessageEvent;
}
