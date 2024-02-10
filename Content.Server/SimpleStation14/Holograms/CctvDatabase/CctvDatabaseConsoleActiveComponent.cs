namespace Content.Server.SimpleStation14.Holograms.CctvDatabase;

/// <summary>
///     Marks this entity as an active <see cref="CctvDatabaseConsoleComponent"/>.
/// </summary>
[RegisterComponent]
public sealed class CctvDatabaseConsoleActiveComponent : Component
{
    /// <summary>
    ///     The mind currently being printed.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public Mind.Mind? PrintingMind;

    /// <summary>
    ///     The time the mind will be printed at.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan PrintTime = TimeSpan.Zero;
}
