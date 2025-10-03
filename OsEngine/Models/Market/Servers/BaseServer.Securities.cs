using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using OsEngine.Language;
using OsEngine.Models.Candles;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;

namespace OsEngine.Models.Market.Servers;

public partial class BaseServer
{

    private List<SecurityFlowTime> _subscribeSecurities = [];

    private ConcurrentQueue<SecurityFlowTime> _securitiesFeedFlow = new();

    /// <summary>
    /// all instruments in the system
    /// </summary>
    public List<Security> Securities { get; private set; } = [];

    /// <summary>
    /// often used securities. optimizes access to securities
    /// </summary>
    private List<Security> _frequentlyUsedSecurities = [];

    private async void CheckDataFlowThread()
    {
        while (true)
        {
            await Task.Delay(3000);

            if (MainWindow.ProccesIsWorked == false) { return; }

            if (ServerStatus != ServerConnectStatus.Connect) { continue; }


            try
            {
                // 1 разбираем очередь с обновлением данных с сервера
                while (_securitiesFeedFlow.TryDequeue(out SecurityFlowTime securityFlowTime))
                {
                    if (securityFlowTime.LastTimeMarketDepth != DateTime.MinValue)
                    {// пришло обновление стакана

                        for (int i = 0; i < _subscribeSecurities.Count; i++)
                        {
                            if (_subscribeSecurities[i].SecurityName == securityFlowTime.SecurityName
                                && securityFlowTime.LastTimeMarketDepth > _subscribeSecurities[i].LastTimeMarketDepth)
                            {
                                _subscribeSecurities[i].LastTimeMarketDepth = securityFlowTime.LastTimeMarketDepth;
                                break;
                            }
                        }
                    }
                    else if (securityFlowTime.LastTimeTrade != DateTime.MinValue)
                    {// пришло обновление в ленте сделок

                        for (int i = 0; i < _subscribeSecurities.Count; i++)
                        {
                            if (_subscribeSecurities[i].SecurityName == securityFlowTime.SecurityName
                                && securityFlowTime.LastTimeTrade > _subscribeSecurities[i].LastTimeTrade)
                            {
                                _subscribeSecurities[i].LastTimeTrade = securityFlowTime.LastTimeTrade;
                                break;
                            }
                        }
                    }

                }

                // 2 смотрим, есть ли отставание по какой-то бумаге

                SecurityFlowTime maxDataDelayMarketDepth = null;
                SecurityFlowTime maxDataDelayTrade = null;

                for (int i = 0; i < _subscribeSecurities.Count; i++)
                {
                    if (_subscribeSecurities[i].LastTimeTrade != DateTime.MinValue)
                    {
                        if (maxDataDelayTrade == null ||
                            maxDataDelayTrade.LastTimeTrade > _subscribeSecurities[i].LastTimeTrade)
                        {
                            maxDataDelayTrade = _subscribeSecurities[i];
                        }
                    }

                    if (_subscribeSecurities[i].LastTimeMarketDepth != DateTime.MinValue)
                    {
                        if (maxDataDelayMarketDepth == null
                            || maxDataDelayMarketDepth.LastTimeMarketDepth > _subscribeSecurities[i].LastTimeMarketDepth)
                        {
                            maxDataDelayMarketDepth = _subscribeSecurities[i];
                        }
                    }
                }

                // 3 смотрим, не пора ли перезапускать коннектор

                bool needToReconnect = false;

                if (maxDataDelayMarketDepth != null
                    && maxDataDelayMarketDepth.LastTimeMarketDepth.AddMinutes(Permissions.CheckDataFeedLogic_NoDataMinutesToDisconnect)
                    < DateTime.Now)
                { // перезагружаем т.к. нет стаканов уже N минут
                    string messageToLog = "ERROR data feed. No MarketDepth. CheckDataFlowThread in Aserver. \n";
                    messageToLog += "Connector: " + ServerType + "\n";
                    messageToLog += "Security: " + maxDataDelayMarketDepth.SecurityName + "\n";
                    messageToLog += "No data time: " + (DateTime.Now - maxDataDelayMarketDepth.LastTimeMarketDepth).ToString() + "\n";
                    messageToLog += "Reconnect activated";
                    OnLogRecieved(messageToLog, LogMessageType.Error);
                    needToReconnect = true;
                }
                if (maxDataDelayTrade != null
                    && maxDataDelayTrade.LastTimeTrade.AddMinutes(Permissions.CheckDataFeedLogic_NoDataMinutesToDisconnect * 3)
                    < DateTime.Now)
                { // перезагружаем т.к. нет трейдов уже N минут

                    string messageToLog = "ERROR data feed. No Trades. CheckDataFlowThread in Aserver. \n";
                    messageToLog += "Connector: " + ServerType + "\n";
                    messageToLog += "Security: " + maxDataDelayTrade.SecurityName + "\n";
                    messageToLog += "No data time: " + (DateTime.Now - maxDataDelayTrade.LastTimeTrade).ToString() + "\n";
                    messageToLog += "Reconnect activated";
                    OnLogRecieved(messageToLog, LogMessageType.Error);
                    needToReconnect = true;
                }

                if (needToReconnect)
                {
                    ServerRealization_Disconnected();
                }
            }
            catch (Exception ex)
            {
                OnLogRecieved(ex.ToString(), LogMessageType.Error);
                await Task.Delay(15000);
            }
        }
    }

