using System;
using OsEngine.Models.Market.Servers;

namespace OsEngine.Models.Data;

public class Source
{
    public Source() {  }

    public Source(BaseServer server)
    {
        Name = server.ServerNameUnique;
        IsConnected = false;
        Server = server;
        ServerType = server.GetType();
    }

    public string Name { get; set; }
    public bool IsConnected { get; set; }
    public string Status { get; set; }
    public BaseServer Server { get; set; }
    public Type ServerType { get; set; }

    public bool IsHideParameters { get; set; }
}
