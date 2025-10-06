/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OsEngine.Language;
// using OsEngine.Market.Connectors;
using OsEngine.Models.Candles;
using OsEngine.Models.Entity;
using OsEngine.Models.Entity.Server;
using OsEngine.Models.Logging;
using OsEngine.Models.Terminal;
// using OsEngine.OsTrader.Panels;
// using OsEngine.OsTrader.Panels.Tab;

namespace OsEngine.Models.Market.Servers.Tester;

public partial class TesterServer : ObservableObject, IServer, ILog
{
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

    public List<Portfolio> Portfolios { get; private set; } = [];

    public Portfolio Portfolio = new()
    {
        Number = "GodMode",
        ValueCurrent = 1_000_000,
        ValueBegin = 1_000_000,
        ValueBlocked = 0,
        ServerUniqueName = "Tester"
    };

    public TesterServer()
    {
        _logMaster.Listen(this);
        Load();
        // LoadClearingInfo();
        // LoadNonTradePeriods();

        if (ActiveSet != null)
        {
            Task.Run(LoadSecurities);
        }

        if (_worker == null)
        {
            _worker = new Thread(WorkThreadArea)
            {
                Name = "TesterServerThread",
                IsBackground = true,
                CurrentCulture = InvariantCulture,
            };
            _worker.Start();
        }

        _candleManager = new CandleManager(this, StartProgram.IsTester);
        _candleManager.CandleUpdateEvent += _candleManager_CandleUpdateEvent;
        _candleManager.LogMessageEvent += SendLogMessage;
        _candleManager.TypeTesterData = TypeTesterData;

        CheckSet();

        Portfolios.Add(Portfolio);
        PortfoliosChangeEvent?.Invoke(Portfolios);
    }

    public ServerType ServerType => ServerType.Tester;

    public string ServerNameAndPrefix => ServerType.ToString();

    #region Server status

    public ServerConnectStatus ServerStatus
    {
        get;
        private set
        {
            if (value != field)
            {
                field = value;
                SendLogMessage(field + OsLocalization.Market.Message7, LogMessageType.Connect);
                ConnectStatusChangeEvent?.Invoke(field.ToString());
            }
        }
    } = ServerConnectStatus.Disconnect;

    public event Action<string> ConnectStatusChangeEvent;

    // public int CountDaysTickNeedToSave { get; set; }

    // public bool NeedToSaveTicks { get; set; }

    public DateTime ServerTime
    {
        get => _serverTime;
        private set
        {
            if (value <= _serverTime) { return; }

            _serverTime = value;
            TimeServerChangeEvent?.Invoke(_serverTime);
        }
    }
    private DateTime _serverTime;

    public event Action<DateTime> TimeServerChangeEvent;

    #endregion

    #region Management

    public void TestingStart()
    {
        try
        {
            if (_lastStartSecurityTime.AddSeconds(5) > DateTime.Now)
            {
                SendLogMessage(OsLocalization.Market.Message97, LogMessageType.Error);
                return;
            }

            Regime = TesterRegime.Pause;
            Thread.Sleep(200);
            _serverTime = DateTime.MinValue;
            TestingFastIsActivate = false;

            // FIX:
            // ServerMaster.ClearOrders();

            SendLogMessage(OsLocalization.Market.Message35, LogMessageType.System);

            if (_candleSeriesTesterActivate != null)
            {
                for (int i = 0; i < _candleSeriesTesterActivate.Count; i++)
                {
                    _candleSeriesTesterActivate[i].Clear();
                }
            }

            _candleSeriesTesterActivate = [];

            int countSeriesInLastTest = _candleManager.ActiveSeriesCount;

            _candleManager.Clear();

            NeedToReconnectEvent?.Invoke();

            int timeToWaitConnect = 100 + countSeriesInLastTest * 60;

            if (timeToWaitConnect > 10000)
            {
                timeToWaitConnect = 10000;
            }

            if (timeToWaitConnect < 1000)
            {
                timeToWaitConnect = 1000;
            }

            Thread.Sleep(timeToWaitConnect);

            _allTrades = null;

            if (_timeStart == DateTime.MinValue)
            {
                SendLogMessage(OsLocalization.Market.Message47, LogMessageType.System);
                return;
            }

            // NOTE: Why not include seconds and ms?
            _timeNow = new DateTime(_timeStart.Year, _timeStart.Month, _timeStart.Day, _timeStart.Hour, 0, 0);

            ProfitArray.Clear();

            _dataIsActive = false;

            NumberGen.ResetToZeroInTester();

            _activeOrders.Clear();

            Thread.Sleep(2000);

            Regime = TesterRegime.Play;

            try
            {
                TestingStartEvent?.Invoke();
            }
            catch (Exception ex)
            {
                SendLogMessage(ex.ToString(), LogMessageType.Error);
            }
        }
        catch (Exception ex)
        {
            SendLogMessage(ex.ToString(), LogMessageType.Error);
        }
    }

