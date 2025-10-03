/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using OsEngine.Models.Candles;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;

namespace OsEngine.Models.Market.Servers;

public class ServerCandleStorage
{
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="server"> server for saving candles </param>
    public ServerCandleStorage(BaseServer server)
    {
        _pathName = $@"Data{Path.DirectorySeparatorChar}"
            + $"{server.ServerNameUnique}Candles";

        Thread saver = new(CandleSaverSpaceInOneFile)
        {
            CurrentCulture = new CultureInfo("RU-ru"),
            IsBackground = false
        };
        saver.Start();
    }

    /// <summary>
    /// directory for saving data
    /// </summary>
    private string _pathName;

    /// <summary>
    /// is the service enabled
    /// </summary>
    public required Input.Bool IsSaveCandles { get; init; }

    /// <summary>
    /// number of candles to be saved to the file system
    /// </summary>
    public required Input.Int SaveCandlesNumber { get; init; }

    /// <summary>
    /// securities for saving
    /// </summary>
    private List<CandleSeries> _series = [];

    /// <summary>
    /// save security data
    /// </summary>
    public void SetSeriesToSave(CandleSeries series)
    {
        string spec = series.Specification;

        if(string.IsNullOrEmpty(spec))
        {
            return;
        }

        for (int i = 0; i < _series.Count; i++)
        {
            if(_series[i] == null)
            {
                continue;
            }
            if (_series[i].Specification == spec)
            {
                _series.RemoveAt(i);
                break;
            }
        }

        for (int i = 0; i < _series.Count; i++)
        {
            if (_series[i] == null)
            {
                continue;
            }
            if (_series[i].UID == series.UID)
            {
                _series.RemoveAt(i);
                break;
            }
        }

        _series.Add(series);
    }

    /// <summary>
    /// delete the data series from the save
    /// </summary>
    public void RemoveSeries(CandleSeries series)
    {
        for (int i = 0; i < _series.Count; i++)
        {
            if (_series[i] == null)
            {
                continue;
            }
            if (_series[i].UID == series.UID)
            {
                _series.RemoveAt(i);
                break;
            }
        }
    }

    // saving in file

    /// <summary>
    /// method with candles saving thread
    /// </summary>
    private void CandleSaverSpaceInOneFile()
    {
        if (!Directory.Exists(_pathName))
        {
            Directory.CreateDirectory(_pathName);
        }

        while (true)
        {
            try
            {
                Thread.Sleep(60000);

                if (MainWindow.ProccesIsWorked == false)
                {
                    return;
                }

                if (IsSaveCandles == false)
                {
                    continue;
                }

                for (int i = 0; i < _series.Count; i++)
                {
                    if (_series[i] == null)
                    {
                        continue;
                    }
                    if (MainWindow.ProccesIsWorked == false)
                    {
                        return;
                    }

                    SaveSeries(_series[i]);
                }
            }
            catch (Exception error)
            {
                string msg = error.Message;

                if(msg.Contains("ThrowArgumentOutOfRangeException") == false)
                {
                    SendNewLogMessage(error.ToString(), LogMessageType.Error);
                }
            }
        }

    }

    /// <summary>
    /// objects storing information on collections of stored data
    /// </summary>
    private List<CandleSeriesSaveInfo> _candleSeriesSaveInfos = [];

    /// <summary>
    /// blocker of multithreaded access to specifications of data stored by the object
    /// </summary>
    private object _lockerSpec = new();

    /// <summary>
    /// request an object that stores information on the data to be saved
    /// </summary>
    private CandleSeriesSaveInfo GetSpecInfo(string specification)
    {
        lock (_lockerSpec)
        {
            CandleSeriesSaveInfo mySaveInfo = _candleSeriesSaveInfos.Find(s => s.Specification == specification);

            if (mySaveInfo == null)
            {
                mySaveInfo = TryLoadCandle(specification);

                mySaveInfo ??= new CandleSeriesSaveInfo
                {
                    Specification = specification
                };

                _candleSeriesSaveInfos.Add(mySaveInfo);
            }

            return mySaveInfo;
        }
    }

    /// <summary>
    /// save the data series
    /// </summary>
    private void SaveSeries(CandleSeries series)
    {
        CandleSeriesSaveInfo mySaveInfo = GetSpecInfo(series.Specification);

        if (series.CandlesAll == null ||
            series.CandlesAll.Count == 0)
        {
            return;
        }

        Candle firstCandle = series.CandlesAll[0];
        Candle lastCandle = series.CandlesAll[^1];

        if (mySaveInfo.LastCandleTime != null
            && mySaveInfo.AllCandlesInFile != null)
        {
            if (firstCandle.Time == mySaveInfo.LastCandleTime &&
                lastCandle.Time == mySaveInfo.StartCandleTime &&
                lastCandle.Close == mySaveInfo.LastCandlePrice)
            {
                return;
            }
        }

        mySaveInfo.InsertCandles(series.CandlesAll, SaveCandlesNumber);

        if (Directory.Exists(_pathName) == false)
        {
            Directory.CreateDirectory(_pathName);
        }

        using StreamWriter writer = new(_pathName + "\\" + series.Specification + ".txt");
        for (int i = 0; i < mySaveInfo.AllCandlesInFile.Count; i++)
        {
            writer.WriteLine(mySaveInfo.AllCandlesInFile[i].StringToSave);
        }

        writer.Close();
    }

