using Content.Shared.Preferences;

namespace Content.Server.SimpleStation14.Traits.Events
{
    public sealed class BeingClonedEvent : CancellableEntityEventArgs
    {
        public HumanoidCharacterProfile Profile { get; set; }
        public Mind.Mind Mind { get; set; }
        public EntityUid Cloner { get; set; }

        public BeingClonedEvent(HumanoidCharacterProfile profile, Mind.Mind mind, EntityUid cloner)
        {
            Profile = profile;
            Mind = mind;
            Cloner = cloner;
        }
    }

    public sealed class BeenClonedEvent : EntityEventArgs
    {
        public HumanoidCharacterProfile Profile { get; set; }
        public Mind.Mind Mind { get; set; }
        public EntityUid Mob { get; set; }
        public EntityUid Cloner { get; set; }

        public BeenClonedEvent(HumanoidCharacterProfile profile, Mind.Mind mind, EntityUid mob, EntityUid cloner)
        {
            Profile = profile;
            Mind = mind;
            Mob = mob;
            Cloner = cloner;
        }
    }
}
