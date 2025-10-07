/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Language;
using OsEngine.Models.Candles;
using OsEngine.Models.Entity;
using OsEngine.Models.Entity.Server;
using OsEngine.Models.Logging;
using OsEngine.Models.Market.Servers;
using OsEngine.Models.Market.Servers.Tester;

namespace OsEngine.Models.Market.Connectors;


/// <summary>
/// class that provides a universal interface for connecting to the servers of the exchange for bots
/// terminals and tabs that can trade
/// </summary>
public partial class ConnectorCandles
{
    #region Service code

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="name"> bot name</param>
    /// <param name="startProgram"> program that created the bot which created this connection</param>
    public ConnectorCandles(string name, StartProgram startProgram, bool createEmulator)
    {
        UniqueName = name;
        StartProgram = startProgram;

        TimeFrameBuilder = new TimeFrameBuilder(UniqueName, startProgram);
        ServerType = ServerType.None;


        if (StartProgram != StartProgram.IsOsOptimizer)
        {
            _canSave = true;
            Load();
            // FIX:
            // ServerMaster.RevokeOrderToEmulatorEvent += ServerMaster_RevokeOrderToEmulatorEvent;
        }

        if (createEmulator == true && startProgram != StartProgram.IsOsOptimizer)
        {
            _emulator = new OrderExecutionEmulator();
            _emulator.MyTradeEvent += ConnectorBot_NewMyTradeEvent;
            _emulator.OrderChangeEvent += ConnectorBot_NewOrderIncomeEvent;

        }

        if (!string.IsNullOrWhiteSpace(SecurityName))
        {
            _taskIsDead = false;
            Task.Run(Subscribe);
        }
        else
        {
            _taskIsDead = true;
        }

        if (StartProgram == StartProgram.IsTester)
        {
            PortfolioName = "GodMode";
        }
    }

    /// <summary>
    /// program that created the bot which created this connection
    /// </summary>
    public StartProgram StartProgram;

    /// <summary>
    /// shows whether it is possible to save settings
    /// </summary>
    private bool _canSave;


