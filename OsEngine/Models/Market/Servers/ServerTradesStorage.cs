/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;

namespace OsEngine.Models.Market.Servers;

/// <summary>
/// server ticks storage
/// </summary>
public class ServerTradesStorage
{
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="server"> server for saving trades</param>
    public ServerTradesStorage(BaseServer server)
    {
        _server = server;

        var s = Path.DirectorySeparatorChar;
        _pathName = $"Data{Path.DirectorySeparatorChar}"
            + $"{server.ServerNameUnique}Trades";

        Thread saver = new(TickSaverSpaceInOneFile)
        {
            CurrentCulture = new CultureInfo("RU-ru"),
            IsBackground = false
        };
        saver.Start();
    }

    /// <summary>
    /// Serviced connection
    /// </summary>
    private IServer _server;

    /// <summary>
    /// shows whether need to save trades
    /// </summary>
    public required Input.Bool IsSaveTrades { get; init; }

    /// <summary>
    /// how many days upload from history
    /// </summary>
    public required Input.Int DaysToLoad { get; init; }

    /// <summary>
    /// directory for saving data
    /// </summary>
    private string _pathName;

    /// <summary>
    /// securities for saving
    /// </summary>
    private List<Security> _securities = [];

    /// <summary>
    /// save security data 
    /// </summary>
    public void SetSecurityToSave(Security security)
    {
        if (_securities.Find(security1 => security1.Name == security.Name) == null)
        {
            _securities.Add(security);
        }
    }

    /// <summary>
    /// upload ticks for some instrument
    /// </summary>
    public event Action<List<Trade>[]> TickLoadedEvent;

    /// <summary>
    /// service information for saving trades
    /// </summary>
    private List<TradeSaveInfo> _tradeSaveInfo;

    // for saving in one file

