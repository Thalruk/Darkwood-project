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

    public bool TryEquip(ItemInstance item, bool toRightHand)
    {
        if (item == null) return false;

        EquippableModule equipModule = item.itemDef.GetModule<EquippableModule>();
        if (equipModule == null || equipModule.equipType != EquipmentType.Weapon)
        {
            Debug.Log($"[EquipmentManager] Item {item.itemDef.displayName} is not a weapon!");
            return false;
        }

        if (equipModule.isTwoHanded)
        {
            if (rightHandSlot != null) DropItemOnGround(rightHandSlot);
            if (leftHandSlot != null && leftHandSlot != rightHandSlot) DropItemOnGround(leftHandSlot);

            rightHandSlot = item;
            leftHandSlot = item;
            return true;
        }

        if (toRightHand)
        {
            if (rightHandSlot != null) DropItemOnGround(rightHandSlot);
            rightHandSlot = item;
        }
        else
        {
            if (leftHandSlot != null) DropItemOnGround(leftHandSlot);
            leftHandSlot = item;
        }

        return true;
    }

    public bool EquipPants(ItemInstance pantsItem)
    {
        if (pantsItem == null) return false;

        EquippableModule equipModule = pantsItem.itemDef.GetModule<EquippableModule>();
        if (equipModule == null || equipModule.equipType != EquipmentType.Pants) return false;

        ContainerModule container = pantsItem.itemDef.GetModule<ContainerModule>();
        if (container == null) return false;

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
        if (beltItem == null) return false;

        EquippableModule equipModule = beltItem.itemDef.GetModule<EquippableModule>();
        if (equipModule == null || equipModule.equipType != EquipmentType.Belt) return false;

        beltSlot = beltItem;
        return true;
    }

    public bool EquipBackpack(ItemInstance backpackItem)
    {
        if (backpackItem == null) return false;

        EquippableModule equipModule = backpackItem.itemDef.GetModule<EquippableModule>();
        if (equipModule == null || equipModule.equipType != EquipmentType.Backpack) return false;

        ContainerModule container = backpackItem.itemDef.GetModule<ContainerModule>();
        if (container == null) return false;

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
        if (item == null || worldItemPrefab == null || playerTransform == null) return;

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