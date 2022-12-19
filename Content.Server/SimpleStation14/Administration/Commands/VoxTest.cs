using Content.Server.Mind.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class VoxTest : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entities = default!;

        public string Command => "voxTest";
        public string Description => "ASS BLAST USA!!";
        public string Help => "voxTest";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
        var _polymorphableSystem = _entities.EntitySysManager.GetEntitySystem<PolymorphableSystem>();

            foreach (var comp in _entities.EntityQuery<MindComponent>())
            {
                SoundSystem.Play("/Audio/SimpleStation14/Admin/Commands/voxtest.ogg", Filter.Entities(comp.Owner), comp.Owner);
            }
        }
    }
}
