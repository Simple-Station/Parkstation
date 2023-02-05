using Content.Server.Cuffs.Components;
using Content.Server.Objectives.Interfaces;
using Content.Server.Station.Components;
using JetBrains.Annotations;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;

namespace Content.Server.Objectives.Interfaces
{
    public abstract class GenericFreewillCondition : IObjectiveCondition
    {
        private Mind.Mind? _mind;

        public IObjectiveCondition GetAssigned(Mind.Mind mind)
        {
            var clone = (GenericFreewillCondition)this.MemberwiseClone();
            clone._mind = mind;
            return clone;
        }

        public abstract string Title { get; }

        public abstract string Description { get; }

        public abstract SpriteSpecifier Icon { get;  }

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
            return other is GenericFreewillCondition esc && Equals(_mind, esc._mind);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((GenericFreewillCondition) obj);
        }

        public override int GetHashCode()
        {
            return _mind != null ? _mind.GetHashCode() : 0;
        }
    }
}
