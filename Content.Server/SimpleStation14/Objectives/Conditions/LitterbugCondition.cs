using Content.Server.Objectives.Interfaces;
using JetBrains.Annotations;
using Robust.Shared.Utility;

namespace Content.Server.Objectives.Conditions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed class LitterbugCondition : GenericFreewillCondition
    {
        public override string Title => Loc.GetString("objective-condition-litterbug-title");

        public override string Description => Loc.GetString("objective-condition-litterbug-description");

        public override SpriteSpecifier Icon =>
            new SpriteSpecifier.Rsi(new ResourcePath("Clothing/Hands/Gloves/fingerless.rsi"), "icon");
    }
}
