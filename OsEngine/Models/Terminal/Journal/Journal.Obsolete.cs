using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Terminal;

public partial class Journal
{
    [Obsolete(nameof(_journals))]
    public static List<Journal> ControllersToCheck => _journals;

    // NOTE: Its exactly allpositions
    [Obsolete(nameof(AllPositions))]
    private ObservableCollection<Position> _deals { get; set; } = [];

    [Obsolete(nameof(AllPositions))]
    public ObservableCollection<Position> AllPosition { get => AllPositions; }

    [Obsolete(nameof(OpenLongPositions))]
    public ObservableCollection<Position> OpenAllLongPositions { get => OpenLongPositions; }
    [Obsolete(nameof(OpenShortPositions))]
    public ObservableCollection<Position> OpenAllShortPositions { get => OpenShortPositions; }

    [Obsolete(nameof(ClosedPositions))]
    public ObservableCollection<Position> CloseAllPositions => ClosedPositions;
    [Obsolete(nameof(ClosedShortPositions))]
    public List<Position> CloseAllShortPositions => ClosedShortPositions;
    [Obsolete(nameof(ClosedLongPositions))]
    public List<Position> CloseAllLongPositions => ClosedLongPositions;

    [Obsolete]
    public event Action<Position> PositionStateChangeEvent;

    [Obsolete]
    public event Action<Position> PositionNetVolumeChangeEvent;

    [Obsolete]
    public event Action<Position, SignalType> UserSelectActionEvent;
}
