using System;
using System.Collections.Generic;
using System.IO;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;

namespace OsEngine.Models.Market.Servers;

public partial class BaseServer
{
    private List<Security> LoadSavedSecurities()
    {
        List<Security> securities = [];

        try
        {
            if (Directory.Exists(@"Engine\ServerDopSettings") == false)
            {
                return securities;
            }

            if (Directory.Exists($@"Engine\ServerDopSettings\{ServerType}") == false)
            {
                return securities;
            }

            string[] paths = Directory.GetFiles(@"Engine\ServerDopSettings\" + ServerType);

            for (int i = 0; paths != null && i < paths.Length; i++)
            {
                string curPath = paths[i];

                using StreamReader reader = new(curPath);
                string secInStr = reader.ReadToEnd();

                Security newSecurity = new();
                newSecurity.LoadFromString(secInStr);
                securities.Add(newSecurity);
            }

            return securities;
        }
        catch (Exception ex)
        {
            OnLogRecieved(ex.ToString(), LogMessageType.Error);
            return securities;
        }
    }

    public void Delete()
    {
        $@"Engine\{ServerNameUnique}Params.txt".TryDelete();
        $@"Engine\{ServerNameUnique}ServerSettings.txt".TryDelete();
        try
        {
            ServerRealization.Dispose();
        }
        catch
        {
            // ignore
        }
    }
}