    public bool TestingFastIsActivate;

    private void CheckWaitOrdersRegime()
    {
        if (_waitSomeActionInPosition == true)
        {
            _waitSomeActionInPosition = false;

            if (TestingFastIsActivate == true)
            {
                TestingFastOnOff();

            }
            Regime = TesterRegime.Pause;
        }
    }

    public void ToDateTimeTestingFast(DateTime timeToGo)
    {
        if (Regime == TesterRegime.NotActive || timeToGo < _timeNow)
        {
            return;
        }

        _timeWeAwaitToStopFastRegime = timeToGo;

        if (TestingFastIsActivate == false)
        {
            TestingFastOnOff();
        }
    }

    private void CheckGoTo()
    {
        if (_timeWeAwaitToStopFastRegime != DateTime.MinValue &&
            _timeWeAwaitToStopFastRegime < _timeNow)
        {
            _timeWeAwaitToStopFastRegime = DateTime.MinValue;

            if (TestingFastIsActivate)
            {
                TestingFastOnOff();
            }

            Regime = TesterRegime.Pause;
        }
    }

    private DateTime _timeWeAwaitToStopFastRegime;

    private bool _waitSomeActionInPosition;

    public event Action TestingStartEvent;

    public event Action TestingFastEvent;

    public event Action TestingEndEvent;

    public event Action TestingNewSecurityEvent;

    #endregion

    #region Main thread work place

    private Thread _worker;

    private void WorkThreadArea()
    {
        Thread.Sleep(2000);

        while (true)
        {
            if (ServerStatus != ServerConnectStatus.Connect
                    && Securities != null && Securities.Count != 0)
            {
                ServerStatus = ServerConnectStatus.Connect;
            }
            else if (ServerStatus == ServerConnectStatus.Connect
                    && (Securities == null || Securities.Count == 0))
            {
                ServerStatus = ServerConnectStatus.Disconnect;
            }

            try
            {
                if (Regime == TesterRegime.Pause ||
                    Regime == TesterRegime.NotActive)
                {
                    Thread.Sleep(500);
                    continue;
                }

                if (!_dataIsReady)
                {

                    SendLogMessage(OsLocalization.Market.Message48, LogMessageType.System);
                    Regime = TesterRegime.NotActive;
                    continue;
                }


                if (Regime == TesterRegime.PlusOne)
                {
                    // NOTE: Ok
                    if (Regime != TesterRegime.Pause)
                    {
                        LoadNextData();
                    }
                    CheckOrders();
                    continue;
                }
                else if (Regime == TesterRegime.Play)
                {

                    LoadNextData();
                    CheckOrders();
                }
            }
            catch (Exception error)
            {
                SendLogMessage(error.ToString(), LogMessageType.Error);
                Thread.Sleep(1000);
            }
        }
    }

    private TimeSpan _timeInterval;

    private bool _dataIsActive;

    public TesterRegime Regime
    {
        get;
        set
        {
            if (field == value) { return; }

            field = value;
            Console.WriteLine(value);

            TestRegimeChangeEvent?.Invoke(field);
            OnPropertyChanged(nameof(Regime));
        }
    } = TesterRegime.NotActive;

    public event Action<TesterRegime> TestRegimeChangeEvent;

    private void LoadNextData()
    {
        if (_timeNow > _timeEnd)
        {
            Regime = TesterRegime.Pause;

            SendLogMessage(OsLocalization.Market.Message37, LogMessageType.System);
            TestingEndEvent?.Invoke();
            return;
        }

        if (_candleSeriesTesterActivate == null ||
            _candleSeriesTesterActivate.Count == 0)
        {
            Regime = TesterRegime.Pause;

            SendLogMessage(OsLocalization.Market.Message38,
                           LogMessageType.System);
            TestingEndEvent?.Invoke();
            return;
        }

        if (_dataIsActive == false)
        {
            _timeNow = _timeNow.AddSeconds(1);
        }
        else
        {
            _timeNow += _timeInterval;
        }

        CheckGoTo();

        //_waitSomeActionInPosition;


        for (int i = 0; _candleSeriesTesterActivate != null && i < _candleSeriesTesterActivate.Count; i++)
        {
            _candleSeriesTesterActivate[i].Load(_timeNow);
        }
    }

