/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Net;

namespace OsEngine.Models.Market.Proxy;

public class ProxyOsa
{
    public WebProxy GetWebProxy() =>
        new($"{Ip}:{Port}")
        {
            Credentials = new NetworkCredential(Login, UserPassword)
        };

    public int Number;

    public bool IsOn = false;

    public string Ip;

    public int Port;

    public string Login;
    public string UserPassword;
    public string Location = "Unknown";
    public string AutoPingLastStatus = "Unknown";
    public string PingWebAddress = "http://ipinfo.io/";

    public int UseConnectionCount;

    public string GetStringToSave()
    {
        string result = IsOn + "%";
        result += Number + "%";
        result += Location + "%";
        result += Ip + "%";
        result += Port + "%";
        result += Login + "%";
        result += UserPassword + "%";
        result += AutoPingLastStatus + "%";
        result += PingWebAddress + "%";

        return result;
    }

    public void LoadFromString(string saveStr)
    {
        string[] _params = saveStr.Split("%");
        IsOn = Convert.ToBoolean(_params[0]);
        Number = Convert.ToInt32(_params[1]);
        Location = _params[2];
        Ip = _params[3];
        Port = int.Parse(_params[4]);
        Login = _params[5];
        UserPassword = _params[6];
        AutoPingLastStatus = _params[7];
        PingWebAddress = _params[8];
    }
}
