using Content.Server.Objectives.Interfaces;
using JetBrains.Annotations;
using Robust.Shared.Utility;

namespace Content.Server.Objectives.Conditions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed class PickpocketCondition : GenericFreewillCondition
    {
        public override string Title => Loc.GetString("objective-condition-pickpocket-title");

        public override string Description => Loc.GetString("objective-condition-pickpocket-description");

        public override SpriteSpecifier Icon =>
            new SpriteSpecifier.Rsi(new ResourcePath("Clothing/Hands/Gloves/fingerless.rsi"), "icon");
    }
}