    #endregion

    #region Orders 2. Work with placing and cancellation of my orders

    private SecurityTester GetMySecurity(Order order)
    {
        SecurityTester security = null;

        if (TypeTesterData == TesterDataType.Candle)
        {
            for (int i = 0; i < _candleSeriesTesterActivate.Count; i++)
            {
                if (_candleSeriesTesterActivate[i].Security.Name == order.SecurityNameCode
                    && _candleSeriesTesterActivate[i].TimeFrame == order.TimeFrameInTester)
                {
                    security = _candleSeriesTesterActivate[i];
                    break;
                }
            }

            security ??=
                    _candleSeriesTesterActivate.Find(
                        tester =>
                        tester.Security.Name == order.SecurityNameCode
                        &&
                        (tester.LastCandle != null
                         || tester.LastTradeSeries != null
                         || tester.LastMarketDepth != null));
        }
        else
        {
            security =
                _candleSeriesTesterActivate.Find(
                    tester =>
                    tester.Security.Name == order.SecurityNameCode
                    &&
                    (tester.LastCandle != null
                     || tester.LastTradeSeries != null
                     || tester.LastMarketDepth != null));
        }

        return security;
    }


    #endregion

    public List<MyTrade> MyTrades { get; private set; } = [];

    public event Action<MyTrade> NewMyTradeEvent;

    public ObservableCollection<OrderClearing> ClearingTimes;

    public ObservableCollection<NonTradePeriod> NonTradePeriods;

    #region Profits and losses of exchange

    public List<decimal> ProfitArray = [];

    // NOTE: Not really needed as can be done in Journal
    public void AddProfit(decimal profit)
    {
        if (_profitMarketIsOn == false)
        {
            return;
        }
        Portfolios[0].ValueCurrent += profit;
        ProfitArray.Add(Portfolios[0].ValueCurrent);

        NewCurrentValue?.Invoke(Portfolios[0].ValueCurrent);

        PortfoliosChangeEvent?.Invoke(Portfolios);

    }

    // NOTE: Can be done without it
    public event Action<decimal> NewCurrentValue;

    #endregion

    #region Portfolios and positions on the exchange


    public void SetStartDeposit(decimal value)
    {
        Portfolio.ValueCurrent = value;
        Portfolio.ValueBegin = value;
        Portfolio.ValueBlocked = 0;
        Portfolio.ClearPositionOnBoard();
    }

    private void ChangePosition(Order orderExecute)
    {
        List<PositionOnBoard> positions = Portfolios[0].GetPositionOnBoard();

        if (positions == null ||
            orderExecute == null)
        {
            return;
        }

        PositionOnBoard myPositioin =
            positions.Find(board => board.SecurityNameCode == orderExecute.SecurityNameCode);

        myPositioin ??= new PositionOnBoard
        {
            SecurityNameCode = orderExecute.SecurityNameCode,
            PortfolioName = orderExecute.PortfolioNumber,
            ValueBegin = 0
        };

        if (orderExecute.Side == Side.Buy)
        {
            myPositioin.ValueCurrent += orderExecute.Volume;
        }

        if (orderExecute.Side == Side.Sell)
        {
            myPositioin.ValueCurrent -= orderExecute.Volume;
        }

        Portfolios[0].SetNewPosition(myPositioin);

        PortfoliosChangeEvent?.Invoke(Portfolios);
    }

    public Portfolio GetPortfolioForName(string name)
    {
        if (Portfolios == null)
        {
            return null;
        }

        return Portfolios.Find(portfolio => portfolio.Number == name);
    }

    public event Action<List<Portfolio>> PortfoliosChangeEvent;

    #endregion

    #region Securities

    private List<Security> _securities;

    public List<Security> Securities => _securities;

    public Security GetSecurityForName(string name, string secClass)
    {
        if (_securities == null)
        {
            return null;
        }

        return _securities.Find(security => security.Name == name);
    }

    private void _candleManager_CandleUpdateEvent(CandleSeries series)
    {
        if (Regime == TesterRegime.PlusOne)
        {
            Regime = TesterRegime.Pause;
        }

        // write last tick time in server time / перегружаем последним временем тика время сервера
        ServerTime = series.CandlesAll[^1].TimeStart;

        NewCandleIncomeEvent?.Invoke(series);
    }