    /// <summary>
    /// query previously saved security candles
    /// </summary>
    public List<Candle> GetCandles(string specification, int count)
    {
        CandleSeriesSaveInfo mySaveInfo = GetSpecInfo(specification);

        List<Candle> candles = mySaveInfo.AllCandlesInFile;

        if (candles != null &&
            candles.Count != 0 &&
            candles.Count - 1 - count > 0)
        {
            candles = candles.GetRange(candles.Count - 1 - count, count);
        }

        if (candles == null)
        {
            return null;
        }

        List<Candle> newArray = [];

        for (int i = 0; i < candles.Count; i++)
        {
            newArray.Add(candles[i]);
        }

        return newArray;
    }

    /// <summary>
    /// try to load paper candle data from the file system
    /// </summary>
    private CandleSeriesSaveInfo TryLoadCandle(string specification)
    {
        List<Candle> candlesFromServer = [];

        if (File.Exists(_pathName + "\\" + specification + ".txt"))
        {
            try
            {
                using StreamReader reader = new(_pathName + "\\" + specification + ".txt");
                while (reader.EndOfStream == false)
                {
                    string str = reader.ReadLine();
                    Candle newCandle = new();
                    newCandle.SetCandleFromString(str);
                    candlesFromServer.Add(newCandle);
                }

                reader.Close();
            }
            catch (Exception e)
            {
                // ignore
            }
        }

        // далее смотрим есть ли сохранение в глобальном хранилище


        List<Candle> candlesFromOsData = [];

        string path = "Data\\ServersCandleTempData\\" + specification + ".txt";
        if (Directory.Exists("Data\\ServersCandleTempData") &&
            File.Exists(path))
        {
            try
            {
                using StreamReader reader = new(path);
                while (reader.EndOfStream == false)
                {
                    string str = reader.ReadLine();
                    Candle newCandle = new();
                    newCandle.SetCandleFromString(str);
                    candlesFromOsData.Add(newCandle);
                }

                reader.Close();
            }
            catch (Exception e)
            {
                // ignore
            }
        }

        if (candlesFromOsData.Count == 0 &&
            candlesFromServer.Count == 0)
        {
            return null;
        }

        List<Candle> resultCandles = [];

        if (candlesFromOsData.Count != 0 &&
            candlesFromServer.Count != 0)
        {
            resultCandles = candlesFromServer;
            resultCandles = resultCandles.Merge(candlesFromOsData);
        }
        else if (candlesFromServer.Count != 0)
        {
            resultCandles = candlesFromServer;
        }
        else if (candlesFromOsData.Count != 0)
        {
            resultCandles = candlesFromOsData;
        }

        CandleSeriesSaveInfo myInfo = new()
        {
            Specification = specification
        };
        myInfo.InsertCandles(resultCandles, SaveCandlesNumber);

        return myInfo;
    }

    // log messages сообщения в лог 

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
    /// information to save candles
    /// </summary>
    private class CandleSeriesSaveInfo
    {
        private int _lastCandleCount;

        private DateTime _lastCandleTime;

        public void InsertCandles(List<Candle> candles, int maxCount)
        {
            if (candles == null) { return; }

            if (AllCandlesInFile == null
                || AllCandlesInFile.Count == 0)
            { 
                // первая прогрузка свечками
                AllCandlesInFile = [];

                for (int i = 0; i < candles.Count; i++)
                {
                    AllCandlesInFile.Add(candles[i]);
                }
            }
            else if(_lastCandleCount == candles.Count &&
                    candles[^1].Time == _lastCandleTime)
            { 
                // обновилась последняя свеча
                AllCandlesInFile[^1] = candles[^1];
            }
            else if(candles.Count > 1 
                    && _lastCandleCount + 1 == candles.Count
                    && candles[^2].Time == _lastCandleTime)
            { 
                // добавилась одна свечка
                AllCandlesInFile.Add(candles[^1]);
            }
            else
            { 
                // добавилось не ясное кол-во свечей
                AllCandlesInFile = AllCandlesInFile.Merge(candles);
            }

            if (AllCandlesInFile.Count == 0)
            {
                return;
            }

            _lastCandleCount = candles.Count;
            _lastCandleTime = candles[^1].Time;

            LastCandleTime = AllCandlesInFile[^1].Time;
            StartCandleTime = AllCandlesInFile[0].Time;
            LastCandlePrice = AllCandlesInFile[^1].Close;

            TryTrim(maxCount);
        }

        private void TryTrim(int count)
        {
            if (AllCandlesInFile.Count < count) { return; }

            AllCandlesInFile = AllCandlesInFile.GetRange(AllCandlesInFile.Count - count, count);

            StartCandleTime = AllCandlesInFile[0].Time;
        }

        public List<Candle> AllCandlesInFile;

        public string Specification;

        public DateTime LastCandleTime;

        public DateTime StartCandleTime;

        public decimal LastCandlePrice;
    }
}
