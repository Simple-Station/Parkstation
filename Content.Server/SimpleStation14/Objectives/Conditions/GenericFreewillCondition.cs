using Content.Server.Cuffs.Components;
using Content.Server.Objectives.Interfaces;
using Content.Server.Station.Components;
using JetBrains.Annotations;
using Robust.Shared.Map.Components;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Server.Objectives.Conditions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed class GenericFlawedCondition : IObjectiveCondition, ISerializationHooks
    {
        private Mind.Mind? _mind;

        [DataField("title", required: true)]
        private string _title = "";

        [DataField("description", required: true)]
        private string _description = "";

        [DataField("icon", required: true)]
        private SpriteSpecifier _icon = SpriteSpecifier.Invalid;

        public IObjectiveCondition GetAssigned(Mind.Mind mind)
        {
            return new GenericFlawedCondition
            {
                _mind = mind,
                _title = _title,
                _description = _description,
                _icon = _icon,
            };
        }

        public string Title => Loc.GetString(_title);

        public string Description => Loc.GetString(_description);

        public SpriteSpecifier Icon => _icon;

        private bool IsAgentOnShuttle(TransformComponent agentXform, EntityUid? shuttle)
        {
            if (shuttle == null)
                return false;

            var entMan = IoCManager.Resolve<IEntityManager>();

            if (!entMan.TryGetComponent<MapGridComponent>(shuttle, out var shuttleGrid) ||
                !entMan.TryGetComponent<TransformComponent>(shuttle, out var shuttleXform))
            {
                return false;
            }

            return shuttleXform.WorldMatrix.TransformBox(shuttleGrid.LocalAABB).Contains(agentXform.WorldPosition);
        }

        public float Progress
        {
            get
            {
                var entMan = IoCManager.Resolve<IEntityManager>();

                if (_mind?.OwnedEntity == null
                    || !entMan.TryGetComponent<TransformComponent>(_mind.OwnedEntity, out var xform))
                    return 0f;

                var shuttleContainsAgent = false;
                var agentIsAlive = !_mind.CharacterDeadIC;
                var agentIsEscaping = !(entMan.TryGetComponent<CuffableComponent>(_mind.OwnedEntity, out var cuffed)
                                        && cuffed.CuffedHandCount > 0); // You're not escaping if you're restrained!

                // Any emergency shuttle counts for this objective.
                foreach (var stationData in entMan.EntityQuery<StationDataComponent>())
                {
                    if (IsAgentOnShuttle(xform, stationData.EmergencyShuttle))
                    {
                        shuttleContainsAgent = true;
                        break;
                    }
                }

                return (shuttleContainsAgent && agentIsAlive && agentIsEscaping) ? 1f : 0f;
            }
        }

        public float Difficulty => 1.3f;

        public bool Equals(IObjectiveCondition? other)
        {
            return other is GenericFlawedCondition esc && Equals(_mind, esc._mind);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((GenericFlawedCondition) obj);
        }

        public override int GetHashCode()
        {
            return _mind != null ? _mind.GetHashCode() : 0;
        }
    }
}