    /// <summary>
    /// show settings window
    /// </summary>
    public void ShowDialog(bool canChangeSettingsSaveCandlesIn)
    {
        try
        {
            // if (ServerMaster.GetServers() == null ||
            //     ServerMaster.GetServers().Count == 0)
            // {
            //     SendNewLogMessage(OsLocalization.Market.Message1, LogMessageType.Error);
            //     return;
            // }
            //
            // if (_ui == null)
            // {
            //     _ui = new ConnectorCandlesUi(this);
            //     _ui.IsCanChangeSaveTradesInCandles(canChangeSettingsSaveCandlesIn);
            //     _ui.LogMessageEvent += SendNewLogMessage;
            //     _ui.Closed += _ui_Closed;
            //     _ui.Show();
            // }
            // else
            // {
            //     _ui.Activate();
            // }
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    private void _ui_Closed(object sender, EventArgs e)
    {
        try
        {
            // _ui.LogMessageEvent -= SendNewLogMessage;
            // _ui.Closed -= _ui_Closed;
            // _ui = null;
            //
            // if (DialogClosed != null)
            // {
            //     DialogClosed();
            // }
        }
        catch
        {
            // ignore
        }
    }

    public event Action DialogClosed;

    // private ConnectorCandlesUi _ui;

    #endregion

    #region Settings and properties

    /// <summary>
    /// name of bot that owns the connector
    /// </summary>
    public string UniqueName { get; }

    /// <summary>
    /// trade server
    /// </summary>
    public IServer MyServer { get; private set; }

    /// <summary>
    /// connector's server type 
    /// </summary>
    public ServerType ServerType;

    /// <summary>
    /// connector`s server full name
    /// </summary>
    public string ServerFullName;

    /// <summary>
    /// unique server number. Service data for the optimizer
    /// </summary>
    public int ServerUid;

    /// <summary>
    /// whether the object is connected to the server
    /// </summary>
    public bool IsConnected => CandleSeries != null;

    /// <summary>
    /// connector is ready to send Orders it true
    /// </summary>
    public bool IsReadyToTrade
    {
        get
        {
            if (MyServer == null)
            {
                return false;
            }

            if (MyServer.ServerStatus != ServerConnectStatus.Connect)
            {
                return false;
            }

            if (StartProgram != StartProgram.IsOsTrader)
            { // в тестере и оптимизаторе дальше не проверяем
                return true;
            }

            if (MyServer.LastStartServerTime.AddSeconds(5) > DateTime.Now)
            {
                return false;
            }

            if (MyServer is BaseServer server)
            {
                if (server.LastStartServerTime.AddSeconds(server.WaitTimeToTradeAfterFirstStart) > DateTime.Now)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// connector's portfolio number
    /// </summary>
    public string PortfolioName;

    /// <summary>
    /// connector's portfolio object
    /// </summary>
    public Portfolio Portfolio
    {
        get
        {
            try
            {
                return MyServer?.GetPortfolioForName(PortfolioName);
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }

            return null;
        }
    }

    /// <summary>
    /// connector's security name
    /// </summary>
    public string SecurityName
    {
        get => _securityName;
        set
        {
            if (value != _securityName)
            {
                _securityName = value;
                Save();
                Reconnect();
            }
        }
    }
    private string _securityName;

    /// <summary>
    /// connector's security class
    /// </summary>
    public string SecurityClass
    {
        get => _securityClass;
        set
        {
            if (value != _securityClass)
            {
                _securityClass = value;
                Save();
                Reconnect();
            }
        }
    }
    private string _securityClass;

    /// <summary>
    /// connector's security object
    /// </summary>
    public Security Security
    {
        get
        {
            try
            {
                if (MyServer != null)
                {
                    return MyServer.GetSecurityForName(_securityName, _securityClass);
                }
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }

            return null;
        }
    }

    /// <summary>
    /// does the server supports Market type orders. Support = true
    /// </summary>
    public bool MarketOrdersIsSupport
    {
        get
        {
            if (ServerType == ServerType.Lmax ||
                ServerType == ServerType.Tester ||
                 ServerType == ServerType.Optimizer ||
                ServerType == ServerType.BitMex)
            {
                return true;
            }

            if (ServerType == ServerType.None)
            {
                return false;
            }

            // FIX:
            // IServerPermission serverPermision = ServerMaster.GetServerPermission(ServerType);
            //
            // if (serverPermision == null)
            // {
            //     return false;
            // }
            //
            // return serverPermision.MarketOrdersIsSupport;
            return false;
        }
    }

    /// <summary>
    /// does the server support order price change. Support = true
    /// </summary>
    public bool IsCanChangeOrderPrice
    {
        get
        {
            if (ServerType == ServerType.None)
            {
                return false;
            }

            ServerPermissions serverPermision = MyServer.Permissions;

            if (serverPermision == null)
            {
                return false;
            }

            return serverPermision.CanChangeOrderPrice;
        }
    }

    /// <summary>
    /// shows whether execution of orders in emulation mode is enabled
    /// </summary>
    public bool EmulatorIsOn;

    /// <summary>
    /// emulator. Object for order execution in the emulation mode 
    /// </summary>
    private readonly OrderExecutionEmulator _emulator;

    /// <summary>
    /// whether event feeding is enabled in the robot
    /// </summary>
    // NOTE: Subscribe/Unsubscribe from events when turn on/off
    public bool EventsIsOn
    {
        get => _eventsIsOn;
        set
        {
            if (_eventsIsOn == value) { return; }

            _eventsIsOn = value;
            Save();
        }
    }
    private bool _eventsIsOn = true;

    /// <summary>
    /// commission type for positions
    /// </summary>
    public CommissionType CommissionType;

    /// <summary>
    /// commission rate
    /// </summary>
    public decimal CommissionValue;

    #endregion

    #region Candle series settings

    /// <summary>
    /// candle series that collects candles  
    /// </summary>
    public CandleSeries CandleSeries { get; private set; }

    /// <summary>
    /// object preserving settings for building candles
    /// </summary>
    public TimeFrameBuilder TimeFrameBuilder;

    /// <summary>
    /// method of creating candles: from ticks or from depths 
    /// </summary>
    public CandleMarketDataType CandleMarketDataType
    {
        set
        {
            if (value == TimeFrameBuilder.CandleMarketDataType)
            {
                return;
            }
            TimeFrameBuilder.CandleMarketDataType = value;

            if (value == CandleMarketDataType.MarketDepth)
            {
                NeedToLoadServerData = true;
            }

            Reconnect();
        }
        get { return TimeFrameBuilder.CandleMarketDataType; }
    }

    /// <summary>
    /// method of creating candles: Simple / Volume / Range / etc
    /// </summary>
    public string CandleCreateMethodType
    {
        set
        {
            if (value == TimeFrameBuilder.CandleCreateMethodType)
            {
                return;
            }
            TimeFrameBuilder.CandleCreateMethodType = value;
            Reconnect();
        }
        get { return TimeFrameBuilder.CandleCreateMethodType; }
    }

    /// <summary>
    /// candles timeframe on which the connector is subscribed
    /// </summary>
    public TimeFrame TimeFrame
    {
        get { return TimeFrameBuilder.TimeFrame; }
        set
        {
            try
            {
                if (value != TimeFrameBuilder.TimeFrame
                    || (value == TimeFrame.Sec1 &&
                    TimeFrameBuilder.TimeFrameTimeSpan.TotalSeconds == 0))
                {
                    TimeFrameBuilder.TimeFrame = value;
                    Reconnect();
                }
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
        }
    }

    /// <summary>
    /// candle timeframe in the form of connector' s Timespan
    /// </summary>
    public TimeSpan TimeFrameTimeSpan
    {
        get { return TimeFrameBuilder.TimeFrameTimeSpan; }
    }

    /// <summary>
    /// whether the trades tape is saved inside the candles
    /// </summary>
    public bool SaveTradesInCandles
    {
        get { return TimeFrameBuilder.SaveTradesInCandles; }
        set
        {
            if (value == TimeFrameBuilder.SaveTradesInCandles)
            {
                return;
            }
            TimeFrameBuilder.SaveTradesInCandles = value;
            Reconnect();
        }
    }

    #endregion

    #region Data subscription

    private DateTime _lastReconnectTime;

    private Lock _reconnectLocker = new();

    private void Reconnect()
    {
        try
        {
            lock (_reconnectLocker)
            {
                if (_lastReconnectTime.AddSeconds(1) > DateTime.Now)
                {
                    ConnectorStartedReconnectEvent?.Invoke(SecurityName, TimeFrame, TimeFrameTimeSpan, PortfolioName, ServerFullName);
                    return;
                }
                _lastReconnectTime = DateTime.Now;
            }


            if (CandleSeries != null)
            {
                CandleSeries.Stop();
                CandleSeries.Clear();
                CandleSeries.CandleUpdateEvent -= MySeries_CandleUpdateEvent;
                CandleSeries.CandleFinishedEvent -= MySeries_CandleFinishedEvent;

                MyServer?.StopThisSecurity(CandleSeries);
                CandleSeries = null;
            }

            Save();

            ConnectorStartedReconnectEvent?.Invoke(SecurityName, TimeFrame, TimeFrameTimeSpan, PortfolioName, ServerFullName);

            if (_taskIsDead == true)
            {
                _taskIsDead = false;
                Task.Run(Subscribe);

                NewCandlesChangeEvent?.Invoke(null);
            }
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    private bool _lastHardReconnectOver = true;

    public void ReconnectHard()
    {
        if (_lastHardReconnectOver == false)
        {
            return;
        }

        _lastHardReconnectOver = false;

        DateTime timestart = DateTime.Now;

        if (CandleSeries != null)
        {
            CandleSeries.Stop();
            CandleSeries.Clear();
            CandleSeries.CandleUpdateEvent -= MySeries_CandleUpdateEvent;
            CandleSeries.CandleFinishedEvent -= MySeries_CandleFinishedEvent;

            MyServer?.StopThisSecurity(CandleSeries);
            CandleSeries = null;
        }

        Reconnect();

        _lastHardReconnectOver = true;
    }

    private bool _taskIsDead;

    private bool _needToStopThread;

    private Lock _myServerLocker = new();

    private static int _aliveTasks = 0;

    private static Lock _aliveTasksArrayLocker = new();

    private bool _alreadyCheckedInAliveTasksArray = false;

    private static int _tasksCountOnSubscribe = 0;

    private static Lock _tasksCountLocker = new();

    private async void Subscribe()
    {
        try
        {
            _alreadyCheckedInAliveTasksArray = false;

            while (true)
            {
                if (ServerType == ServerType.Optimizer)
                {
                    await Task.Delay(1);
                }
                else if (ServerType == ServerType.Tester)
                {
                    await Task.Delay(10);
                }
                else
                {
                    int millisecondsToDelay = _aliveTasks * 5;

                    lock (_aliveTasksArrayLocker)
                    {
                        if (_alreadyCheckedInAliveTasksArray == false)
                        {
                            _aliveTasks++;
                            _alreadyCheckedInAliveTasksArray = true;
                        }

                        if (millisecondsToDelay < 500)
                        {
                            millisecondsToDelay = 500;
                        }
                    }

                    await Task.Delay(millisecondsToDelay);
                }

                if (_needToStopThread)
                {
                    lock (_aliveTasksArrayLocker)
                    {
                        if (_aliveTasks > 0)
                        {
                            _aliveTasks--;
                        }
                    }
                    return;
                }

                if (ServerType == ServerType.None ||
                    string.IsNullOrWhiteSpace(SecurityName))
                {
                    continue;
                }

                // FIX: Doesnt contain Tester and Optimizer servers
                List<BaseServer> servers = [.. BaseServer.Servers.Values];

                if (servers == null)
                {
                    if (ServerType != ServerType.None)
                    {
                        // FIX: When AutoConnection will be done
                        // ServerMaster.SetServerToAutoConnection(ServerType, ServerFullName);
                    }
                    continue;
                }

                try
                {
                    if (ServerType == ServerType.Optimizer &&
                        ServerUid != 0)
                    {
                        // FIX: When OptimizerServer created
                        // for (int i = 0; i < servers.Count; i++)
                        // {
                        //     if (servers[i] == null)
                        //     {
                        //         servers.RemoveAt(i);
                        //         i--;
                        //         continue;
                        //     }
                        //     if (servers[i] is OptimizerServer optimizerServer1
                        //             && optimizerServer1.NumberServer == ServerUid)
                        //     {
                        //         MyServer = servers[i];
                        //         break;
                        //     }
                        //
                        // }
                    }
                    else
                    {
                        for (int i = 0; i < servers.Count; i++)
                        {
                            if (servers[i].ServerType == ServerType
                                && servers[i].ServerNameAndPrefix.StartsWith(ServerFullName))
                            {
                                MyServer = servers[i];
                                break;
                            }
                            else if (string.IsNullOrEmpty(ServerFullName) &&
                                servers[i].ServerType == ServerType)
                            {
                                MyServer = servers[i];
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    // ignore
                    continue;
                }

                if (MyServer == null)
                {
                    if (ServerType != ServerType.None)
                    {
                        // ServerMaster.SetServerToAutoConnection(ServerType, ServerFullName);
                    }
                    continue;
                }

                ServerConnectStatus stat = MyServer.ServerStatus;

                if (stat != ServerConnectStatus.Connect)
                {
                    continue;
                }

                SubscribeOnServer(MyServer);

                if (MyServer is TesterServer tester)
                {
                    // NOTE: Why unsub and sub?
                    tester.TestingEndEvent -= Connector_TestingEndEvent;
                    tester.TestingEndEvent += Connector_TestingEndEvent;

                    tester.TestingStartEvent -= Connector_TestingStartEvent;
                    tester.TestingStartEvent += Connector_TestingStartEvent;
                }

                if (CandleSeries == null)
                {
                    while (CandleSeries == null)
                    {
                        if (_needToStopThread)
                        {
                            lock (_aliveTasksArrayLocker)
                            {
                                if (_aliveTasks > 0)
                                {
                                    _aliveTasks--;
                                }
                            }
                            return;
                        }
                        if (MyServer == null)
                        {
                            continue;
                        }

                        if (StartProgram == StartProgram.IsOsTrader ||
                            StartProgram == StartProgram.IsOsData)
                        {
                            int millisecondsToDelay = _aliveTasks * 5;

                            if (millisecondsToDelay < 500)
                            {
                                millisecondsToDelay = 500;
                            }

                            if (millisecondsToDelay > 1000)
                            {
                                millisecondsToDelay = 1000;
                            }

                            await Task.Delay(millisecondsToDelay);
                        }
                        else
                        {
                            await Task.Delay(1);
                        }

                        if (_tasksCountOnSubscribe > 20)
                        {
                            continue;
                        }

                        lock (_tasksCountLocker)
                        {
                            _tasksCountOnSubscribe++;
                        }

                        lock (_myServerLocker)
                        {
                            if (MyServer != null)
                            {
                                CandleSeries = MyServer.StartThisSecurity(_securityName, TimeFrameBuilder, _securityClass);
                            }
                        }

                        lock (_tasksCountLocker)
                        {
                            _tasksCountOnSubscribe--;
                        }

                        // FIX: When Optimizer server created
                        // if (CandleSeries == null &&
                        //     MyServer is OptimizerServer optimizerServer &&
                        //     optimizerServer.ServerType == ServerType.Optimizer &&
                        //     optimizerServer.NumberServer != ServerUid)
                        // {
                        //     for (int i = 0; i < servers.Count; i++)
                        //     {
                        //         if (servers[i] is OptimizerServer optimizerServer1
                        //                 && optimizerServer1.NumberServer == ServerUid)
                        //         {
                        //             UnSubscribeOnServer(MyServer);
                        //             MyServer = servers[i];
                        //             SubscribeOnServer(MyServer);
                        //             break;
                        //         }
                        //     }
                        // }
                    }

                    CandleSeries.CandleUpdateEvent += MySeries_CandleUpdateEvent;
                    CandleSeries.CandleFinishedEvent += MySeries_CandleFinishedEvent;
                    _taskIsDead = true;
                }


                _taskIsDead = true;

                SecuritySubscribeEvent?.Invoke(Security);

                lock (_aliveTasksArrayLocker)
                {
                    if (_aliveTasks > 0)
                    {
                        _aliveTasks--;
                    }
                }

                return;
            }
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    private void UnSubscribeOnServer(IServer server)
    {
        server.NewBidAscIncomeEvent -= ConnectorBotNewBidAscIncomeEvent;
        server.NewMyTradeEvent -= ConnectorBot_NewMyTradeEvent;
        server.CancelOrderFailEvent -= _myServer_CancelOrderFailEvent;
        server.NewOrderIncomeEvent -= ConnectorBot_NewOrderIncomeEvent;
        server.NewMarketDepthEvent -= ConnectorBot_NewMarketDepthEvent;
        server.NewTradeEvent -= ConnectorBot_NewTradeEvent;
        server.TimeServerChangeEvent -= myServer_TimeServerChangeEvent;
        server.NeedToReconnectEvent -= _myServer_NeedToReconnectEvent;
        server.PortfoliosChangeEvent -= Server_PortfoliosChangeEvent;
        server.NewAdditionalMarketDataEvent -= Server_NewAdditionalMarketDataEvent;
        server.NewFundingEvent -= Server_NewFundingEvent;
        server.NewVolume24hUpdateEvent -= Server_NewVolume24hUpdateEvent;
    }

    private void SubscribeOnServer(IServer server)
    {
        server.NewBidAscIncomeEvent -= ConnectorBotNewBidAscIncomeEvent;
        server.NewMyTradeEvent -= ConnectorBot_NewMyTradeEvent;
        server.NewOrderIncomeEvent -= ConnectorBot_NewOrderIncomeEvent;
        server.CancelOrderFailEvent -= _myServer_CancelOrderFailEvent;
        server.NewMarketDepthEvent -= ConnectorBot_NewMarketDepthEvent;
        server.NewTradeEvent -= ConnectorBot_NewTradeEvent;
        server.TimeServerChangeEvent -= myServer_TimeServerChangeEvent;
        server.NeedToReconnectEvent -= _myServer_NeedToReconnectEvent;
        server.PortfoliosChangeEvent -= Server_PortfoliosChangeEvent;
        server.NewAdditionalMarketDataEvent -= Server_NewAdditionalMarketDataEvent;
        server.NewFundingEvent -= Server_NewFundingEvent;
        server.NewVolume24hUpdateEvent -= Server_NewVolume24hUpdateEvent;

        if (NeedToLoadServerData)
        {
            server.NewMarketDepthEvent += ConnectorBot_NewMarketDepthEvent;
            server.NewBidAscIncomeEvent += ConnectorBotNewBidAscIncomeEvent;
            server.NewTradeEvent += ConnectorBot_NewTradeEvent;
            server.TimeServerChangeEvent += myServer_TimeServerChangeEvent;
            server.NewMyTradeEvent += ConnectorBot_NewMyTradeEvent;
            server.NewOrderIncomeEvent += ConnectorBot_NewOrderIncomeEvent;
            server.CancelOrderFailEvent += _myServer_CancelOrderFailEvent;
            server.PortfoliosChangeEvent += Server_PortfoliosChangeEvent;
            server.NewAdditionalMarketDataEvent += Server_NewAdditionalMarketDataEvent;
            server.NewFundingEvent += Server_NewFundingEvent;
            server.NewVolume24hUpdateEvent += Server_NewVolume24hUpdateEvent;
        }

        server.NeedToReconnectEvent += _myServer_NeedToReconnectEvent;
    }

    public bool NeedToLoadServerData = true;

    private void _myServer_NeedToReconnectEvent()
    {
        Reconnect();
    }

    #endregion

    #region Trade data access interface

    /// <summary>
    /// trades feed
    /// </summary>
    public List<Trade> Trades
    {
        get
        {
            try
            {
                if (MyServer != null)
                {
                    return MyServer.GetAllTradesToSecurity(MyServer.GetSecurityForName(_securityName, _securityClass));
                }
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }

            return null;
        }
    }

    /// <summary>
    /// connector's candles
    /// </summary>
    public List<Candle> Candles(bool onlyReady)
    {
        try
        {
            if (CandleSeries == null ||
                CandleSeries.CandlesAll == null)
            {
                return null;
            }
            if (onlyReady)
            {
                return CandleSeries.CandlesOnlyReady;
            }
            else
            {
                return CandleSeries.CandlesAll;
            }

        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }


        return null;
    }

    /// <summary>
    /// best price of seller in the depth
    /// </summary>
    public decimal BestAsk { get; private set; }

    /// <summary>
    /// best price of buyer in the depth
    /// </summary>
    public decimal BestBid { get; private set; }

    /// <summary>
    /// server time
    /// </summary>
    public DateTime MarketTime
    {
        get
        {
            try
            {
                if (MyServer == null)
                {
                    return DateTime.Now;
                }
                return MyServer.ServerTime;
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
            return DateTime.Now;
        }
    }

    /// <summary>
    /// Data of Options
    /// </summary>
    public OptionMarketData OptionMarketData { get; private set; } = new();

    /// <summary>
    /// Data of Funding
    /// </summary>
    public Funding Funding { get; private set; } = new();

    /// <summary>
    /// Volume24h
    /// </summary>
    public SecurityVolumes SecurityVolumes { get; private set; } = new();

    #endregion

    #region Orders

    /// <summary>
    /// execute order
    /// </summary>
    public void OrderExecute(Order order, bool isEmulator = false)
    {
        try
        {
            if (MyServer == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(order.ServerName))
            {
                order.ServerName = ServerFullName;
            }

            if (MyServer.ServerStatus == ServerConnectStatus.Disconnect)
            {
                SendNewLogMessage(OsLocalization.Market.Message2, LogMessageType.Error);
                return;
            }

            if (StartProgram == StartProgram.IsTester ||
                StartProgram == StartProgram.IsOsOptimizer)
            {
                order.TimeFrameInTester = TimeFrameBuilder.TimeFrame;
            }

            order.SecurityNameCode = SecurityName;
            order.SecurityClassCode = SecurityClass;
            order.PortfolioNumber = PortfolioName;
            order.ServerType = ServerType;
            order.TimeCreate = MarketTime;

            if (StartProgram != StartProgram.IsTester &&
                StartProgram != StartProgram.IsOsOptimizer &&
                (EmulatorIsOn
                || MyServer.ServerType == ServerType.Finam
                || isEmulator))
            {
                _emulator?.OrderExecute(order);
            }
            else
            {
                MyServer.ExecuteOrder(order);
            }
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// cancel order
    /// </summary>
    public void OrderCancel(Order order)
    {
        try
        {
            if (MyServer == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(order.SecurityNameCode))
            {
                order.SecurityNameCode = SecurityName;
            }

            if (string.IsNullOrEmpty(order.PortfolioNumber))
            {
                order.PortfolioNumber = PortfolioName;
            }

            if (MyServer.ServerStatus == ServerConnectStatus.Disconnect)
            {
                SendNewLogMessage(OsLocalization.Market.Message99, LogMessageType.Error);
                return;
            }

            if (EmulatorIsOn
                || MyServer.ServerType == ServerType.Finam
                || order.SecurityNameCode == SecurityName + " TestPaper")
            {
                _emulator?.OrderCancel(order);
            }
            else
            {
                MyServer.CancelOrder(order);
            }
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// Order price change
    /// </summary>
    /// <param name="order">An order that will have a new price</param>
    /// <param name="newPrice">New price</param>
    public void ChangeOrderPrice(Order order, decimal newPrice)
    {
        if (order == null)
        {
            return;
        }

        try
        {
            if (MyServer == null)
            {
                return;
            }

            if (MyServer.ServerStatus == ServerConnectStatus.Disconnect)
            {
                SendNewLogMessage(OsLocalization.Market.Message2, LogMessageType.Error);
                return;
            }

            if (order.Volume == order.VolumeExecute
                || order.State == OrderStateType.Done
                || order.State == OrderStateType.Fail)
            {
                return;
            }

            if (EmulatorIsOn)
            {
                if (_emulator.ChangeOrderPrice(order, newPrice))
                {
                    OrderChangeEvent?.Invoke(order);
                }
            }
            else
            {
                if (IsCanChangeOrderPrice == false)
                {
                    SendNewLogMessage(OsLocalization.Trader.Label373, LogMessageType.Error);
                    return;
                }

                MyServer.ChangeOrderPrice(order, newPrice);
            }
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// incoming event: need to cancel the order
    /// </summary>
    private void ServerMaster_RevokeOrderToEmulatorEvent(Order order)
    {
        if (order.SecurityNameCode != SecurityName + " TestPaper"
            && order.SecurityNameCode != SecurityName)
        {
            return;
        }

        if (IsConnected == false
           || IsReadyToTrade == false)
        {
            SendNewLogMessage(OsLocalization.Trader.Label191, LogMessageType.Error);
            return;
        }

        OrderCancel(order);
    }

    public void CheckEmulatorExecution(decimal price)
    {
        if (EmulatorIsOn == false)
        {
            return;
        }

        _emulator.ProcessBidAsc(price, price);
    }

    #endregion

    #region Events


    #endregion

    #region Log

    /// <summary>
    /// send new message to up
    /// </summary>
    public void SendNewLogMessage(string message, LogMessageType type)
    {
        if (LogMessageEvent != null)
        {
            LogMessageEvent(message, type);
        }
        else if (type == LogMessageType.Error)
        { // if nobody is subscribed to us and there is an error in the log / если на нас никто не подписан и в логе ошибка
            MessageBox.Show(message);
        }
    }

    /// <summary>
    /// outgoing log message
    /// </summary>
    public event Action<string, LogMessageType> LogMessageEvent;

    #endregion
}

/// <summary>
/// connector work type
/// </summary>
public enum ConnectorWorkType
{
    /// <summary>
    /// real connection
    /// </summary>
    Real,

    /// <summary>
    /// test trading
    /// </summary>
    Tester
}
