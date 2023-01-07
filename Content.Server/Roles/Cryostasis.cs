using System.Globalization;
using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Players;
using Content.Server.Roles;
using Content.Server.Station.Systems;
using Content.Shared.Administration;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Console;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Physics;
using Content.Shared.Inventory;

namespace Content.Server.Administration.Commands.Cryostasis
{
    [AnyCommand]
    public sealed class CryostasisCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entities = default!;
        [Dependency] private readonly IEntitySystemManager _entitysys = default!;
        // [Dependency] private readonly ChatSystem _chatSystem = default!;

        public string Command => "cryostasis";
        public string Description => "Deletes you and opens up a new job slot. Do this in a secure area or put your belongings in a secure area. MISUSE WILL BE MODERATED";
        public string Help => $"Usage: {Command}";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player as IPlayerSession;
            if (player == null)
            {
                shell.WriteLine("You aren't a player.");
                return;
            }

            var mind = player.ContentData()?.Mind;
            if (mind == null)
            {
                shell.WriteLine("You can't do this without a mind.");
                return;
            }

            if (!mind.HasRole<Job>())
            {
                shell.WriteLine("You do not have a job, you are not accessible by nanotrasen, therefore unable to cryo.");
                return;
            }

            if (mind.CurrentEntity == null)
            {
                shell.WriteLine("You do not have an entity?");
                return;
            }

            var uid = (EntityUid) mind.CurrentEntity;
            Role? job = null;
            JobPrototype? jobprotot = null;
            var _prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            var _stationJobs = EntitySystem.Get<StationJobsSystem>();
            var isantag = false;

            foreach (var role in mind.AllRoles)
            {
                if (role.Antagonist == true)
                {
                    isantag = true;
                    continue;
                }
                if (job != null) continue;

                job = role;
                mind.RemoveRole(role);
            }

            if (job == null)
            {
                shell.WriteLine("You do not have a job, you are not accessible by nanotrasen, therefore unable to cryo.");
                return;
            }

            if (isantag == true)
            {
                shell.WriteLine("You are an antagonist, the Syndicate would not like this, and Centcom will get too easy of a resolution. This would be inappropriate.");
                return;
            }

            foreach (var jobproto in _prototypeManager.EnumeratePrototypes(typeof(JobPrototype)))
            {
                if (typeof(JobPrototype) != typeof(JobPrototype)) continue;

                var jobprotoo = (JobPrototype) jobproto;
                if (Loc.GetString(jobprotoo.Name).ToLower() == job.Name.ToLower()) jobprotot = jobprotoo;
            }

            if (jobprotot == null)
            {
                shell.WriteLine("You do not have a job, you are not accessible by nanotrasen, therefore unable to cryo.");
                return;
            }

            _entities.TryGetComponent<InventoryComponent>(uid, out var inventoryComponent);
            var invSystem = _entities.System<InventorySystem>();
            if (invSystem.TryGetSlots(uid, out var slotDefinitions, inventoryComponent))
            {
                foreach (var slot in slotDefinitions)
                {
                    invSystem.TryUnequip(uid, slot.Name, true, true, false, inventoryComponent);
                }
            }

            if (!EntitySystem.Get<GameTicker>().OnGhostAttempt(mind, false))
            {
                shell.WriteLine("You can't ghost right now.");
                return;
            }

            SoundSystem.Play("/Audio/Effects/Lightning/lightningbolt.ogg", Filter.Pvs(uid), uid);

            EntityUid? station = null;
            // TODO: when multiple stations ever get loaded regularly at the same time with jobs, do something about this var maybe?
            station = EntitySystem.Get<StationSystem>().Stations.ToList()[0];

            if (station != null)
                EntitySystem.Get<ChatSystem>().DispatchStationAnnouncement((EntityUid) station,
                    Loc.GetString("cryo-departure-announcement",
                    ("character", _entities.GetComponent<MetaDataComponent>(uid).EntityName),
                    ("job", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(jobprotot.Name))
                    ), Loc.GetString("latejoin-arrival-sender"),
                    playDefaultSound: false);
            else shell.WriteLine("No station found, leave announcement will not be sent.");

            // TODO: put it in a box? (the items)
            _entities.QueueDeleteEntity(uid);
        }
    }
}
