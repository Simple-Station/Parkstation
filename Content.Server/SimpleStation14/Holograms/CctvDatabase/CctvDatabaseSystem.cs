//TODO: HOLO In the future the entire CCTV Database system should be completely replaced
// with something that uses station records, requires tracked camera time,
// and a whole bunch of other stuff that'll be fun to use.
// For the time being, this works.

using Content.Server.GameTicking;
using Content.Server.Mind.Components;
using Content.Server.SimpleStation14.Holograms.Components;
using Content.Server.Station.Systems;
using Content.Shared.SimpleStation14.Holograms;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server.SimpleStation14.Holograms.CctvDatabase;

public sealed class CctvDatabaseSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private const string HoloDiskPrototype = "HologramDisk";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HologramTargetComponent, PlayerSpawnCompleteEvent>(OnPlayerSpawn);

        SubscribeLocalEvent<CctvDatabaseConsoleComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<CctvDatabaseConsoleComponent, CctvDatabasePrintRequestMessage>(OnPrintRequest);
    }

    public override void Update(float delta)
    {
        while (EntityQueryEnumerator<CctvDatabaseConsoleActiveComponent>().MoveNext(out var console, out var activeComp))
        {
            if (activeComp.PrintTime <= _timing.CurTime)
                FinishPrint(console, activeComp);
        }
    }

    private void OnPlayerSpawn(EntityUid player, HologramTargetComponent holoTargetComp, PlayerSpawnCompleteEvent args)
    {
        if (!TryComp<StationCctvDatabaseComponent>(args.Station, out var stationDatabaseComp))
            return;
        if (!TryComp<MindContainerComponent>(player, out var mindContainerComp) || mindContainerComp.Mind is not { } mind)
            return;

        stationDatabaseComp.PotentialsList.Add(mind);

        while (EntityQueryEnumerator<CctvDatabaseConsoleComponent>().MoveNext(out var console, out _))
            UpdateUserInterface(console);
    }

    private void OnUIOpened(EntityUid uid, CctvDatabaseConsoleComponent component, BoundUIOpenedEvent args)
    {
        if (args.UiKey is not CctvDatabaseUiKey.Key)
            return;

        UpdateUserInterface(uid);
    }

    private void UpdateUserInterface(EntityUid uid, CctvDatabaseConsoleActiveComponent? activeComp = null)
    {
        if (!_ui.TryGetUi(uid, CctvDatabaseUiKey.Key, out var bui))
            return;

        if (_station.GetOwningStation(uid) is not { } station || !TryComp<StationCctvDatabaseComponent>(station, out var stationDatabaseComp))
            return;

        TimeSpan? finishTime = null;

        if (Resolve(uid, ref activeComp, false))
            finishTime = activeComp.PrintTime;

        _ui.TrySetUiState(uid, CctvDatabaseUiKey.Key, new CctvDatabaseState(stationDatabaseComp.PotentialsList.ConvertAll(x => x.CharacterName ?? "Unknown"), finishTime));
    }

    private void OnPrintRequest(EntityUid console, CctvDatabaseConsoleComponent consoleComp, CctvDatabasePrintRequestMessage args)
    {
        if (HasComp<CctvDatabaseConsoleActiveComponent>(console))
            return;

        if (_station.GetOwningStation(console) is not { } station || !TryComp<StationCctvDatabaseComponent>(station, out var stationDatabaseComp))
            return;

        if (stationDatabaseComp.PotentialsList.Count <= args.Index) // Should never happen.
        {
            Log.Error($"CCTV Database console {console} tried to print a disk with index {args.Index} but the list only has {stationDatabaseComp.PotentialsList.Count} entries.");
            return;
        }

        var mind = stationDatabaseComp.PotentialsList[args.Index];
        var activeComp = AddComp<CctvDatabaseConsoleActiveComponent>(console);
        activeComp.PrintingMind = mind;
        activeComp.PrintTime = consoleComp.TimeToPrint + _timing.CurTime;
        UpdateUserInterface(console, activeComp);
    }

    private void FinishPrint(EntityUid console, CctvDatabaseConsoleActiveComponent activeComp)
    {
        var disk = Spawn(HoloDiskPrototype, Transform(console).Coordinates);
        var diskComp = EnsureComp<HologramDiskComponent>(disk);
        diskComp.HoloMind = activeComp.PrintingMind;
        RemComp<CctvDatabaseConsoleActiveComponent>(console);
        UpdateUserInterface(console);
    }
}
