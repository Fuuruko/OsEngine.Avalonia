using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using OsEngine.Models.Data;
using OsEngine.Models.Logging;
using OsEngine.Models.Market.Servers;
using OsEngine.Views.Data;
using OsEngine.Views.Market.Servers;

namespace OsEngine.ViewModels.Data;

public partial class OsDataViewModel : BaseViewModel
{
    private static ObservableCollection<Source> _feedServers { get; } = GetFeedServers();
    public ObservableCollection<Source> FeedServers => _feedServers;

    public Logs Logs { get; }

    public ObservableCollection<Source> Sources { get; } = LoadSources();

    public ObservableCollection<SetViewModel> Sets { get; } = [];
    public Dictionary<Source, ServerParametersWindow> _openedSettingsWindows = [];
    private AddServersWindow _addServersWindow;
    private Window _dataLoaderWindow;

    public OsDataViewModel(Window window)
    {
        _dataLoaderWindow = window;
        Logs = new("DataLoader", Models.Entity.StartProgram.IsOsData);
    }

    private static ObservableCollection<Source> GetFeedServers()
    {
        Type baseServer = typeof(BaseServer);
        Type feedInterface = typeof(IFeedServer);
        List<Source> feedServers = [];

        var feedServerTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => baseServer.IsAssignableFrom(t)
                    && t.IsClass
                    && !t.IsAbstract
                    && feedInterface.IsAssignableFrom(t))
            .ToArray();

        foreach (var fst in feedServerTypes)
        {
            feedServers.Add(new()
                    {
                    // Name = (string)fst.GetProperty("Name").GetValue(null),
                    Name = fst.Name,
                    ServerType = fst,
                    });
            Console.WriteLine($"{fst.Name} {fst}");
        }

        return new (feedServers.OrderBy(s => s.Name));
    }

    private static ObservableCollection<Source> LoadSources()
    {
        return new ObservableCollection<Source>(BaseServer.Servers
                .Select(p => new Source(p.Value)));
    }

    public void AddNewSet()
    {
        new SetsWindow(this).Show();
    }

    public void AddSecurities()
    {
        new AddSecuritiesWindow().Show();
    }

    public void ShowFeedServers()
    {
        if (_addServersWindow == null)
        {
            _addServersWindow = new AddServersWindow(this);
            _addServersWindow.Show();
            _addServersWindow.Closing += (s, e) => _addServersWindow = null;
        }
        else
        {
            _addServersWindow.Activate();
        }
    }

    public void AddServer(Source source)
    {
        var serv = Sources.FirstOrDefault(s => s.ServerType == source.ServerType);
        if (serv == null || serv.Server.Permissions.SupportsMultipleServers)
        {
            // Console.WriteLine($"{serv.Name} {serv.Server.Permissions.SupportsMultipleServers}");
            var server = BaseServer.CreateServer(source.ServerType);
            Source s = new()
            {
                Name = source.Name,
                ServerType = source.ServerType,
                IsConnected = false,
                Server = server,
                // IsHideParameters = server is IHideParameters
            };
            Sources.Add(s);

            // if (typeof(IHideParameters).IsAssignableFrom(source.ServerType))
            // {
            //     return;
            // }
            // OpenServerSettings(s);

        }
    }

    public void OpenServerSettings(Source source)
    {
        if (_openedSettingsWindows.TryGetValue(source, out var existingWindow))
        {
            existingWindow.Activate();
        }
        else
        {
            // var servers = BaseServer.GetServers(source.ServerType);
            // var settings = new ServerParametersWindow(servers, 0);
            var settings = new ServerParametersWindow(source.Server.Inputs);
            _openedSettingsWindows[source] = settings;
            settings.Closed += (s, e) => _openedSettingsWindows.Remove(source);
            settings.Show();
        }
    }

    public async void DeleteServer(Source source)
    {
        bool confirmation = await MessageBox.ConfirmDialog($"Are you sure you want to delete server {source.Name}?", _dataLoaderWindow);
        if (confirmation)
        {
            BaseServer.DeleteServer(source.Server);
            Sources.Remove(source);
        }
    }
}
