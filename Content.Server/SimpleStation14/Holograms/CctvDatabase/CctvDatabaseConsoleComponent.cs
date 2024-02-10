namespace Content.Server.SimpleStation14.Holograms.CctvDatabase;

/// <summary>
///     Marks this entity as a CCTV Database console, allowing it to print CCTV footage onto disks.
/// </summary>
/// <remarks>
///     Mostly a temporary thing for the Hologram system, should be expanded when recordings are actually a thing?
/// </remarks>
[RegisterComponent]
public sealed class CctvDatabaseConsoleComponent : Component
{
    /// <summary>
    ///     The amount of time it takes this Console to print a disk.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan TimeToPrint = TimeSpan.FromMinutes(1.5);
}