    public event Action<List<Security>> SecuritiesChangeEvent;

    public void ShowSecuritiesDialog()
    {
        // SecuritiesUi ui = new SecuritiesUi(this);
        // ui.ShowDialog();
    }

    #endregion

    #region Get securities data from file system

    public TesterSourceDataType SourceDataType
    {
        get { return _sourceDataType; }
        set
        {
            if (value == _sourceDataType)
            {
                return;
            }

            _sourceDataType = value;
            ReloadSecurities();
        }
    }
    private TesterSourceDataType _sourceDataType;

    public List<string> Sets { get; private set; }

    private void CheckSet()
    {
        string[] folders = Directory.GetDirectories($"Data{Path.DirectorySeparatorChar}");

        if (folders.Length == 0)
        {
            SendLogMessage(OsLocalization.Market.Message25, LogMessageType.System);
        }

        List<string> sets = [];

        for (int i = 0; i < folders.Length; i++)
        {
            if (folders[i].Split('_').Length == 2)
            {
                sets.Add(folders[i].Split('_')[1]);
                SendLogMessage("Найден сет: " + folders[i].Split('_')[1], LogMessageType.System);
            }
        }

        if (sets.Count == 0)
        {
            SendLogMessage(OsLocalization.Market.Message25, LogMessageType.System);
        }
        Sets = sets;
    }

    public void SetNewSet(string setName)
    {
        string newSet = @"Data" + @"\" + @"Set_" + setName;
        if (newSet == ActiveSet)
        {
            return;
        }

        SendLogMessage(OsLocalization.Market.Message27 + setName, LogMessageType.System);
        ActiveSet = newSet;

        if (_sourceDataType == TesterSourceDataType.Set)
        {
            ReloadSecurities();
        }
        Save();
    }

    public void ReloadSecurities()
    {
        // clear all data and disconnect / чистим все данные, отключаемся
        Regime = TesterRegime.NotActive;
        _dataIsReady = false;
        ServerStatus = ServerConnectStatus.Disconnect;
        _securities = null;
        SecuritiesTester.Clear();
        _candleManager.Clear();
        _candleSeriesTesterActivate = [];
        Save();

        // update / обновляем

        Task.Run(LoadSecurities);

        NeedToReconnectEvent?.Invoke();
    }

    public string PathToFolder => _pathToFolder;
    private string _pathToFolder;

    public void ShowPathSenderDialog()
    {
        if (Regime == TesterRegime.Play)
        {
            Regime = TesterRegime.Pause;
        }

        // System.Windows.Forms.FolderBrowserDialog myDialog = new System.Windows.Forms.FolderBrowserDialog();

        // if (string.IsNullOrWhiteSpace(_pathToFolder))
        // {
        //     myDialog.SelectedPath = _pathToFolder;
        // }
        //
        // myDialog.ShowDialog();
        //
        // if (myDialog.SelectedPath != "" &&
        //     _pathToFolder != myDialog.SelectedPath) // если хоть что-то выбрано
        // {
        //     _pathToFolder = myDialog.SelectedPath;
        //     if (_sourceDataType == TesterSourceDataType.Folder)
        //     {
        //         ReloadSecurities();
        //     }
        // }
    }

    #endregion

    #region Subscribe securities to robots

    private List<SecurityTester> _candleSeriesTesterActivate = [];

    private CandleManager _candleManager;

    private DateTime _lastStartSecurityTime;

