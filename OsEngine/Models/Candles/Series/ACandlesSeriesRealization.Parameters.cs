/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System.Collections.Generic;
using OsEngine.Models.Candles.Factory;

namespace OsEngine.Models.Candles.Series;

// NOTE: Rename?
public abstract partial class ACandlesSeriesRealization
{
    public CandlesParameterDecimal CreateParameterDecimal(string name, string label, decimal value)
    {
        ICandleSeriesParameter newParameter = Parameters.Find(p => p.SysName == name);

        if (newParameter != null)
        {
            return (CandlesParameterDecimal)newParameter;
        }

        newParameter = new CandlesParameterDecimal(name, label, value);
        Parameters.Add(newParameter);
        LoadParameterValues(newParameter);

        return (CandlesParameterDecimal)newParameter;
    }

    public CandlesParameterInt CreateParameterInt(string name, string label, int value)
    {
        ICandleSeriesParameter newParameter = Parameters.Find(p => p.SysName == name);

        if (newParameter != null)
        {
            return (CandlesParameterInt)newParameter;
        }

        newParameter = new CandlesParameterInt(name, label, value);
        Parameters.Add(newParameter);
        LoadParameterValues(newParameter);

        return (CandlesParameterInt)newParameter;
    }

    public CandlesParameterString CreateParameterStringCollection(string name, string label,
            string value, List<string> collection)
    {
        ICandleSeriesParameter newParameter = Parameters.Find(p => p.SysName == name);

        if (newParameter != null)
        {
            return (CandlesParameterString)newParameter;
        }

        newParameter = new CandlesParameterString(name, label, value, collection);
        Parameters.Add(newParameter);
        LoadParameterValues(newParameter);

        return (CandlesParameterString)newParameter;
    }

    public CandlesParameterBool CreateParameterBool(string name, string label, bool value)
    {
        ICandleSeriesParameter newParameter = Parameters.Find(p => p.SysName == name);

        if (newParameter != null)
        {
            return (CandlesParameterBool)newParameter;
        }

        newParameter = new CandlesParameterBool(name, label, value);
        Parameters.Add(newParameter);
        LoadParameterValues(newParameter);

        return (CandlesParameterBool)newParameter;
    }
}
