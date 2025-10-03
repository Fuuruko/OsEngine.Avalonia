using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Market.Servers;

internal interface IStaticServer
{
    static abstract string ServerName { get; }
    static abstract ServerPermissions Permissions { get; }
    // Permissions => ((IStaticServer)this).Permissions
}

public partial class BaseServer
{
    public static bool IsAutoConnect { get; set; } = false;

    public static Dictionary<Guid, BaseServer> Servers { get; private set; } = LoadServers();

    public static BaseServer CreateServer(Type type)
    {
        BaseServer server = (BaseServer)Activator.CreateInstance(type);
        Servers.Add(server.GUID, server);
        SaveServers();
        return server;
    }

    public static void DeleteServer(BaseServer server)
    {
        // TODO: Disconnect and dispose before delete
        Servers.Remove(server.GUID);
        server.Dispose();
        SaveServers();
    }

    private static Dictionary<Guid, BaseServer> LoadServers()
    {
        if (!File.Exists($"Engine{Path.DirectorySeparatorChar}Servers.json"))
        {
            SaveServers();
            return [];
        }

        try
        {
            string json = File.ReadAllText($"Engine{Path.DirectorySeparatorChar}Servers.json");

            ServersSettings settings = JsonConvert.DeserializeObject<ServersSettings>(json);


            IsAutoConnect = settings.IsAutoConnect;

            Dictionary<Guid, BaseServer> servers = [];
            foreach (var (guid, serverSettings) in settings.Servers)
            {
                BaseServer server = (BaseServer)Activator
                    .CreateInstance(serverSettings.Type);

                foreach (var i in server.Inputs)
                {
                    Console.WriteLine(i.Name);
                }
                Console.WriteLine();
                server.GUID = guid;
                // server.Name = serverSettings.Name;
                foreach (var i in server.Inputs)
                {
                    Console.WriteLine(i.Name);
                }
                Dictionary<string, IBaseInput> inputs = server.Inputs
                    .ToDictionary(p => p.Name, p => p);
                foreach (var input in serverSettings.Inputs)
                {
                    try
                    {
                        inputs[input.Name].Value = input.Value;
                    }
                    catch
                    {
                        // MessageBox.Show($"Input {input.Name} has been deleted.");
                    }
                }
                servers[guid] = server;
            }

            return servers;
        }
        catch (Exception error)
        {
            MessageBox.Show(error.ToString());
            return [];
        }
    }

    private void SetupInputs()
    {
        Type baseInput = typeof(IBaseInput);
        Type derivedType = GetType();
        Type baseType = typeof(BaseServer);

        Inputs = [
            _namePostfix,
            _isKeepTrades,
            _uploadTradesDaysNumber,
            _isKeepCandles,
            _keepCandlesNumber,
            _needToLoadBidAskInTrades2,
            _isClearTrades,
            _isClearCandles,
            _isUpdateOnlyNewPriceTrades,
            _needToUseFullMarketDepth2,
        ];

        if (this is IProxySupport)
        {
            // Inputs.Add(new Input.String());
            // Inputs.Add();
        }

        Console.WriteLine(derivedType);

        IBaseInput[] derivedInputs = derivedType.GetFields(BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Instance)
            .Where(f => f.FieldType.IsAssignableTo(typeof(IBaseInput)))
            .Select(f => (IBaseInput)f.GetValue(this))
            .ToArray();


        // FieldInfo[] baseFields = baseType.GetFields(BindingFlags.Public
        //         | BindingFlags.NonPublic
        //         | BindingFlags.Instance)
        //     .Where(f => f.FieldType.IsAssignableTo(baseInput))
        //     .ToArray();

        // Console.WriteLine("baseFields");
        // foreach (var df in baseFields)
        // {
        //     Console.WriteLine(df);
        // }

        Console.WriteLine("derivedFields");
        foreach (var df in derivedInputs)
        {
            Console.WriteLine(df);
            // Console.WriteLine(baseFields.Contains(df));
        }

        // var uniqueFields = derivedFields.Where(d =>
        //         !baseFields.Any(b =>
        //             !b.FieldType.IsAssignableTo(typeof(IBaseInput))
        //             && b.Name == d.Name
        //             && b.FieldType == d.FieldType))
        //     .ToList();

        // Console.WriteLine("uniqueFields");
        // foreach (var df in uniqueFields)
        // {
        //     Console.WriteLine(df);
        // }

        // IBaseInput[] inputs = uniqueFields
        //     .Select(f => (IBaseInput)f.GetValue(this))
        //     .ToArray();

        Inputs.InsertRange(0, derivedInputs);
    }

    public static void SaveServers()
    {
        Dictionary<Guid, ServerSettings> servers = [];
        foreach (var s in Servers ?? [])
        {
            servers.Add(s.Key, new(s.Value));
        }
        ServersSettings settings = new(IsAutoConnect, servers);

        var jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            // TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        };
        string json = JsonConvert.SerializeObject(settings, jsonSettings);

        File.WriteAllText($"Engine{Path.DirectorySeparatorChar}Servers.json", json);
    }
}

file struct ServersSettings(bool isAutoConnect, Dictionary<Guid, ServerSettings> servers)
{
    public bool IsAutoConnect = isAutoConnect;
    public Dictionary<Guid, ServerSettings> Servers = servers;
}

file struct ServerSettings(BaseServer server)
{
    public Type Type = server.GetType();
    public InputDTO[] Inputs = [.. server.Inputs.Select(i => new InputDTO(i))];
}

file struct InputDTO(IBaseInput input)
{
    public string Name = input.Name;
    public object Value = input.Value;
}
