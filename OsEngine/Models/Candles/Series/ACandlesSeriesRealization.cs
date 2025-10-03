/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OsEngine.Models.Candles.Factory;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Candles.Series;

// NOTE: Rename to ChartStyles
public abstract partial class ACandlesSeriesRealization
{
    public static List<Type> CandleTypes = GetDerivedClasses();

    private static List<Type> GetDerivedClasses()
    {
        var baseClass = typeof(ACandlesSeriesRealization);

        return Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass
                    && baseClass.IsAssignableFrom(t)
                    && t.Namespace == "OsEngine.Models.Candles.Series"
                    && t != baseClass
                  )
            .ToList();
    }

    // TODO: Derived classes still can be simplified
    public abstract void OnStateChange(CandleSeriesState state);

    protected abstract void PreUpdateCandle(DateTime time, decimal price,
            decimal volume, bool canPushUp, Side side);

    protected abstract void UpdateCandle(DateTime time, decimal price,
            decimal volume, bool canPushUp, Side side);

    protected abstract void FinishCandle(DateTime time, decimal price,
            decimal volume, bool canPushUp, Side side);

    // NOTE: Can be simplified if dont assign null
    // TODO: Check that there will be no null values in array
    public List<Candle> CandlesAll
    {
        get;
        set
        {
            if (value == null) { CandlesAll.Clear(); }
            field = value;
        }
    } = [];

    // NOTE: Used only in HeikenAshi
    public Security Security;

    public TimeFrame TimeFrame;

    public List<ICandleSeriesParameter> Parameters = [];

    public ACandlesSeriesRealization()
    {
        OnStateChange(CandleSeriesState.Configure);

        for (int i = 0; i < Parameters.Count; i++)
        {
            LoadParameterValues(Parameters[i]);
        }
    }

    public void Init()
    {
        OnStateChange(CandleSeriesState.Configure);

        for (int i = 0; i < Parameters.Count; i++)
        {
            LoadParameterValues(Parameters[i]);
        }
    }

    public void Delete()
    {
        CandlesAll.Clear();
        CandlesAll.TrimExcess();
        CandleUpdated = null;
        CandleFinished = null;
        ParametersChangeByUser = null;
    }

    private ICandleSeriesParameter LoadParameterValues(ICandleSeriesParameter newParameter)
    {
        newParameter.ValueChange += Parameter_ValueChange;

        return newParameter;
    }

    private void Parameter_ValueChange()
    {
        ParametersChangeByUser?.Invoke();

        OnStateChange(CandleSeriesState.ParametersChange);
    }

    public string GetSaveString()
    {
        string result = "";

        for (int i = 0; i < Parameters.Count; i++)
        {
            result += Parameters[i].GetStringToSave();

            if (i + 1 != Parameters.Count)
            {
                result += "$";
            }
        }

        return result;
    }

    public void SetSaveString(string value)
    {
        if (value == null ||
            value == "")
        {
            return;
        }
        string[] parametersInArray = value.Split('$');

        for (int i = 0; i < parametersInArray.Length; i++)
        {
            string[] curParam = parametersInArray[i].Split('#');

            for (int j = 0; j < Parameters.Count; j++)
            {
                if (curParam[0] == Parameters[j].SysName)
                {
                    Parameters[j].LoadParamFromString(curParam[1]);
                    break;
                }
            }
        }

        ParametersChangeByUser?.Invoke();
    }

    protected void OnCandleUpdated(bool canPushUp)
    {
        if (canPushUp) { CandleUpdated?.Invoke(); }
    }

    protected void OnCandleFinished(bool canPushUp)
    {
        if (canPushUp) { CandleFinished?.Invoke(); }
    }

    public Action[] GetSubscribtions()
    {
        return (Action[])CandleUpdated.GetInvocationList();
    }

    public void ClearEvents()
    {
        CandleUpdated = null;
        CandleFinished = null;
    }

    public event Action CandleUpdated;

    public event Action CandleFinished;

    public event Action ParametersChangeByUser;

}

public enum CandleSeriesState
{
    Configure,
    Dispose,
    ParametersChange
}