    public CandleSeries StartThisSecurity(string securityName, TimeFrameBuilder timeFrameBuilder, string securityClass)
    {
        if (securityName == ""
                || ServerStatus != ServerConnectStatus.Connect
                || _securities == null
                || Portfolios == null)
        {
            return null;
        }

        Security security = null;

        for (int i = 0; i < _securities.Count; i++)
        {
            if (_securities[i].Name == securityName)
            {
                security = _securities[i];
                break;
            }
        }

        if (security == null)
        {
            return null;
        }

        // find security / находим бумагу

        if (TesterDataType.MarketDepth.HasFlag(TypeTesterData))
        {
            timeFrameBuilder.CandleMarketDataType = CandleMarketDataType.MarketDepth;
        }

        if (TesterDataType.Tick.HasFlag(TypeTesterData))
        {
            timeFrameBuilder.CandleMarketDataType = CandleMarketDataType.Tick;
        }

        CandleSeries series = new(timeFrameBuilder, security, StartProgram.IsTester);

        // start security for unloading / запускаем бумагу на выгрузку

        SecurityTester securityTester;
        SecurityTester securityTester2;
        if (TypeTesterData != TesterDataType.Candle)
        {
            SecurityTesterDataType securityTesterDataType;
            if (timeFrameBuilder.CandleMarketDataType == CandleMarketDataType.Tick)
            {
                securityTesterDataType = SecurityTesterDataType.Tick;
            }
            else
            {
                securityTesterDataType = SecurityTesterDataType.MarketDepth;
            }
            securityTester2 = _candleSeriesTesterActivate.Find(tester
                    => tester.Security.Name == securityName
                    && tester.DataType == securityTesterDataType);
            if (securityTester2 == null)
            {
                securityTester = SecuritiesTester.Find(tester
                        => tester.Security.Name == securityName
                        && tester.DataType == securityTesterDataType);
                if (securityTester == null)
                {
                    return null;
                }
                _candleSeriesTesterActivate.Add(securityTester);

            }
        }
        else
        {
            TimeSpan time = GetTimeFrameInSpan(timeFrameBuilder.TimeFrame);
            securityTester2 = _candleSeriesTesterActivate.Find(tester
                    => tester.Security.Name == securityName
                    && tester.DataType == SecurityTesterDataType.Candle
                    && tester.TimeFrameSpan == time);
            if (securityTester2 == null)
            {
                securityTester = SecuritiesTester.Find(tester
                        => tester.Security.Name == securityName
                        && tester.DataType == SecurityTesterDataType.Candle
                        && tester.TimeFrameSpan == time);
                if (securityTester == null)
                {
                    return null;
                }
                _candleSeriesTesterActivate.Add(securityTester);

            }
        }

        if (TypeTesterData != TesterDataType.Candle &&
                timeFrameBuilder.CandleMarketDataType == CandleMarketDataType.Tick)
        {
            if (_candleSeriesTesterActivate.Find(tester => tester.Security.Name == securityName &&
                        tester.DataType == SecurityTesterDataType.Tick) == null)
            {
                if (SecuritiesTester.Find(tester => tester.Security.Name == securityName &&
                            tester.DataType == SecurityTesterDataType.Tick) != null)
                {
                    _candleSeriesTesterActivate.Add(
                            SecuritiesTester.Find(tester => tester.Security.Name == securityName &&
                                tester.DataType == SecurityTesterDataType.Tick));
                }
                else
                { // there is nothing to run the series / нечем запускать серию
                    return null;
                }
            }
        }

        else if (TypeTesterData != TesterDataType.Candle &&
                timeFrameBuilder.CandleMarketDataType == CandleMarketDataType.MarketDepth)
        {
            if (_candleSeriesTesterActivate.Find(tester => tester.Security.Name == securityName &&
                        tester.DataType == SecurityTesterDataType.MarketDepth) == null)
            {
                if (SecuritiesTester.Find(tester => tester.Security.Name == securityName &&
                            tester.DataType == SecurityTesterDataType.MarketDepth) != null)
                {
                    _candleSeriesTesterActivate.Add(
                            SecuritiesTester.Find(tester => tester.Security.Name == securityName &&
                                tester.DataType == SecurityTesterDataType.MarketDepth));
                }
                else
                { // there is nothing to run the series / нечем запускать серию
                    return null;
                }
            }
        }
        else if (TypeTesterData == TesterDataType.Candle)
        {
            TimeSpan time = GetTimeFrameInSpan(timeFrameBuilder.TimeFrame);
            if (_candleSeriesTesterActivate.Find(tester => tester.Security.Name == securityName &&
                        tester.DataType == SecurityTesterDataType.Candle &&
                        tester.TimeFrameSpan == time) == null)
            {
                if (SecuritiesTester.Find(tester => tester.Security.Name == securityName &&
                            tester.DataType == SecurityTesterDataType.Candle &&
                            tester.TimeFrameSpan == time) == null)
                {
                    return null;
                }

                _candleSeriesTesterActivate.Add(
                        SecuritiesTester.Find(tester => tester.Security.Name == securityName &&
                            tester.DataType == SecurityTesterDataType.Candle &&
                            tester.TimeFrameSpan == time));
            }
        }

        _candleManager.StartSeries(series);

        SendLogMessage(OsLocalization.Market.Message14 + series.Security.Name +
                OsLocalization.Market.Message15 + series.TimeFrame +
                OsLocalization.Market.Message16, LogMessageType.System);

        _lastStartSecurityTime = DateTime.Now;

        LoadSecurityEvent?.Invoke();

        return series;
    }

