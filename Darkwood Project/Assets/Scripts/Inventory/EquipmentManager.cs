using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [Header("Hand Slots")]
    public ItemInstance rightHandSlot;
    public ItemInstance leftHandSlot;

    [Header("Armor Slots")]
    public ItemInstance pantsSlot;
    public ItemInstance beltSlot;
    public ItemInstance backpackSlot;

    [Header("World Interaction")]
    public Transform playerTransform;
    public GameObject worldItemPrefab;

    private List<InventoryGrid> activePantsGrids = new List<InventoryGrid>();
    private List<InventoryGrid> activeBackpackGrids = new List<InventoryGrid>();

    public void SpillAndClearGrids(List<InventoryGrid> grids)
    {
        List<ItemInstance> itemsToDrop = new List<ItemInstance>();

        foreach (InventoryGrid grid in grids)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    ItemInstance item = grid.GetItemAt(x, y);
                    if (item != null && !itemsToDrop.Contains(item))
                    {
                        itemsToDrop.Add(item);
                        grid.RemoveItem(item);
                    }
                }
            }
        }

        foreach (ItemInstance item in itemsToDrop)
        {
            Debug.Log($"[EquipmentManager] Spilled {item.itemDef.displayName} to the ground.");
            DropItemOnGround(item);
        }

        grids.Clear();
    }
    private bool HasValidItem(ItemInstance item)
    {
        return item != null && item.itemDef != null;
    }
    public bool TryEquip(ItemInstance item, bool toRightHand)
    {
        if (!HasValidItem(item)) return false;

        EquippableModule equipModule = item.itemDef.GetModule<EquippableModule>();
        if (equipModule == null || equipModule.equipType != EquipmentType.Weapon)
        {
            Debug.Log($"[EquipmentManager] Item {item.itemDef.displayName} is not a weapon!");
            return false;
        }

        if (equipModule.isTwoHanded)
        {
            if (rightHandSlot == item && leftHandSlot == item) return true;

            if (HasValidItem(rightHandSlot)) DropItemOnGround(rightHandSlot);
            if (HasValidItem(leftHandSlot) && leftHandSlot != rightHandSlot) DropItemOnGround(leftHandSlot);

            rightHandSlot = item;
            leftHandSlot = item;
            return true;
        }

        if (toRightHand)
        {
            if (rightHandSlot == item) return true;

            if (HasValidItem(rightHandSlot)) DropItemOnGround(rightHandSlot);
            rightHandSlot = item;
        }
        else
        {
            if (leftHandSlot == item) return true;

            if (HasValidItem(leftHandSlot)) DropItemOnGround(leftHandSlot);
            leftHandSlot = item;
        }

        return true;
    }

    public bool EquipPants(ItemInstance pantsItem)
    {
        if (!HasValidItem(pantsItem)) return false;

        EquippableModule equipModule = pantsItem.itemDef.GetModule<EquippableModule>();
        if (equipModule == null || equipModule.equipType != EquipmentType.Pants) return false;

        ContainerModule container = pantsItem.itemDef.GetModule<ContainerModule>();
        if (container == null) return false;

        if (pantsSlot == pantsItem) return true;

        if (HasValidItem(pantsSlot))
        {
            SpillAndClearGrids(activePantsGrids);
            DropItemOnGround(pantsSlot);
        }

        pantsSlot = pantsItem;
        activePantsGrids.Clear();

        foreach (PocketDefinition pocket in container.pockets)
        {
            activePantsGrids.Add(new InventoryGrid(pocket.width, pocket.height));
        }
        return true;
    }

    public bool EquipBelt(ItemInstance beltItem)
    {
        if (!HasValidItem(beltItem)) return false;

        EquippableModule equipModule = beltItem.itemDef.GetModule<EquippableModule>();
        if (equipModule == null || equipModule.equipType != EquipmentType.Belt) return false;

        if (beltSlot == beltItem) return true;

        if (HasValidItem(beltSlot)) DropItemOnGround(beltSlot);

        beltSlot = beltItem;
        return true;
    }

    public bool EquipBackpack(ItemInstance backpackItem)
    {
        if (!HasValidItem(backpackItem)) return false;

        EquippableModule equipModule = backpackItem.itemDef.GetModule<EquippableModule>();
        if (equipModule == null || equipModule.equipType != EquipmentType.Backpack) return false;

        ContainerModule container = backpackItem.itemDef.GetModule<ContainerModule>();
        if (container == null) return false;

        if (backpackSlot == backpackItem) return true;

        if (HasValidItem(backpackSlot))
        {
            SpillAndClearGrids(activeBackpackGrids);
            DropItemOnGround(backpackSlot);
        }

        backpackSlot = backpackItem;
        activeBackpackGrids.Clear();

        foreach (PocketDefinition pocket in container.pockets)
        {
            activeBackpackGrids.Add(new InventoryGrid(pocket.width, pocket.height));
        }
        return true;
    }

    public void DropItemOnGround(ItemInstance item)
    {
        if (!HasValidItem(item) || worldItemPrefab == null || playerTransform == null) return;

        Vector3 dropOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
        GameObject droppedObj = Instantiate(worldItemPrefab, playerTransform.position + dropOffset, Quaternion.identity);

        WorldItem worldItem = droppedObj.GetComponent<WorldItem>();
        if (worldItem != null)
        {
            worldItem.Setup(item);
        }
    }

    public List<InventoryGrid> GetActivePantsGrids() { return activePantsGrids; }
    public List<InventoryGrid> GetActiveBackpackGrids() { return activeBackpackGrids; }
}