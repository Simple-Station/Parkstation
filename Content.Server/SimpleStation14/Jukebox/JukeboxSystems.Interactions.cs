using Content.Server.Power.Components;
using Content.Shared.SimpleStation14.Jukebox;
using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Jukebox;

public sealed partial class JukeboxSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <summary>
    ///     Checks if the Jukebox can play songs when its power state changes.
    /// </summary>
    private void OnPowerChanged(EntityUid uid, JukeboxComponent component, ref PowerChangedEvent args)
    {
        CheckCanPlay(uid, component);
    }

    /// <summary>
    ///     Handles the Jukebox's play button being pressed to toggle between playing and paused.
    /// </summary>
    private void OnPlayButtonPressed(EntityUid jukeBox, JukeboxComponent jukeboxComp, JukeboxPlayButtonPressedMessage msg)
    {
        if (jukeboxComp.Paused)
        {
            UnPause(jukeBox, jukeboxComp);
        }
        else
        {
            Pause(jukeBox, jukeboxComp);
        }
    }

    /// <summary>
    ///     Handles the Jukebox's skip button being pressed to skip the current song.
    /// </summary>
    private void OnSkipButtonPressed(EntityUid jukeBox, JukeboxComponent jukeboxComp, JukeboxSkipButtonPressedMessage msg)
    {
        Skip(jukeBox, jukeboxComp);
    }

    /// <summary>
    ///     Handles a song being selected in the Jukebox's ui.
    /// </summary>
    private void OnSongSelected(EntityUid jukeBox, JukeboxComponent jukeboxComp, JukeboxSongSelectedMessage msg)
    {
        TryQueueSong(jukeBox, jukeboxComp, msg.Song);
    }

    /// <summary>
    ///     Generates a valid serial number for the jukeboxes. This is just goofy.
    /// </summary>
    public string GenerateSerialNumber()
    {
        var digits = new int[8];

        digits[0] = _random.Next(1, 10);
        digits[1] = _random.Next(10);
        digits[2] = digits[1];
        digits[3] = digits[0];
        digits[4] = (digits[0] + digits[1]) % 10;
        digits[5] = digits[2] - digits[3];
        digits[6] = digits[4] * digits[5];
        digits[7] = (digits[0] + digits[1] + digits[2] + digits[3]) % 5;

        var serial = $"{digits[0]}{digits[1]}{digits[2]}{digits[3]}{digits[4]}{digits[5]}{digits[6]}";

        var letter = digits[7] == 0 ? 90 : digits[7] % 2 == 0 ? (digits[7] + 65) : (90 - digits[7]);

        return $"{serial}{(char) letter}";
    }
}
