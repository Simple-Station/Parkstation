using Content.Server.MachineLinking.Events;
using Content.Server.MachineLinking.System;
using Content.Server.Power.Components;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Containers;
using Content.Server.Sound.Components;
using Content.Shared.Sound.Components;
using Content.Shared.Interaction;
using Robust.Shared.Timing;

namespace Content.Server.SimpleStation14.LoudSpeakers;

public sealed class DoorSignalControlSystem : EntitySystem
{
    [Dependency] private readonly SignalLinkerSystem _signal = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LoudSpeakerComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<LoudSpeakerComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<LoudSpeakerComponent, SignalReceivedEvent>(OnSignalReceived);

        SubscribeLocalEvent<LoudSpeakerComponent, InteractHandEvent>(OnInteractHand);
    }

    private void OnShutdown(EntityUid uid, LoudSpeakerComponent component, ComponentShutdown args)
    {
        if (component.CurrentPlayingSound != null)
            component.CurrentPlayingSound.Stop();
    }

    private void OnInit(EntityUid uid, LoudSpeakerComponent component, ComponentInit args)
    {
        if (component.Ports)
            _signal.EnsureReceiverPorts(uid, component.PlaySoundPort);
    }

    /// <summary>
    ///     Tries to play a loudspeaker.
    /// </summary>
    /// <param name="uid">The Loudspeaker to play.</param>
    /// <param name="component">The Loudspeaker component.</param>
    /// <returns>True if the Loudspeaker was played, false otherwise.</returns>
    public bool TryPlayLoudSpeaker(EntityUid uid, LoudSpeakerComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (component.NextPlayTime > _timing.CurTime)
            return false;

        if (TryComp<ApcPowerReceiverComponent>(uid, out var powerComp) && !powerComp.Powered)
            return false;

        PlayLoudSpeaker(uid, component, GetSpeakerSound(uid, component));

        return true;
    }

    private void PlayLoudSpeaker(EntityUid uid, LoudSpeakerComponent component, SoundSpecifier sound)
    {
        var newParams = sound.Params
            .WithVolume(sound.Params.Volume * component.VolumeMod)
            .WithMaxDistance(sound.Params.MaxDistance * component.RangeMod)
            .WithRolloffFactor(sound.Params.RolloffFactor * component.RolloffMod)
            .WithVariation((sound.Params.Variation !> 0 ? component.DefaultVariance : sound.Params.Variation) * component.VarianceMod);

        if (component.Interrupt && component.CurrentPlayingSound != null)
            component.CurrentPlayingSound.Stop();

        component.NextPlayTime = _timing.CurTime + component.Cooldown;

        component.CurrentPlayingSound = _audio.Play(sound, Filter.Pvs(uid, component.RangeMod), uid, true, newParams);
    }

    private SoundSpecifier GetSpeakerSound(EntityUid uid, LoudSpeakerComponent component)
    {
        if (!TryComp<ContainerManagerComponent>(uid, out var containerManager) ||
            !containerManager.TryGetContainer(component.ContainerSlot, out var container))
            return component.DefaultSound;

        if (container.ContainedEntities.Count == 0)
            return component.DefaultSound;

        var entity = container.ContainedEntities[0];

        switch (entity)
        {
            case { } when TryComp<EmitSoundOnTriggerComponent>(entity, out var trigger) && trigger.Sound != null:
                return trigger.Sound;

            case { } when TryComp<EmitSoundOnActivateComponent>(entity, out var activate) && activate.Sound != null:
                return activate.Sound;

            case { } when TryComp<EmitSoundOnUseComponent>(entity, out var use) && use.Sound != null:
                return use.Sound;

            case { } when TryComp<EmitSoundOnDropComponent>(entity, out var drop) && drop.Sound != null:
                return drop.Sound;

            case { } when TryComp<EmitSoundOnLandComponent>(entity, out var land) && land.Sound != null:
                return land.Sound;

            default:
                return component.DefaultSound;
        }
    }

    private void OnSignalReceived(EntityUid uid, LoudSpeakerComponent component, SignalReceivedEvent args)
    {
        if (args.Port == component.PlaySoundPort)
        {
            TryPlayLoudSpeaker(uid, component);
        }
    }

    private void OnInteractHand(EntityUid uid, LoudSpeakerComponent component, InteractHandEvent args)
    {
        if (!component.TriggerOnInteract)
            return;

        if (!TryPlayLoudSpeaker(uid, component))
            return;

        args.Handled = true;
    }
}
