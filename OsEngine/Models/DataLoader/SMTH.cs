using System;
using System.Collections.Generic;
using System.IO;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Data;

public class DataPie(string tempFileDirectory)
{
    public string _pathMyTempPieInTfFolder = tempFileDirectory;

    public int CountTriesToLoadSet
    {
        get => _countTriesToLoadSet;
        set
        {
            if (_countTriesToLoadSet == value) { return; }

            _countTriesToLoadSet = value;
            SavePieSettings();
        }
    }
    private int _countTriesToLoadSet;

    public void Delete()
    {

    }

    public void Clear()
    {
        try
        {
            CountTriesToLoadSet = 0;
            ObjectCount = 0;
            StartFact = DateTime.MinValue;
            EndFact = DateTime.MinValue;
            Status = DataPieStatus.None;

            if (File.Exists(_pathMyTempPieInTfFolder + "\\" + TempFileName))
            {
                File.Delete(_pathMyTempPieInTfFolder + "\\" + TempFileName);
            }
        }
        catch
        {
            // ignore
        }
    }

    public void UpDateStatus()
    {
        // 1 Актуальное время старта
        CandlePieStatusInfo CandlesInfo = null;

        if (_pathMyTempPieInTfFolder.Contains("Tick") == false
                &&
                _pathMyTempPieInTfFolder.Contains("Sec") == false)
        {
            CandlesInfo = LoadCandlesPieStatus();
        }

        TradePieStatusInfo TradesInfo = null;

        if ((CandlesInfo == null
                    || CandlesInfo.FirstCandle == null)
                && 
                (_pathMyTempPieInTfFolder.Contains("Tick") == true
                 || _pathMyTempPieInTfFolder.Contains("Sec") == true))
        {
            TradesInfo = LoadTradesPieStatus();
        }

        DateTime start = DateTime.MinValue;

        if (CandlesInfo != null && CandlesInfo.FirstCandle != null)
        {
            start = CandlesInfo.FirstCandle.TimeStart;
        }

        if (TradesInfo != null && TradesInfo.FirstTrade != null)
        {
            start = TradesInfo.FirstTrade.Time;
        }

        StartFact = start;

        // 2 актуальное время конца

        DateTime end = DateTime.MinValue;

        if (CandlesInfo != null && CandlesInfo.LastCandle != null)
        {
            end = CandlesInfo.LastCandle.TimeStart;
        }

        if (TradesInfo != null && TradesInfo.LastTrade != null)
        {
            end = TradesInfo.LastTrade.Time;
        }

        EndFact = end;

        if (CandlesInfo == null &&
                TradesInfo == null)
        {
            ObjectCount = 0;
        }

        if (CandlesInfo != null)
        {
            ObjectCount = CandlesInfo.CandlesCount;
        }

        if (TradesInfo != null)
        {
            ObjectCount = TradesInfo.TradesCount;
        }
    }

    public DateTime Start;

    public DateTime StartFact;

    public DateTime End;

    public DateTime EndFact;

    public DataPieStatus Status;

    public int ObjectCount;

    public string TempFileName
    {
        get
        {
            if (_tempFileName != null)
            {
                return _tempFileName;
            }

            _tempFileName = Start.ToString("yyyyMMdd") + "_" + End.ToString("yyyyMMdd") + ".txt";

            return _tempFileName;
        }
    }

    private string _tempFileName;

