using Content.Server.SimpleStation14.Speech.Components;
using Content.Server.Speech;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Server.SimpleStation14.Speech.EntitySystems;

public sealed class VoiceOfGodSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoiceOfGodComponent, AccentGetEvent>(OnAccent);
    }

    public string Accentuate(VoiceOfGodComponent component, string message)
    {
        if (component.Sound != "none")
        {
            SoundSystem.Play(component.Sound,
                Filter.Pvs(component.Owner),
                component.Owner,
                new AudioParams()
                {
                    Volume = component.Volume
                }
            );
        }

        return component.Accent ? message.ToUpper() : message;
    }

    private void OnAccent(EntityUid uid, VoiceOfGodComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(component, args.Message);
    }
}
