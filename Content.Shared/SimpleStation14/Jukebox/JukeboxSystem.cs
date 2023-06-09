using System.Collections.Generic;
using Content.Shared.SimpleStation14.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared.SimpleStation14.Jukebox;

[Serializable, NetSerializable]
public enum JukeboxUiKey : byte
{
    Key
}

/// <summary>
///     Sent from server to client to tell the client to update the Jukebox UI based on the <see cref="JukeboxComponent"/>.
/// </summary>
/// <remarks>
///     This is used instead of <see cref="BoundUserInterfaceState"/> because the component is already networked, and contains all the data needed.
/// </remarks>
[Serializable, NetSerializable]
public sealed class JukeboxUpdateStateMessage : BoundUserInterfaceMessage
{
    public bool PopulateSongs { get; }

    public JukeboxUpdateStateMessage(bool populateSongs = false)
    {
        PopulateSongs = populateSongs;
    }
}

/// <summary>
///     Sent from client to server when the play button is pressed.
/// </summary>
[Serializable, NetSerializable]
public sealed class JukeboxPlayButtonPressedMessage : BoundUserInterfaceMessage
{
}

/// <summary>
///     Sent from client to server when the skip button is pressed.
/// </summary>
[Serializable, NetSerializable]
public sealed class JukeboxSkipButtonPressedMessage : BoundUserInterfaceMessage
{
}

/// <summary>
///     Sent from client to server when a song is selected, containing the selected song's ID.
/// </summary>
[Serializable, NetSerializable]
public sealed class JukeboxSongSelectedMessage : BoundUserInterfaceMessage
{
    public string Song { get; }

    public JukeboxSongSelectedMessage(string song)
    {
        Song = song;
    }
}