    /// <summary>
    /// take the instrument as a Security by name of instrument
    /// </summary>
    public Security GetSecurityForName(string securityName, string securityClass)
    {
        if (Securities.Count > 0) { return null; }

        if (string.IsNullOrEmpty(securityClass) == false)
        {
            for (int i = 0; i < _frequentlyUsedSecurities.Count; i++)
            {
                if (_frequentlyUsedSecurities[i].Name == securityName &&
                    _frequentlyUsedSecurities[i].NameClass == securityClass)
                {
                    return _frequentlyUsedSecurities[i];
                }
            }

            for (int i = 0; i < Securities.Count; i++)
            {
                if (Securities[i].Name == securityName &&
                    Securities[i].NameClass == securityClass)
                {
                    _frequentlyUsedSecurities.Add(Securities[i]);
                    return Securities[i];
                }
            }
        }

        return Securities.Find(s => s.Name == securityName);
    }

    /// <summary>
    /// show security
    /// </summary>
    // FIX:
    // public void ShowSecuritiesDialog()
    // {
    //     SecuritiesUi ui = new SecuritiesUi(this);
    //     ui.ShowDialog();
    // }

    /// <summary>
    /// instruments changed
    /// </summary>
    public event Action<List<Security>> SecuritiesChangeEvent;


    // FIX:
    // private SecuritiesUi _securitiesUi;
    //
    // private void AServer_UserClickButton()
    // {
    //     if (_securitiesUi == null)
    //     {
    //         _securitiesUi = new SecuritiesUi(this);
    //         _securitiesUi.Show();
    //         _securitiesUi.Closed += _securitiesUi_Closed;
    //     }
    //     else
    //     {
    //         _securitiesUi.Activate();
    //     }
    // }
    //
    // private void _securitiesUi_Closed(object sender, EventArgs e)
    // {
    //     _securitiesUi.Closed -= _securitiesUi_Closed;
    //     _securitiesUi = null;
    // }

    private List<Security> _savedSecurities;

