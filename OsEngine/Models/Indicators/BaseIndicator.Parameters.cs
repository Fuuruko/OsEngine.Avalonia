using System.Collections.Generic;

namespace OsEngine.Models.Indicators;

public partial class BaseIndicator
{
    // /// <summary>
    // /// create a Decimal type parameter
    // /// </summary>
    // /// <param name="name">parameter name</param>
    // /// <param name="value">default value</param>
    // public IndicatorParameterDecimal CreateParameterDecimal(string name, decimal value)
    // {
    //     IndicatorParameter newParameter = Parameters.Find(p => p.Name == name);
    //
    //     if (newParameter != null)
    //     {
    //         return (IndicatorParameterDecimal)newParameter;
    //     }
    //
    //     newParameter = new IndicatorParameterDecimal(name, value);
    //
    //     ParameterDigit param = new(newParameter);
    //     ParametersDigit.Add(param);
    //
    //     return (IndicatorParameterDecimal)LoadParameterValues(newParameter);
    // }
    //
    // /// <summary>
    // /// create int parameter 
    // /// </summary>
    // /// <param name="name">parameter name</param>
    // /// <param name="value">default value</param>
    // public IndicatorParameterInt CreateParameterInt(string name, int value)
    // {
    //     IndicatorParameter newParameter = Parameters.Find(p => p.Name == name);
    //
    //     if (newParameter != null)
    //     {
    //         return (IndicatorParameterInt)newParameter;
    //     }
    //
    //     newParameter = new IndicatorParameterInt(name, value);
    //
    //     ParameterDigit param = new(newParameter);
    //     ParametersDigit.Add(param);
    //
    //     return (IndicatorParameterInt)LoadParameterValues(newParameter);
    // }
    //
    // /// <summary>
    // /// create string collection parameter
    // /// </summary>
    // /// <param name="name">parameter name</param>
    // /// <param name="value">default value</param>
    // /// <param name="collection">possible enumeration parameters</param>
    // public IndicatorParameterString CreateParameterStringCollection(string name, string value, List<string> collection)
    // {
    //     IndicatorParameter newParameter = Parameters.Find(p => p.Name == name);
    //
    //     if (newParameter != null)
    //     {
    //         return (IndicatorParameterString)newParameter;
    //     }
    //
    //     newParameter = new IndicatorParameterString(name, value, collection);
    //
    //     return (IndicatorParameterString)LoadParameterValues(newParameter);
    // }
    //
    // /// <summary>
    // /// create string parameter
    // /// </summary>
    // /// <param name="name">parameter name</param>
    // /// <param name="value">default value</param>
    // public IndicatorParameterString CreateParameterString(string name, string value)
    // {
    //     IndicatorParameter newParameter = Parameters.Find(p => p.Name == name);
    //
    //     if (newParameter != null)
    //     {
    //         return (IndicatorParameterString)newParameter;
    //     }
    //
    //     newParameter = new IndicatorParameterString(name, value);
    //
    //     return (IndicatorParameterString)LoadParameterValues(newParameter);
    // }
    //
    // /// <summary>
    // /// create bool type parameter
    // /// </summary>
    // /// <param name="name">parameter name</param>
    // /// <param name="value">default value</param>
    // public IndicatorParameterBool CreateParameterBool(string name, bool value)
    // {
    //     IndicatorParameter newParameter = Parameters.Find(p => p.Name == name);
    //
    //     if (newParameter != null)
    //     {
    //         return (IndicatorParameterBool)newParameter;
    //     }
    //
    //     newParameter = new IndicatorParameterBool(name, value);
    //     return (IndicatorParameterBool)LoadParameterValues(newParameter);
    // }
}
