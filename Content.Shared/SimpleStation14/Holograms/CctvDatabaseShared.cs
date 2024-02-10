using Content.Shared.CrewManifest;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Holograms;

[Serializable, NetSerializable]
public enum CctvDatabaseUiKey
{
    Key
}

[Serializable, NetSerializable]
public sealed class CctvDatabaseState : BoundUserInterfaceState
{
    public List<string> CrewManifest;
    public TimeSpan? FinishedPrintingTime;

    public CctvDatabaseState(List<string> crewManifest, TimeSpan? finishedPrintingTime = null)
    {
        CrewManifest = crewManifest;
        FinishedPrintingTime = finishedPrintingTime;
    }
}

[Serializable, NetSerializable]
public sealed class CctvDatabasePrintRequestMessage : BoundUserInterfaceMessage
{
    public int Index;

    public CctvDatabasePrintRequestMessage(int index)
    {
        Index = index;
    }
}
