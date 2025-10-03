using System;
using System.Collections.Generic;

namespace OsEngine.Models.Indicators;

public partial class BaseIndicator
{
    [Obsolete(nameof(CanUserDelete))]
    public bool CanDelete
    {
        get => CanUserDelete;
        set => CanUserDelete = value; 
    }

    [Obsolete($"Use {nameof(IsVisible)} instead")]
    public bool PaintOn {
        get => IsVisible;
        set => IsVisible = value;
    }

    [Obsolete(nameof(IncludeIndicatorsD))]
    public List<BaseIndicator> IncludeIndicators = [];
    [Obsolete(nameof(IncludeIndicatorsD))]
    public List<string> IncludeIndicatorsName = [];

    [Obsolete($"Use {nameof(Parameters)} instead")]
    public List<ParameterDigit> ParametersDigit = [];

}

public class ParameterDigit
{
    public ParameterDigit(IndicatorParameter parameter)
    {
        _parameter = parameter;
    }

    private IndicatorParameter _parameter;

    public string Name
    {
        get { return _parameter.Name; }
    }

    public decimal Value
    {
        get
        {
            if (_parameter.Type == IndicatorParameterType.Decimal)
            {
                return ((IndicatorParameterDecimal)_parameter).ValueDecimal;
            }
            else //if (_parameter.Type == IndicatorParameterType.Int)
            {
                return ((IndicatorParameterInt)_parameter).ValueInt;
            }
        }
        set
        {
            if (_parameter.Type == IndicatorParameterType.Decimal)
            {
                ((IndicatorParameterDecimal)_parameter).ValueDecimal = value;
            }
            else //if (_parameter.Type == IndicatorParameterType.Int)
            {
                ((IndicatorParameterInt)_parameter).ValueInt = (int)value;
            }
        }
    }
}