    public void LoadPieSettings()
    {
        string pathToTempFile = _pathMyTempPieInTfFolder  + "\\" + "Settings_" + TempFileName;

        if (File.Exists(pathToTempFile) == false)
        {
            return;
        }

        try
        {
            using StreamReader reader = new(pathToTempFile);
            _countTriesToLoadSet = Convert.ToInt32(reader.ReadLine());
        }
        catch (Exception error)
        {
            //SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    private void SavePieSettings()
    {
        string pathToTempFile = _pathMyTempPieInTfFolder  + "\\" + "Settings_" + TempFileName;

        try
        {
            using StreamWriter writer = new(pathToTempFile, false);

            writer.WriteLine(CountTriesToLoadSet);
        }
        catch (Exception error)
        {
            //SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    #region Candles

    public void SetNewCandlesInPie(List<Candle> candles)
    {
        SaveCandleDataPieInTempFile(candles);
        UpDateStatus();
    }

    public CandlePieStatusInfo LoadCandlesPieStatus()
    {
        string pathToTempFile = _pathMyTempPieInTfFolder + "\\" + TempFileName;

        if (File.Exists(pathToTempFile) == false)
        {
            return null;
        }

        CandlePieStatusInfo result = new();

        int candlesCount = 0;

        try
        {
            using StreamReader reader = new(pathToTempFile);
            while (reader.EndOfStream == false)
            {
                candlesCount++;
                string str = reader.ReadLine();

                if (result.FirstCandle == null)
                {
                    Candle newCandle = new();
                    newCandle.SetCandleFromString(str);
                    result.FirstCandle = newCandle;
                }
                if (reader.EndOfStream == true)
                {
                    Candle newCandle = new();
                    newCandle.SetCandleFromString(str);
                    result.LastCandle = newCandle;
                }
            }
        }
        catch (Exception error)
        {
            //SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }

        result.CandlesCount = candlesCount;

        if (result.CandlesCount != 0)
        {
            Status = DataPieStatus.Load;
        }

        return result;
    }

    public List<Candle> LoadCandleDataPieInTempFile()
    {
        string pathToTempFile = _pathMyTempPieInTfFolder + "\\" + TempFileName;

        if (File.Exists(pathToTempFile) == false)
        {
            return null;
        }

        List<Candle> candles = [];

        try
        {
            using StreamReader reader = new(pathToTempFile);
            while (reader.EndOfStream == false)
            {
                string str = reader.ReadLine();

                Candle newCandle = new();
                newCandle.SetCandleFromString(str);
                candles.Add(newCandle);
            }
        }
        catch (Exception error)
        {
            //SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }

        if (candles.Count != 0)
        {
            Status = DataPieStatus.Load;
        }

        return candles;
    }

    private void SaveCandleDataPieInTempFile(List<Candle> candles)
    {
        string pathToTempFile = _pathMyTempPieInTfFolder + "\\" + TempFileName;

        try
        {
            DateTime realEnd = End.AddDays(1);

            using StreamWriter writer = new(pathToTempFile, false);
            for (int i = 0; i < candles.Count; i++)
            {
                if (candles[i].TimeStart < Start)
                {
                    continue;
                }

                if (candles[i].TimeStart > realEnd)
                {
                    break;
                }

                writer.WriteLine(candles[i].StringToSave);
            }
        }
        catch (Exception error)
        {
            //SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    #endregion

    #region Trades

    public TradePieStatusInfo LoadTradesPieStatus()
    {
        string pathToTempFile = _pathMyTempPieInTfFolder + "\\" + TempFileName;

        if (File.Exists(pathToTempFile) == false)
        {
            return null;
        }

        TradePieStatusInfo info = new();

        int tradesCount = 0;

        try
        {
            using StreamReader reader = new(pathToTempFile);
            while (reader.EndOfStream == false)
            {
                tradesCount++;
                string str = reader.ReadLine();

                if (info.FirstTrade == null)
                {
                    Trade firstTrade = new();
                    firstTrade.SetTradeFromString(str);
                    info.FirstTrade = firstTrade;
                }

                if (reader.EndOfStream == true)
                {
                    Trade lastTrade = new();
                    lastTrade.SetTradeFromString(str);
                    info.LastTrade = lastTrade;
                }
            }
        }
        catch (Exception error)
        {
            //SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }

        info.TradesCount = tradesCount;

        if (info.FirstTrade != null)
        {
            Status = DataPieStatus.Load;
        }

        return info;
    }

    public List<Trade> LoadTradeDataPieFromTempFile()
    {

        string pathToTempFile = _pathMyTempPieInTfFolder + "\\" + Start.ToString("yyyyMMdd") + "_" + End.ToString("yyyyMMdd") + ".txt";

        if (File.Exists(pathToTempFile) == false)
        {
            return null;
        }

        List<Trade> trades = [];

        try
        {
            using StreamReader reader = new(pathToTempFile);
            while (reader.EndOfStream == false)
            {
                string str = reader.ReadLine();

                Trade newTrade = new();
                newTrade.SetTradeFromString(str);
                trades.Add(newTrade);
            }
        }
        catch (Exception error)
        {
            //SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }

        if (trades.Count != 0)
        {
            Status = DataPieStatus.Load;
        }

        return trades;
    }

    public void SetNewTradesInPie(List<Trade> trades)
    {
        SaveTradesDataPieInTempFile(trades);
        UpDateStatus();
    }

    private void SaveTradesDataPieInTempFile(List<Trade> trades)
    {
        string pathToTempFile = _pathMyTempPieInTfFolder + "\\" + TempFileName;

        try
        {
            DateTime realEnd = End.AddDays(1);

            using StreamWriter writer = new(pathToTempFile, false);
            for (int i = 0; i < trades.Count; i++)
            {
                if (trades[i].Time < Start)
                {
                    continue;
                }

                if (trades[i].Time > realEnd)
                {
                    break;
                }

                writer.WriteLine(trades[i].GetSaveString());
            }
        }
        catch (Exception error)
        {
            //SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    #endregion
}

public enum SecurityLoadStatus
{
    None,
    Activate,
    Load,
    Loading,
    Error
}

public enum DataPieStatus
{
    None,
    Load,
    InProcess
}

public class TradePieStatusInfo
{
    public Trade FirstTrade;

    public Trade LastTrade;

    public int TradesCount;

}

public class CandlePieStatusInfo
{
    public Candle FirstCandle;

    public Candle LastCandle;

    public int CandlesCount;
}
