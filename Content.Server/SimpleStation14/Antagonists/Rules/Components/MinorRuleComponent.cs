using Content.Server.SimpleStation14.Minor;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.SimpleStation14.Antagonists.Rules.Components;

[RegisterComponent, Access(typeof(MinorRuleSystem))]
public sealed class MinorRuleComponent : Component
{
    public readonly SoundSpecifier AddedSound = new SoundPathSpecifier("/Audio/Misc/tatoralert.ogg");
    public List<MinorRole> Minors = new();

    [DataField("minorPrototypeId", customTypeSerializer: typeof(PrototypeIdSerializer<AntagPrototype>))]
    public string MinorPrototypeId = "Minor";

    public int TotalMinors => Minors.Count;
    public string[] Codewords = new string[3];

    public enum SelectionState
    {
        WaitingForSpawn = 0,
        ReadyToSelect = 1,
        SelectionMade = 2,
    }

    public SelectionState SelectionStatus = SelectionState.WaitingForSpawn;
    public TimeSpan AnnounceAt = TimeSpan.Zero;
    public Dictionary<IPlayerSession, HumanoidCharacterProfile> StartCandidates = new();
}