    /// <summary>
    /// method with tick saving thread
    /// </summary>
    private void TickSaverSpaceInOneFile()
    {
        _tradeSaveInfo = [];

        if (!Directory.Exists(_pathName))
        {
            Directory.CreateDirectory(_pathName);
        }
        while (true)
        {
            try
            {
                Thread.Sleep(15000);

                if (_server.ServerStatus != ServerConnectStatus.Connect
                        || IsSaveTrades == false
                        || _weLoadTrades == false)
                {
                    continue;
                }

                if (MainWindow.ProccesIsWorked == false)
                {
                    return;
                }

                List<Trade>[] allTrades = _server.AllTrades;

                for (int i1 = 0;
                    allTrades != null && Thread.CurrentThread.Name != "deleteThread" && i1 < allTrades.Length;
                    i1++)
                {
                    if (allTrades[i1].Count == 0) { continue; }

                    if (MainWindow.ProccesIsWorked == false)
                    {
                        // если приложение закрывается
                        return;
                    }

                    if (_securities.Find(security => security.Name == allTrades[i1][0].SecurityNameCode) == null)
                    {
                        continue;
                    }

                    TradeSaveInfo tradeInfo =
                        _tradeSaveInfo.Find(s => s.SecurityName == allTrades[i1][0].SecurityNameCode);

                    if (tradeInfo == null)
                    {
                        tradeInfo = new TradeSaveInfo
                        {
                            SecurityName = allTrades[i1][0].SecurityNameCode
                        };
                        _tradeSaveInfo.Add(tradeInfo);
                    }

                    if (tradeInfo.LastSaveIndex >= allTrades[i1].Count)
                    {
                        continue;
                    }

                    int lastSecond = allTrades[i1][tradeInfo.LastSaveIndex].Time.Second;
                    int lastMillisecond = allTrades[i1][tradeInfo.LastSaveIndex].MicroSeconds;

                    StreamWriter writer =
                        new(_pathName + @"\" + allTrades[i1][0].SecurityNameCode + ".txt", true);
                    for (int i = tradeInfo.LastSaveIndex; i < allTrades[i1].Count - 1; i++)
                    {
                        if (allTrades[i1][i].MicroSeconds == 0)
                        { // for some time in microseconds if the connector did not issue them to us / генерим какое-то время микросекунд, если нам коннектор их не выдал
                            if (lastSecond != allTrades[i1][i].Time.Second)
                            {
                                lastMillisecond = 0;
                                lastSecond = allTrades[i1][i].Time.Second;
                            }

                            allTrades[i1][i].MicroSeconds = lastMillisecond += 10;
                        }

                        writer.WriteLine(allTrades[i1][i].GetSaveString());
                    }
                    tradeInfo.LastSaveIndex = allTrades[i1].Count - 1;
                    writer.Close();


                }
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
        }
    }

    private bool _weLoadTrades;

    /// <summary>
    /// upload ticks
    /// </summary>
    /// <param name="dayCount"> number of days for uploading </param>
    public List<Trade>[] LoadTrades()
    {
        try
        {
            if (!Directory.Exists(_pathName))
            {
                _weLoadTrades = true;
                return null;
            }

            List<Trade>[] allTrades = _server.AllTrades;

            string[] saves = Directory.GetFiles(_pathName);

            for (int i = 0; i < saves.Length; i++)
            {
                // upload / загружаем
                StreamReader reader = new(saves[i]);

                List<Trade> newList = [];

                string nameSecurity;

                try
                {
                    string[] array = saves[i].Split('\\');

                    nameSecurity = array[2].Split('.')[0];
                }
                catch
                {
                    continue;
                }

                try
                {
                    DateTime timeStart = DateTime.Now.AddDays(-DaysToLoad - 1);

                    if (timeStart.Month == 1 && timeStart.Day < 10)
                    {
                        timeStart = timeStart.AddDays(-10);
                    }

                    List<string> tradesInStr = [];

                    while (!reader.EndOfStream)
                    {
                        tradesInStr.Add(reader.ReadLine());
                    }

                    for (int i2 = 0; i2 < tradesInStr.Count; i2++)
                    {
                        Trade newTrade = new();

                        string curTrade = tradesInStr[i2];

                        try
                        {
                            newTrade.SetTradeFromString(curTrade);
                        }
                        catch
                        {
                            continue;
                        }

                        newTrade.SecurityNameCode = nameSecurity;

                        if (newTrade.Time.Date < timeStart.Date)
                        {
                            i2 += 100;
                            continue;
                        }

                        newList.Add(newTrade);
                    }
                }
                catch (Exception)
                {
                    continue;
                }

                // save / сохраняем

                if (newList.Count == 0)
                {
                    continue;
                }

                if (_tradeSaveInfo.Find(s => s.SecurityName == newList[0].SecurityNameCode) == null)
                {
                    TradeSaveInfo tradeInfo = new()
                    {
                        SecurityName = newList[0].SecurityNameCode,
                        LastSaveIndex = newList.Count
                    };
                    _tradeSaveInfo.Add(tradeInfo);
                }

                if (allTrades == null)
                {
                    allTrades = [newList];
                }
                else
                {
                    List<Trade>[] newListsArray = new List<Trade>[allTrades.Length + 1];
                    for (int ii = 0; ii < allTrades.Length; ii++)
                    {
                        newListsArray[ii] = allTrades[ii];
                    }
                    newListsArray[^1] = newList;
                    allTrades = newListsArray;
                }

                reader.Close();
            }

            return allTrades;
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
        _weLoadTrades = true;
        return null;
    }

    // log messages

    /// <summary>
    /// send a new message to up
    /// </summary>
    private void SendNewLogMessage(string message, LogMessageType type)
    {
        if (LogMessageEvent != null)
        {
            LogMessageEvent(message, type);
        }
        else if (type == LogMessageType.Error)
        { // if nobody is subscribed to us and there is a log error / если на нас никто не подписан и в логе ошибка
            MessageBox.Show(message);
        }
    }

    /// <summary>
    /// outgoing log message
    /// </summary>
    public event Action<string, LogMessageType> LogMessageEvent;

    /// <summary>
    /// information to save trades
    /// </summary>
    private class TradeSaveInfo
    {
        public string SecurityName;

        /// <summary>
        /// last save time
        /// </summary>
        public DateTime LastSaveObjectTime;

        /// <summary>
        /// the last trade Id we saved
        /// </summary>
        public string LastTradeId;

        /// <summary>
        /// last stored index
        /// </summary>
        public int LastSaveIndex;
    }
}

