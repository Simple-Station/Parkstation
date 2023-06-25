using Robust.Shared.Audio;

namespace Content.Shared.SimpleStation14.EndOfRoundStats.EmitSound; // Sound emitters are shared.

public sealed class EmitSoundStatEvent : EntityEventArgs
{
    public EntityUid Emitter;
    public SoundSpecifier Sound;

    public EmitSoundStatEvent(EntityUid emitter, SoundSpecifier sound)
    {
        Emitter = emitter;
        Sound = sound;
    }
}
