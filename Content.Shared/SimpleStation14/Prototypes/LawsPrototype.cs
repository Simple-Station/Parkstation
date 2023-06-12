using Robust.Shared.Prototypes;

namespace Content.Shared.SimpleStation14.Prototypes;

/// <summary>
///     Defines a set of laws to be used by a Silicon. Simply a list of strings.
/// </summary>
[Prototype("laws")]

public sealed class LawsPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField("laws")]
    public List<string> Laws = new();
}