    private TimeSpan GetTimeFrameInSpan(TimeFrame frame)
    {
        return (int)frame > 0 ? (TimeSpan)frame.GetTimeSpan() : new(0, 0, 1, 0);
    }

    private TimeFrame GetTimeFrame(TimeSpan frameSpan)
    {
        TimeFrame tf = (TimeFrame)frameSpan.TotalSeconds;

        return Enum.IsDefined(tf) ? tf : TimeFrame.Min1;
    }

    public void StopThisSecurity(CandleSeries series) => _candleManager.StopSeries(series);

    public event Action<OptionMarketData> NewAdditionalMarketDataEvent;

    public event Action<News> NewsEvent;

    public event Action NeedToReconnectEvent;

    public event Action LoadSecurityEvent;

    #endregion

    #region Synchronizer 

    // NOTE: Remove?
    private bool _dataIsReady;

    public List<SecurityTester> SecuritiesTester = [];

    // FIX:
    public void SynchSecurities(List<BaseStrategy> bots)
    {
        if (bots == null || bots.Count == 0 ||
            SecuritiesTester.Count == 0)
        {
            return;
        }

        List<string> namesSecurity = [];

    //     for (int i = 0; i < bots.Count; i++)
    //     {
    //         List<BotTabSimple> currentTabs = bots[i].TabsSimple;
    //
    //         for (int i2 = 0; currentTabs != null && i2 < currentTabs.Count; i2++)
    //         {
    //             if (currentTabs[i2].Security != null)
    //             {
    //                 namesSecurity.Add(currentTabs[i2].Security.Name);
    //             }
    //         }
    //
    //
    //         List<BotTabPair> currentTabs = bots[i].TabsPair;
    //
    //         for (int i2 = 0; currentTabs != null && i2 < currentTabs.Count; i2++)
    //         {
    //             List<PairToTrade> pairs = currentTabs[i2].Pairs;
    //
    //             for (int i3 = 0; i3 < pairs.Count; i3++)
    //             {
    //                 PairToTrade pair = pairs[i3];
    //
    //                 if (pair.Tab1.Security != null)
    //                 {
    //                     namesSecurity.Add(pair.Tab1.Security.Name);
    //                 }
    //                 if (pair.Tab2.Security != null)
    //                 {
    //                     namesSecurity.Add(pair.Tab2.Security.Name);
    //                 }
    //             }
    //         }
    //
    //
    //         List<BotTabScreener> currentTabs = bots[i].TabsScreener;
    //
    //         for (int i2 = 0; currentTabs != null && i2 < currentTabs.Count; i2++)
    //         {
    //             List<string> secs = new List<string>();
    //
    //             for (int i3 = 0; i3 < currentTabs[i2].SecuritiesNames.Count; i3++)
    //             {
    //                 if (string.IsNullOrEmpty(currentTabs[i2].SecuritiesNames[i3].SecurityName))
    //                 {
    //                     continue;
    //                 }
    //                 secs.Add(currentTabs[i2].SecuritiesNames[i3].SecurityName);
    //             }
    //
    //             if (secs.Count == 0)
    //             {
    //                 continue;
    //             }
    //
    //             namesSecurity.AddRange(secs);
    //         }
    //
    //
    //         List<BotTabCluster> currentTabs = bots[i].TabsCluster;
    //
    //         for (int i2 = 0; currentTabs != null && i2 < currentTabs.Count; i2++)
    //         {
    //             namesSecurity.Add(currentTabs[i2].CandleConnector.SecurityName);
    //         }
    //
    //
    //         List<BotTabIndex> currentTabsSpread = bots[i].TabsIndex;
    //
    //         for (int i2 = 0; currentTabsSpread != null && i2 < currentTabsSpread.Count; i2++)
    //         {
    //             BotTabIndex index = currentTabsSpread[i2];
    //
    //             for (int i3 = 0; index.Tabs != null && i3 < index.Tabs.Count; i3++)
    //             {
    //                 ConnectorCandles currentConnector = index.Tabs[i3];
    //
    //                 if (!string.IsNullOrWhiteSpace(currentConnector.SecurityName))
    //                 {
    //                     namesSecurity.Add(currentConnector.SecurityName);
    //                 }
    //             }
    //
    //         }
    //     }
    //
    //     for (int i = 0; i < bots.Count; i++)
    //     {
    //         List<BotTabPair> currentTabs = bots[i].TabsPair;
    //
    //         for (int i2 = 0; currentTabs != null && i2 < currentTabs.Count; i2++)
    //         {
    //             List<PairToTrade> pairs = currentTabs[i2].Pairs;
    //
    //             for (int i3 = 0; i3 < pairs.Count; i3++)
    //             {
    //                 PairToTrade pair = pairs[i3];
    //
    //                 if (pair.Tab1.Security != null)
    //                 {
    //                     namesSecurity.Add(pair.Tab1.Security.Name);
    //                 }
    //                 if (pair.Tab2.Security != null)
    //                 {
    //                     namesSecurity.Add(pair.Tab2.Security.Name);
    //                 }
    //             }
    //         }
    //     }
    //
    //     for (int i = 0; i < bots.Count; i++)
    //     {
    //         List<BotTabScreener> currentTabs = bots[i].TabsScreener;
    //
    //         for (int i2 = 0; currentTabs != null && i2 < currentTabs.Count; i2++)
    //         {
    //             List<string> secs = [];
    //
    //             for (int i3 = 0; i3 < currentTabs[i2].SecuritiesNames.Count; i3++)
    //             {
    //                 if (string.IsNullOrEmpty(currentTabs[i2].SecuritiesNames[i3].SecurityName))
    //                 {
    //                     continue;
    //                 }
    //                 secs.Add(currentTabs[i2].SecuritiesNames[i3].SecurityName);
    //             }
    //
    //             if (secs.Count == 0)
    //             {
    //                 continue;
    //             }
    //
    //             namesSecurity.AddRange(secs);
    //         }
    //     }
    //
    //     for (int i = 0; i < bots.Count; i++)
    //     {
    //         List<BotTabCluster> currentTabs = bots[i].TabsCluster;
    //
    //         for (int i2 = 0; currentTabs != null && i2 < currentTabs.Count; i2++)
    //         {
    //             namesSecurity.Add(currentTabs[i2].CandleConnector.SecurityName);
    //         }
    //     }
    //
    //     for (int i = 0; i < bots.Count; i++)
    //     {
    //         List<BotTabIndex> currentTabsSpread = bots[i].TabsIndex;
    //
    //         for (int i2 = 0; currentTabsSpread != null && i2 < currentTabsSpread.Count; i2++)
    //         {
    //             BotTabIndex index = currentTabsSpread[i2];
    //
    //             for (int i3 = 0; index.Tabs != null && i3 < index.Tabs.Count; i3++)
    //             {
    //                 ConnectorCandles currentConnector = index.Tabs[i3];
    //
    //                 if (!string.IsNullOrWhiteSpace(currentConnector.SecurityName))
    //                 {
    //                     namesSecurity.Add(currentConnector.SecurityName);
    //                 }
    //             }
    //
    //         }
    //     }

        for (int i = 0; i < SecuritiesTester.Count; i++)
        {
            if (namesSecurity.Find(name => name == SecuritiesTester[i].Security.Name) == null)
            {
                SecuritiesTester[i].IsActive = false;
            }
            else
            {
                SecuritiesTester[i].IsActive = true;
            }
        }

        _candleManager.SynhSeries(namesSecurity);

        if (TesterDataType.Tick.HasFlag(TypeTesterData))
        {
            _timeInterval = new TimeSpan(0, 0, 1);
        }
        else if (TesterDataType.MarketDepth.HasFlag(TypeTesterData))
        {
            _timeInterval = new TimeSpan(0, 0, 0, 0, 1);
        }
        else if (TypeTesterData == TesterDataType.Candle)
        {
            var isLessThanSecond = SecuritiesTester
                .Find(name => name.TimeFrameSpan < new TimeSpan(0, 0, 1)) == null;
            _timeInterval = isLessThanSecond ? new(0, 1, 0) : new(0, 0, 1);
        }
    }

