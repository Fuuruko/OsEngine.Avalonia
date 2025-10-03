/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.IO;
using System.Text;
using OsEngine.Models.Candles.Factory;
using OsEngine.Models.Candles.Series;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Candles;

/// <summary>
/// time frame settings storage
/// </summary>
public class TimeFrameBuilder
{

    public TimeFrameBuilder(string name, StartProgram startProgram)
    {
        _name = name;

        if (startProgram != StartProgram.IsOsOptimizer)
        {
            Load();
        }
        else
        {
            CandleSeriesRealization = new Simple();
            CandleSeriesRealization.ParametersChangeByUser += SetSpecification;
            TimeFrame = TimeFrame.Min1;
        }

        SetSpecification();
        _candleCreateMethodType = CandleSeriesRealization.GetType().Name;
    }

    public TimeFrameBuilder(StartProgram startProgram)
    {
        CandleSeriesRealization = new Simple();
        CandleSeriesRealization.ParametersChangeByUser += Save;
        TimeFrame = TimeFrame.Min1;

        SetSpecification();
        _candleCreateMethodType = CandleSeriesRealization.GetType().Name;
    }

    public ACandlesSeriesRealization CandleSeriesRealization;

    private string _name;


    private void Load()
    {
        if (!File.Exists(@"Engine\" + _name + @"TimeFrameBuilder.txt"))
        {
            // CandleSeriesRealization = CandleFactory.CreateCandleSeriesRealization("Simple");
            CandleSeriesRealization = new Simple();
            CandleSeriesRealization.ParametersChangeByUser += Save;
            return;
        }
        try
        {
            using StreamReader reader = new(@"Engine\" + _name + @"TimeFrameBuilder.txt");

            Enum.TryParse(reader.ReadLine(), out TimeFrame frame);

            _saveTradesInCandles = Convert.ToBoolean(reader.ReadLine());
            Enum.TryParse(reader.ReadLine(), out _candleCreateType);

            string seriesName = reader.ReadLine();
            CandleSeriesRealization = CandleFactory.CreateCandleSeriesRealization(seriesName);
            CandleSeriesRealization.SetSaveString(reader.ReadLine());
            CandleSeriesRealization.OnStateChange(CandleSeriesState.ParametersChange);
            TimeFrame = frame;

            CandleSeriesRealization.ParametersChangeByUser += Save;
            reader.Close();
        }
        catch
        {
            // ignore
        }

        if (CandleSeriesRealization == null)
        {
            // CandleSeriesRealization = CandleFactory.CreateCandleSeriesRealization("Simple");
            CandleSeriesRealization = new Simple();
            CandleSeriesRealization.ParametersChangeByUser += Save;
        }
    }

    public void Save()
    {
        SetSpecification();

        try
        {
            using StreamWriter writer = new(@"Engine\" + _name + @"TimeFrameBuilder.txt", false);
            writer.WriteLine(TimeFrame);
            writer.WriteLine(_saveTradesInCandles);
            writer.WriteLine(_candleCreateType);
            writer.WriteLine(CandleSeriesRealization.GetType().Name);
            writer.WriteLine(CandleSeriesRealization.GetSaveString());

            writer.Close();
        }
        catch
        {
            // ignore
        }
    }

    public void Delete()
    {
        try
        {
            if (File.Exists(@"Engine\" + _name + @"TimeFrameBuilder.txt"))
            {
                File.Delete(@"Engine\" + _name + @"TimeFrameBuilder.txt");
            }
        }
        catch
        {
            // ignore
        }

        try
        {
            CandleSeriesRealization?.Delete();
            CandleSeriesRealization = null;
        }
        catch
        {
            // ignore
        }
    }

    public string CandleCreateMethodType
    {
        get => _candleCreateMethodType;
        set
        {
            string newType = value;

            if (newType == _candleCreateMethodType) { return; }

            CandleSeriesRealization?.Delete();

            _candleCreateMethodType = newType;
            CandleSeriesRealization = CandleFactory.CreateCandleSeriesRealization(newType);
            CandleSeriesRealization.ParametersChangeByUser += Save;

            Save();
        }
    }
    private string _candleCreateMethodType;

    public CandleMarketDataType CandleMarketDataType
    {
        get => _candleCreateType;
        set
        {
            if (value != _candleCreateType)
            {
                _candleCreateType = value;
                Save();
            }
        }
    }
    private CandleMarketDataType _candleCreateType;

    public string Specification { get; set; }

    private void SetSpecification()
    {
        if (CandleSeriesRealization == null)
        {
            Specification = null;
        }

        StringBuilder result = new();

        result.Append(_candleCreateType + "_");
        result.Append(_saveTradesInCandles + "_");

        string series = CandleSeriesRealization.GetType().Name + "_";
        series += CandleSeriesRealization.GetSaveString();

        result.Append(series);

        Specification = result.ToString().Replace(",", ".");
    }

    public TimeFrame TimeFrame
    {
        get;
        set
        {
            try
            {
                if (value != field ||
                    value == TimeFrame.Sec1)
                {
                    field = value;
                    var timeSpan = value.GetTimeSpan();
                    if (timeSpan != null) { TimeFrameTimeSpan = (TimeSpan)timeSpan; }

                    if (CandleSeriesRealization == null)
                    {
                        Save();
                        return;
                    }

                    CandleSeriesRealization.TimeFrame = value;

                    Save();
                }
            }
            catch
            {
                // ignore
            }
        }
    }

    public TimeSpan TimeFrameTimeSpan { get; private set; }

    public bool SaveTradesInCandles
    {
        get { return _saveTradesInCandles; }
        set
        {
            if (value == _saveTradesInCandles) { return; }
            _saveTradesInCandles = value;
            Save();
        }
    }

    private bool _saveTradesInCandles;
}
