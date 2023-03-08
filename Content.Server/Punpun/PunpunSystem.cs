using System.Linq;
using Content.Server.GameTicking;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;

namespace Content.Server.SimpleStation14.Punpun;

public class PunpunSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    (int, string, string) punpunData = (0, String.Empty, String.Empty);

    // Get all the roman numerals we'll need to display
    Dictionary<int, string> numerals = new Dictionary<int, string>()
    {
        { 0, "I" },
        { 1, "II" },
        { 2, "III" },
        { 3, "IV" },
        { 4, "V" },
        { 5, "VI" },
        { 6, "VII" },
        { 7, "VIII" },
        { 8, "IX" },
        { 9, "X" },
        { 10, "XI" },
        { 11, "XII" },
        { 12, "XIII" },
        { 13, "XIV" }
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PunpunComponent, ComponentStartup>(OnRoundStart);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEnd);

    }

    private void OnRoundStart(EntityUid uid, PunpunComponent component, ComponentStartup args)
    {
        component.owner = uid;

        if (punpunData.Item1 > 13)
        {
            _entityManager.DeleteEntity(uid);
            punpunData = (0, String.Empty, String.Empty);
            return;
        }

        var metaCompEnt = _entityManager.GetComponent<MetaDataComponent>(uid);
        var name = metaCompEnt.EntityName;
        metaCompEnt.EntityName = name + " " + numerals[punpunData.Item1];

        if (_entityManager.TryGetComponent<InventoryComponent>(uid, out var invComp))
        {
            if (punpunData.Item2 != String.Empty)
            {
                var hat = _entityManager.SpawnEntity(punpunData.Item2, EntityManager.GetComponent<TransformComponent>(uid).Coordinates);
                if (_inventorySystem.TryEquip(uid, hat, "head", true))
                {
                    var metaCompHat = _entityManager.GetComponent<MetaDataComponent>(hat);
                    var hatName = metaCompHat.EntityName;
                    metaCompHat.EntityName = name + "'s " + hatName;
                }
                else
                {
                    _entityManager.DeleteEntity(hat);
                }
            }

            if (punpunData.Item3 != String.Empty)
            {
                var mask = EntityManager.SpawnEntity(punpunData.Item3, EntityManager.GetComponent<TransformComponent>(uid).Coordinates);
                if (_inventorySystem.TryEquip(uid, mask, "mask", true))
                {
                    var metaCompMask = _entityManager.GetComponent<MetaDataComponent>(mask);
                    var maskName = metaCompMask.EntityName;
                    metaCompMask.EntityName = name + "'s " + maskName;
                }
                else
                {
                    _entityManager.DeleteEntity(mask);
                }
            }
        }
    }

    private void OnRoundEnd(RoundEndTextAppendEvent ev)
    {
        // Do an entity query to get all the punpun components
        var punpunComponents = EntityManager.EntityQuery<PunpunComponent>();
        if (punpunComponents.Count() == 0)
        {
            punpunData = (0, String.Empty, String.Empty);
            return;
        }

        var component = punpunComponents.First();
        var uid = component.owner;

        if (!_entityManager.TryGetComponent<MobStateComponent>(uid, out var mobComp) || mobComp.CurrentState == MobState.Dead)
        {
            punpunData = (0, String.Empty, String.Empty);
            return;
        }

        punpunData.Item1++;

        if (_entityManager.TryGetComponent<InventoryComponent>(uid, out var invComp))
        {
            punpunData.Item2 = checkSlot(uid, "head");
            punpunData.Item3 = checkSlot(uid, "mask");
        }
    }

    public string checkSlot(EntityUid uid, string slot)
    {
        if (_inventorySystem.TryGetSlotEntity(uid, slot, out var item) && item != null)
        {
            return _entityManager.GetComponent<MetaDataComponent>(item.Value).EntityPrototype!.ID;
        }
        return String.Empty;
    }
}
