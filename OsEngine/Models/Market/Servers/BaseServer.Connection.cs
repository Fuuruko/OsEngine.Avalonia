using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Language;
using OsEngine.Models.Candles;
using OsEngine.Models.Entity;
using OsEngine.Models.Entity.Server;
using OsEngine.Models.Logging;

namespace OsEngine.Models.Market.Servers;

public partial class BaseServer
{
    /// <summary>
    /// necessary server status. It needs to thread that listens to connectin
    /// Depending on this field manage the connection 
    /// </summary>
    private ServerConnectStatus _status = ServerConnectStatus.Disconnect;

    // public bool IsConnectionActive { get; protected set; }
    // public bool IsRemoteServerActive { get; protected set; }
    public bool IsConnected { get; protected set; }
    public bool IsExchangeConnected { get; protected set; }

    /// <summary>
    /// server time of last starting
    /// </summary>
    public DateTime LastStartServerTime { get; set; }

    /// <summary>
    /// server status
    /// </summary>
    public ServerConnectStatus ServerStatus
    {
        get { return _serverConnectStatus; }
        private set
        {
            if (value != _serverConnectStatus)
            {
                _serverConnectStatus = value;
                OnLogRecieved(_serverConnectStatus + " " + OsLocalization.Market.Message7, LogMessageType.Connect);
                ConnectStatusChangeEvent?.Invoke(_serverConnectStatus.ToString());
            }
        }
    }

    private ServerConnectStatus _serverConnectStatus;

    /// <summary>
    /// run the server. Connect to trade system
    /// </summary>
    public void StartServer()
    {
        if (_status == ServerConnectStatus.Connect) { return; }

        LastStartServerTime = DateTime.Now.AddSeconds(-300);

        _status = ServerConnectStatus.Connect;
    }

    public void StopServer()
    {
        _status = ServerConnectStatus.Disconnect;
    }

    /// <summary>
    /// start a candle-collecting device
    /// </summary>
    private void StartCandleManager()
    {
        if (_candleManager == null)
        {
            _candleManager = new CandleManager(this, StartProgram.IsOsTrader);
            _candleManager.CandleUpdateEvent += _candleManager_CandleUpdateEvent;
            _candleManager.LogMessageEvent += OnLogRecieved;
        }
    }

    /// <summary>
    /// dispose a candle-collecting device
    /// </summary>
    private void DeleteCandleManager()
    {
        if (_candleManager != null)
        {
            _candleManager.CandleUpdateEvent -= _candleManager_CandleUpdateEvent;
            _candleManager.LogMessageEvent -= OnLogRecieved;
            _candleManager.Dispose();
            _candleManager = null;
        }
    }

    // TODO: Redo
    private WebProxy GetProxy()
    {
        // OsLocalization.Market.Label171 Proxy type
        // OsLocalization.Market.Label172 Proxy

        ServerParameterEnum proxyType = null;
        ServerParameterString proxy = null;

        for (int i = 0; i < ServerParameters.Count; i++)
        {
            if (ServerParameters[i].Name == OsLocalization.Market.Label171)
            {
                proxyType = (ServerParameterEnum)ServerParameters[i];
            }
            if (ServerParameters[i].Name == OsLocalization.Market.Label172)
            {
                proxy = (ServerParameterString)ServerParameters[i];
            }
        }

        if (proxy == null
            || proxyType == null)
        {
            return null;
        }

        if (proxyType.Value == "None")
        {
            return null;
        }

        if (proxyType.Value == "Manual")
        {
            string proxyName = proxy.Value;

            if (string.IsNullOrEmpty(proxyName))
            {
                return null;
            }

            // FIX:
            // return ServerMaster.GetProxyManualRegime(proxyName);
            return null;
        }
        else if (proxyType.Value == "Auto")
        {

            // FIX:
            // return ServerMaster.GetProxyAutoRegime(ServerType, ServerNameAndPrefix);
            return null;
        }

        return null;
    }

    /// <summary>
    /// the place where connection is controlled. look at data streams
    /// </summary>
    private async void PrimeThreadArea()
    {
        while (true)
        {
            // NOTE: await Task.Delay(1000)
            Thread.Sleep(1000);
            // await Task.Delay(1000);
            try
            {
                if (ServerRealization == null)
                {
                    continue;
                }

                if (ServerRealization.ServerStatus != ServerConnectStatus.Connect
                    && _status == ServerConnectStatus.Connect
                    && LastStartServerTime.AddSeconds(100) < DateTime.Now)
                {
                    OnLogRecieved(OsLocalization.Market.Message8, LogMessageType.System);
                    ServerRealization.Dispose();
                    _subscribeSecurities.Clear();

                    Portfolios.Clear();

                    DeleteCandleManager();

                    if (Permissions.SupportsProxyForMultipleServers)
                    {
                        WebProxy proxy = GetProxy();

                        if (proxy != null)
                        {
                            OnLogRecieved(OsLocalization.Market.Label173 + "\n" + proxy.Address, LogMessageType.System);
                        }

                        ServerRealization.Connect(proxy);
                    }
                    else
                    {
                        ServerRealization.Connect(null);
                    }

                    LastStartServerTime = DateTime.Now;

                    NeedToReconnectEvent?.Invoke();

                    continue;
                }

                if (ServerRealization.ServerStatus == ServerConnectStatus.Connect && _status == ServerConnectStatus.Disconnect)
                {
                    OnLogRecieved(OsLocalization.Market.Message9, LogMessageType.System);
                    ServerRealization.Dispose();

                    DeleteCandleManager();

                    continue;
                }

                if (ServerRealization.ServerStatus != ServerConnectStatus.Connect)
                {
                    continue;
                }

                if (_candleManager == null)
                {
                    OnLogRecieved(OsLocalization.Market.Message10, LogMessageType.System);
                    StartCandleManager();
                    continue;
                }

                if (Portfolios.Count == 0)
                {
                    ServerRealization.GetPortfolios();
                }

                if (Securities.Count == 0)
                {
                    ServerRealization.GetSecurities();
                }
            }
            catch (Exception error)
            {
                OnLogRecieved(OsLocalization.Market.Message11, LogMessageType.Error);
                OnLogRecieved(error.ToString(), LogMessageType.Error);
                ServerStatus = ServerConnectStatus.Disconnect;

                try
                {
                    ServerRealization.Dispose();
                }
                catch (Exception ex)
                {
                    OnLogRecieved(ex.ToString(), LogMessageType.Error);
                }

                DeleteCandleManager();

                Thread.Sleep(5000);
                // reconnect / переподключаемся

                // NOTE: No need to start 
                Task.Run(PrimeThreadArea);

                NeedToReconnectEvent?.Invoke();

                return;
            }
        }
    }

    /// <summary>
    /// connection state has changed
    /// </summary>
    public event Action<string> ConnectStatusChangeEvent;

    /// <summary>
    /// need to reconnect server and get a new data
    /// </summary>
    public event Action NeedToReconnectEvent;
}
