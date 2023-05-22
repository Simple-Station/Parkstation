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
using Content.Shared.Inventory;
using Content.Server.Administration.Logs;
using Content.Shared.Database;

namespace Content.Server.Administration.Commands.Cryostasis
{
    [AnyCommand]
    public sealed class CryostasisCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entities = default!;
        [Dependency] private readonly IEntitySystemManager _entitysys = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public string Command => "cryostasis";
        public string Description => "Deletes you and opens up a new job slot. Do this in a secure area or put your belongings in a secure area.";
        public string Help => $"Usage: {Command} <force>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player as IPlayerSession;
            // No player (probably the server console).
            if (player == null)
            {
                shell.WriteLine("You aren't a player.");
                return;
            }

            var mind = player.ContentData()?.Mind;
            // No mind (we need one of those..).
            if (mind == null)
            {
                shell.WriteLine("You can't do this without a mind.");
                return;
            }

            if (mind.IsVisitingEntity)
            {
                shell.WriteLine("You cannot do this while visiting something.");
                return;
            }

            var force = false;
            if (args.Length >= 1)
            {
                if (args[0].ToLower() == "true")
                {
                    force = true;
                }
            }

            // No job (unemployed people are not allowed to go into cryostasis).
            if (!mind.HasRole<Job>() && !force)
            {
                shell.WriteLine("You do not have a job, you are not accessible by Nanotrasen, therefore unable to cryo.");
                return;
            }

            // No entity somehow???
            if (mind.CurrentEntity == null)
            {
                shell.WriteLine("You do not have an attached entity.");
                return;
            }

            // Various definitions
            var uid = (EntityUid) mind.CurrentEntity;
            Role? job = null;
            JobPrototype? jobprotot = null;
            var _prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            var _stationJobs = EntitySystem.Get<StationJobsSystem>();
            var isantag = false;

            // Check all roles
            foreach (var role in mind.AllRoles)
            {
                // Can't do this if antag, set a variable for later use.
                if (role.Antagonist == true)
                {
                    isantag = true;
                    break;
                }
                // A job has been found, stop looking.
                if (job != null) break;

                // A job has been found, remove it from the mind (you are passing this job onto a latejoiner).
                job = role;
                mind.RemoveRole(role);
            }

            // No job, you aren't a Nanotrasen employee. Probably some off-station role or random ghost(role).
            if (job == null && !force)
            {
                shell.WriteLine("You do not have a job, you are not accessible by Nanotrasen, therefore unable to cryo.");
                return;
            }

            // // Disappearing randomly as an antag wouldn't be very fun. An admin can handle it if really needed.
            // if (isantag == true)
            // {
            //     shell.WriteLine("You are an antagonist, the Syndicate would not like this, and Nanotrasen will get too easy of a resolution. This would be inappropriate.");
            //     return;
            // }

            // Find a job prototype matching the mind role name (reverse Loc wooo!).
            foreach (var jobproto in _prototypeManager.EnumeratePrototypes(typeof(JobPrototype)))
            {
                if (job == null) break;

                var jobprotoo = (JobPrototype) jobproto;
                if (Loc.GetString(jobprotoo.Name).ToLower() == job.Name.ToLower()) jobprotot = jobprotoo;
            }

            // No matching job, Nanotrasen didn't employ you, who hired you? (jobs should ONLY be made for Nanotrasen ships.)
            if (jobprotot == null && !force)
            {
                shell.WriteLine("You do not have a job, you are not accessible by Nanotrasen, therefore unable to cryo.");
                return;
            }

            // Adminlogs
            _adminLogger.Add(LogType.LateJoin, LogImpact.High, $"{_entities.ToPrettyString(uid):player} is going into cryostasis");

            // TODO: put items in a box? a pile of items is *fine* but a box of sorts might be nice.
            // Unequip all their items, in case they didn't do what was advised in the description or someone needs something from them.
            _entities.TryGetComponent<InventoryComponent>(uid, out var inventoryComponent);
            var invSystem = _entities.System<InventorySystem>();
            if (invSystem.TryGetSlots(uid, out var slotDefinitions, inventoryComponent))
            {
                foreach (var slot in slotDefinitions)
                {
                    invSystem.TryUnequip(uid, slot.Name, true, true, false, inventoryComponent);
                }
            }

            // Ghost them, if they can't be, tell them.
            if (!EntitySystem.Get<GameTicker>().OnGhostAttempt(mind, false))
            {
                shell.WriteLine("You can't ghost right now.");
                return;
            }

            // EPIC sound
            SoundSystem.Play("/Audio/SimpleStation14/Effects/podwoosh.ogg", Filter.Pvs(uid), uid, AudioParams.Default.WithVolume(7f));

            // TODO: when multiple stations ever get loaded regularly at the same time, both with jobs, do something about this var maybe?
            // Find the first station (very likely the one with the jobs)
            EntityUid? station = null;
            station = EntitySystem.Get<StationSystem>().GetStations()[0];

            if (station != null && jobprotot != null)
            {
                // Send a cryostasis announcement to the station, if any.
                var statio = (EntityUid) station;

                EntitySystem.Get<ChatSystem>().DispatchStationAnnouncement(statio,
                    Loc.GetString("cryo-departure-announcement",
                        ("character", _entities.GetComponent<MetaDataComponent>(uid).EntityName),
                        ("job", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Loc.GetString(jobprotot.Name)))),
                    Loc.GetString("latejoin-arrival-sender"),
                    playDefaultSound: false);

                if (!_stationJobs.IsJobUnlimited(statio, jobprotot))
                {
                    _stationJobs.TryGetJobSlot(statio, jobprotot, out var slots);
                    if (slots != null)
                    {
                        _stationJobs.TryAdjustJobSlot(statio, jobprotot, (int) (slots + 1));
                    }
                }
            }
            else shell.WriteLine("No station found, leave announcement will not be sent.");


            // Put them into "Cryostasis".
            _entities.QueueDeleteEntity(uid);
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length == 1)
            {
                // what a great solution
                List<string> list = new();
                list.Add("true");
                list.Add("false");
                return CompletionResult.FromHintOptions(list, Loc.GetString("cryostasis-command-arg-force"));
            }

            return CompletionResult.Empty;
        }
    }
}