    private void TryUpdateSecuritiesUserSettings(List<Security> securities)
    {
        try
        {
            _savedSecurities ??= LoadSavedSecurities();

            for (int i = 0; i < _savedSecurities.Count; i++)
            {
                Security curSaveSec = _savedSecurities[i];

                for (int j = 0; j < securities.Count; j++)
                {
                    var security = securities[j];
                    if (security.Name == curSaveSec.Name
                        && security.NameId == curSaveSec.NameId
                        && security.SecurityType == curSaveSec.SecurityType
                        && security.NameClass == curSaveSec.NameClass)
                    {
                        security.Lot = curSaveSec.Lot;
                        security.PriceStep = curSaveSec.PriceStep;
                        security.PriceStepCost = curSaveSec.PriceStepCost;
                        security.Decimals = curSaveSec.Decimals;
                        security.DecimalsVolume = curSaveSec.DecimalsVolume;
                        security.MinTradeAmount = curSaveSec.MinTradeAmount;
                        security.MinTradeAmountType = curSaveSec.MinTradeAmountType;
                        security.VolumeStep = curSaveSec.VolumeStep;
                        security.PriceLimitHigh = curSaveSec.PriceLimitHigh;
                        security.PriceLimitLow = curSaveSec.PriceLimitLow;
                        security.Go = curSaveSec.Go;
                        security.Strike = curSaveSec.Strike;

                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            OnLogRecieved(ex.ToString(), LogMessageType.Error);
        }
    }



    /// <summary>
    /// multithreaded access locker in StartThisSecurity
    /// </summary>
    private string _lockerStarter = "lockerStarterAserver";

    /// <summary>
    /// start uploading data on instrument
    /// </summary>
    /// <param name="securityName"> security name for running</param>
    /// <param name="timeFrameBuilder"> object that has data about timeframe</param>
    /// <param name="securityClass"> security class for running</param>
    /// <returns> returns CandleSeries if successful else null</returns>
    public CandleSeries StartThisSecurity(string securityName, TimeFrameBuilder timeFrameBuilder, string securityClass)
    {
        try
        {
            lock (_lockerStarter)
            {
                if (securityName == ""
                    || Portfolios == null
                    || Securities == null
                    || _candleManager == null
                    || ServerStatus != ServerConnectStatus.Connect
                    || (LastStartServerTime != DateTime.MinValue
                        && LastStartServerTime.AddSeconds(15) > DateTime.Now))
                {
                    return null;
                }

                Security security = null;

                for (int i = 0; Securities != null && i < Securities.Count; i++)
                {
                    if (Securities[i] == null) { continue; }

                    if (Securities[i].Name == securityName
                        && (securityClass == null
                            || Securities[i].NameClass == securityClass))
                    {
                        security = Securities[i];
                        break;
                    }
                }

                if (security == null) { return null; }

                CandleSeries series = new(timeFrameBuilder, security, StartProgram.IsOsTrader);

                ServerRealization.Subscrible(security);

                _candleManager.StartSeries(series);

                OnLogRecieved(OsLocalization.Market.Message14 + series.Security.Name +
                              OsLocalization.Market.Message15 + series.TimeFrame +
                              OsLocalization.Market.Message16, LogMessageType.System);

                _tradesStorage?.SetSecurityToSave(security);

                _candleStorage.SetSeriesToSave(series);

                SubscribeSecurity(securityName, securityClass);

                return series;
            }
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);

            return null;
        }
    }

    /// <summary>
    /// stop the downloading of candles
    /// </summary>
    /// <param name="series"> candles series that need to stop</param>
    public void StopThisSecurity(CandleSeries series)
    {
        try
        {
            if (ServerStatus != ServerConnectStatus.Connect)
            {
                return;
            }

            if (series != null && _candleManager != null)
            {
                _candleManager.StopSeries(series);
            }

            _candleStorage?.RemoveSeries(series);

            Security security = series.Security;

            if (_candleManager != null &&
                _candleManager.IsSafeToUnsubscribeFromSecurityUpdates(security))
            {
                ServerRealization.Unsubscribe(security);
                RemoveSecurityFromSubscribed(security.Name, security.NameClass);
            }
        }
        catch (Exception ex)
        {
            OnLogRecieved(ex.ToString(), LogMessageType.Error);
        }
    }

    private void SubscribeSecurity(string securityName, string securityClass)
    {
        if (_checkDataFlowIsOn == false)
        {
            return;
        }

        // string[] ignoreClasses = ServerPermission.CheckDataFeedLogic_ExceptionSecuritiesClass;
        string[] ignoreClasses = Permissions.CheckDataFeedLogic_ExceptionSecurities;

        if (ignoreClasses != null)
        {
            for (int i = 0; i < ignoreClasses.Length; i++)
            {
                if (ignoreClasses[i].Equals(securityClass))
                {
                    return;
                }
            }
        }

        for (int i = 0; i < _subscribeSecurities.Count; i++)
        {
            if (_subscribeSecurities[i].SecurityName == securityName
                && _subscribeSecurities[i].SecurityClass == securityClass)
            {
                return;
            }
        }

        SecurityFlowTime newSubscribeSecurity = new()
        {
            SecurityName = securityName,
            SecurityClass = securityClass
        };

        _subscribeSecurities.Add(newSubscribeSecurity);
    }

    private void RemoveSecurityFromSubscribed(string securityName, string securityClass)
    {
        // remove security from subscribed list
        if (_subscribeSecurities == null || _subscribeSecurities.Count == 0)
        {
            return;
        }

        if (securityName == null || securityClass == null)
        {
            return;
        }

        for (int i = 0; i < _subscribeSecurities.Count; i++)
        {
            if (_subscribeSecurities[i].SecurityName == securityName
                && _subscribeSecurities[i].SecurityClass == securityClass)
            {
                _subscribeSecurities.RemoveAt(i);
                return;
            }
        }
    }
}
