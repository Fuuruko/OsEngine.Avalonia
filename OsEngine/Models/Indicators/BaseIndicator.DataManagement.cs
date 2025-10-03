using System;
using System.IO;
using Newtonsoft.Json;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Indicators;

public partial class BaseIndicator
{
    public void Clear()
    {
        _myCandles = [];

        foreach (var series in DataSeries)
        {
            series.Clear();
        }

        foreach (var i in IncludeIndicators)
        {
            i.Clear();
        }
    }

    protected internal void Delete()
    {
        if (StartProgram != StartProgram.IsOsOptimizer)
        {
            $@"Engine\{Name}Values.txt".TryDelete();
            $@"Engine\{Name}Parameters.txt".TryDelete();
        }

        foreach (var s in DataSeries)
        {
            s.Delete();
        }
        DataSeries.Clear();

        foreach (var i in IncludeIndicators)
        {
            i.Delete();
        }
        IncludeIndicators.Clear();

        foreach (var p in Parameters)
        {
            p.ValueChanged -= OnParameterChangedByUser;
        }
        Parameters.Clear();
        ParametersDigit.Clear();

        _myCandles = null;
    }

    public void Load()
    {
        if (Name == "")
        {
            return;
        }
    }

    protected internal void Save()
    {
        if (StartProgram == StartProgram.IsOsOptimizer)
        {
            return;
        }

        SaveParameters();
        SaveSeries();

    }

    /// <summary>
    /// load parameter settings
    /// </summary>
    private IndicatorParameter LoadParameterValues(IndicatorParameter newParameter)
    {
        GetValueParameterSaveByUser(newParameter);

        newParameter.ValueChange += OnParameterChangedByUser;

        // Parameters.Add(newParameter);

        return newParameter;
    }

    /// <summary>
    /// load parameter settings from file
    /// </summary>
    private void GetValueParameterSaveByUser(IndicatorParameter parameter)
    {
        if (!File.Exists($@"Engine\{Name}Parameters.txt"))
        {
            return;
        }
        try
        {
            using StreamReader reader = new($@"Engine\{Name}Parameters.txt");
            while (!reader.EndOfStream)
            {
                string[] save = reader.ReadLine().Split('#');

                if (save[0] == parameter.Name)
                {
                    parameter.LoadParamFromString(save);
                }
            }
            reader.Close();
        }
        catch (Exception)
        {
            // ignore
        }
    }

    /// <summary>
    /// save parameter values
    /// </summary>
    private void SaveParameters()
    {
        if (Parameters.Count == 0) { return; }

        try
        {
            using StreamWriter writer = new($@"Engine\{Name}Parameters.txt", false);
            for (int i = 0; i < Parameters.Count; i++)
            {
                writer.WriteLine(Parameters[i].GetStringToSave());
            }

            writer.Close();
        }
        catch (Exception)
        {
            // ignore
        }
    }

    private void CheckSeriesParametersInSaveData(BaseSeries series)
    {
        if (!File.Exists($@"Engine\{Name}Values.txt")) { return; }

        try
        {
            using StreamReader reader = new($@"Engine\{Name}Values.txt");
            while (!reader.EndOfStream)
            {
                string[] save = reader.ReadLine().Split('&');

                if (save[0] == series.Name)
                {
                    // series.LoadFromStr(save);
                }
            }
            reader.Close();
        }
        catch (Exception)
        {
            // ignore
        }
    }

    private void SaveSeries()
    {
        if (DataSeries.Count == 0) { return; }

        try
        {
            // using StreamWriter writer = new($@"Engine\{Name}Values.txt", false);
            // for (int i = 0; i < DataSeries.Count; i++)
            // {
            //     writer.WriteLine(DataSeries[i].GetSaveStr());
            // }
            string serieses = JsonConvert.SerializeObject(DataSeries);
            File.WriteAllText($@"Engine\{Name}Values.txt", serieses);

            // writer.Close();
        }
        catch (Exception)
        {
            // ignore
        }
    }
}
