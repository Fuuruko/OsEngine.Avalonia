using System.Collections.Generic;
using OsEngine.Models.Entity.Server;

namespace OsEngine.Models.Market.Servers;

public partial class BaseServer
{
    /// <summary>
    /// server parameters
    /// </summary>
    public List<IServerParameter> ServerParameters = [];

    /// <summary>
    /// show settings window
    /// </summary>
    public void ShowDialog(int num = 0)
    {
    }
}
