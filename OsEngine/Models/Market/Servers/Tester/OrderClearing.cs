/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
 */

using System;
using System.Globalization;
// using OsEngine.Market.Connectors;
// using OsEngine.OsTrader.Panels;
// using OsEngine.OsTrader.Panels.Tab;

namespace OsEngine.Models.Market.Servers.Tester;

/// <summary>
/// Clearing for limit orders
/// </summary>
public class OrderClearing
{
    public TimeSpan Time { get; set; }

    public bool IsOn { get; set; }

    public string GetSaveString()
    {
        string result = "";

        result += Time.ToString() + "$";
        result += IsOn;

        return result;
    }

    public void SetFromString(string str)
    {
        string[] strings = str.Split('$');

        Time = TimeSpan.Parse(strings[0]);
        IsOn = Convert.ToBoolean(strings[1]);
    }
}