    #endregion

    private void TesterServer_NewCandleEvent(Candle candle, string nameSecurity, TimeSpan timeFrame)
    {
        ServerTime = candle.TimeStart;

        if (_dataIsActive == false)
        {
            _dataIsActive = true;
        }

        NewBidAscIncomeEvent?.Invoke(candle.Close, candle.Close, GetSecurityForName(nameSecurity, ""));

        _candleManager.SetNewCandleInSeries(candle, nameSecurity, timeFrame);

    }

    public event Action<CandleSeries> NewCandleIncomeEvent;

    void TesterServer_NewMarketDepthEvent(MarketDepth marketDepth)
    {
        if (_dataIsActive == false)
        {
            _dataIsActive = true;
        }

        NewMarketDepthEvent?.Invoke(marketDepth);
    }

    public event Action<MarketDepth> NewMarketDepthEvent;

    public event Action<decimal, decimal, Security> NewBidAscIncomeEvent;

    #region All trades table

    private void TesterServer_NewTradesEvent(List<Trade> tradesNew)
    {
        if (_dataIsActive == false)
        {
            _dataIsActive = true;
        }

        if (_allTrades == null)
        {
            _allTrades = new List<Trade>[1];
            _allTrades[0] = new List<Trade>(tradesNew);
        }
        else
        {// sort trades by storages / сортируем сделки по хранилищам

            for (int indTrade = 0; indTrade < tradesNew.Count; indTrade++)
            {
                Trade trade = tradesNew[indTrade];
                bool isSave = false;
                for (int i = 0; i < _allTrades.Length; i++)
                {
                    if (_allTrades[i] != null && _allTrades[i].Count != 0 &&
                        _allTrades[i][0].SecurityNameCode == trade.SecurityNameCode &&
                        _allTrades[i][0].TimeFrameInTester == trade.TimeFrameInTester)
                    { // if there is already a storage for this instrument, save it/ если для этого инструметна уже есть хранилище, сохраняем и всё
                        isSave = true;
                        if (_allTrades[i][0].Time > trade.Time)
                        {
                            break;
                        }
                        _allTrades[i].Add(trade);
                        break;
                    }
                }
                if (isSave == false)
                { // there is no storage for instrument / хранилища для инструмента нет
                    List<Trade>[] allTradesNew = new List<Trade>[_allTrades.Length + 1];
                    for (int i = 0; i < _allTrades.Length; i++)
                    {
                        allTradesNew[i] = _allTrades[i];
                    }
                    allTradesNew[^1] = [trade];
                    _allTrades = allTradesNew;
                }
            }
        }

        if (!TesterDataType.Tick.HasFlag(TypeTesterData))
        {
            for (int i = 0; i < _allTrades.Length; i++)
            {
                List<Trade> curTrades = _allTrades[i];

                if (curTrades != null &&
                    curTrades.Count > 100)
                {
                    curTrades = curTrades.GetRange(curTrades.Count - 101, 100);
                    _allTrades[i] = curTrades;
                }
            }
        }


        ServerTime = tradesNew[^1].Time;

        if (NewTradeEvent != null)
        {
            for (int i = 0; i < _allTrades.Length; i++)
            {
                List<Trade> trades = _allTrades[i];

                if (tradesNew[0].SecurityNameCode == trades[0].SecurityNameCode
                    && tradesNew[0].TimeFrameInTester == trades[0].TimeFrameInTester)
                {
                    if (_removeTradesFromMemory
                        && trades.Count > 1000)
                    {
                        _allTrades[i] = _allTrades[i].GetRange(trades.Count - 1000, 1000);
                        trades = _allTrades[i];
                    }

                    NewTradeEvent(trades);
                    break;
                }
            }
        }
        NewBidAscIncomeEvent?.Invoke(tradesNew[^1].Price, tradesNew[^1].Price, GetSecurityForName(tradesNew[^1].SecurityNameCode, ""));
    }

    private List<Trade>[] _allTrades;

    public List<Trade>[] AllTrades => _allTrades;

    Queue<MyTrade> IServer.MyTrades => throw new NotImplementedException();

    public List<Trade> GetAllTradesToSecurity(Security security)
    {
        for (int i = 0; _allTrades != null && i < _allTrades.Length; i++)
        {
            if (_allTrades[i] != null && _allTrades[i].Count != 0 &&
                _allTrades[i][0].SecurityNameCode == security.Name)
            {
                return _allTrades[i];
            }
        }
        return null;
    }

    public event Action<List<Trade>> NewTradeEvent;

    #endregion

    public void SendLogMessage(string message, LogMessageType type)
    {
        LogMessageEvent?.Invoke(message, type);
    }

    public void OnLogRecieved(string message, LogMessageType type)
    {
        LogRecieved?.Invoke(message, type);
    }

    private Logs _logMaster = new("TesterServer", StartProgram.IsTester);

    public event Action<string, LogMessageType> LogMessageEvent;

    public event Action<string, LogMessageType> LogRecieved;
}

