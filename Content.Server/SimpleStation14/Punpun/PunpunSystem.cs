using System.Linq;
using Content.Server.GameTicking;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;

namespace Content.Server.SimpleStation14.Punpun;

public sealed class PunpunSystem : EntitySystem
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

    // Checks if the Punpun data has any itemss to eqiup, and names the Punpun upon initialization.
    private void OnRoundStart(EntityUid uid, PunpunComponent component, ComponentStartup args)
    {
        component.owner = uid;

        if (punpunData.Item1 > 13)
        {
            _entityManager.SpawnEntity("PaperWrittenPunpunNote", EntityManager.GetComponent<TransformComponent>(uid).Coordinates);
            _entityManager.QueueDeleteEntity(uid);
            punpunData = (0, String.Empty, String.Empty);

            return;
        }

        var metaCompEnt = _entityManager.GetComponent<MetaDataComponent>(uid);
        var name = metaCompEnt.EntityName;

        metaCompEnt.EntityName = name + " " + numerals[punpunData.Item1];

        if (_entityManager.TryGetComponent<InventoryComponent>(uid, out var invComp))
        {
            EquipItem(uid, "head", punpunData.Item2);
            EquipItem(uid, "mask", punpunData.Item3);
        }
    }

    // Checks if Punpun exists, and is alive at round end.
    // If so, stores the items and increments the Punpun count.
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
            punpunData.Item2 = CheckSlot(uid, "head");
            punpunData.Item3 = CheckSlot(uid, "mask");
        }
    }

    // Equips an item to a slot, and names it.
    private void EquipItem(EntityUid uid, string slot, string item)
    {
        if (item == String.Empty)
            return;

        var itemEnt = EntityManager.SpawnEntity(item, EntityManager.GetComponent<TransformComponent>(uid).Coordinates);
        if (_inventorySystem.TryEquip(uid, itemEnt, slot, true))
        {
            var metaCompItem = _entityManager.GetComponent<MetaDataComponent>(itemEnt);
            var itemName = metaCompItem.EntityName;

            metaCompItem.EntityName = EntityManager.GetComponent<MetaDataComponent>(uid).EntityName + "'s " + itemName;
        }
        else _entityManager.DeleteEntity(itemEnt);
    }

    // Checks if an item exists in a slot, and returns its name.
    private string CheckSlot(EntityUid uid, string slot)
    {
        if (_inventorySystem.TryGetSlotEntity(uid, slot, out var item) && item != null)
            return _entityManager.GetComponent<MetaDataComponent>(item.Value).EntityPrototype!.ID;
        
        return String.Empty;
    }
}
