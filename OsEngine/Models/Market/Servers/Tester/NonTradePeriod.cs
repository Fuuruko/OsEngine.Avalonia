/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
 */

using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
// using OsEngine.Market.Connectors;
// using OsEngine.OsTrader.Panels;
// using OsEngine.OsTrader.Panels.Tab;

namespace OsEngine.Models.Market.Servers.Tester;

/// <summary>
/// Period with NO new positions and NO new open orders in tester
/// </summary>
public partial class NonTradePeriod : ObservableObject
{
    [ObservableProperty]
    private DateTime dateStart;

    [ObservableProperty]
    private DateTime dateEnd;

    [ObservableProperty]
    public bool isOn;

    public string GetSaveString()
    {
        string result = "";

        result += DateStart.ToString(CultureInfo.InvariantCulture) + "$";
        result += DateEnd.ToString(CultureInfo.InvariantCulture) + "$";
        result += IsOn;

        return result;
    }

    public void SetFromString(string str)
    {
        string[] strings = str.Split('$');

        DateStart = Convert.ToDateTime(strings[0], CultureInfo.InvariantCulture);
        DateEnd = Convert.ToDateTime(strings[1], CultureInfo.InvariantCulture);
        IsOn = Convert.ToBoolean(strings[2]);
    }
}


