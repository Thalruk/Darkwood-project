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

    [Header("Debug/Testing")]
    public ItemInstance pants;

    [Header("World Interaction")]
    public Transform playerTransform;
    public GameObject worldItemPrefab;

    private List<InventoryGrid> activePantsGrids = new List<InventoryGrid>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EquipPants(pants);
            InventoryUI.Instance.RefreshUI();
        }
    }

    public bool TryEquip(ItemInstance item, bool toRightHand)
    {
        if (item == null) return false;

        EquippableModule equipModule = item.itemDef.GetModule<EquippableModule>();
        if (equipModule == null)
        {
            Debug.Log($"Item {item.itemDef.displayName} cannot be held in hands!");
            return false;
        }

        if (equipModule.isTwoHanded)
        {
            if (rightHandSlot != null) DropItemOnGround(rightHandSlot);
            if (leftHandSlot != null && leftHandSlot != rightHandSlot) DropItemOnGround(leftHandSlot);

            rightHandSlot = item;
            leftHandSlot = item;

            Debug.Log($"Equipped two-handed: {item.itemDef.displayName}");
            return true;
        }

        if (toRightHand)
        {
            if (rightHandSlot != null && rightHandSlot.itemDef.GetModule<EquippableModule>()?.isTwoHanded == true)
            {
                DropItemOnGround(rightHandSlot);
                leftHandSlot = null;
            }
            else if (rightHandSlot != null)
            {
                DropItemOnGround(rightHandSlot);
            }

            rightHandSlot = item;
            Debug.Log($"Right hand equipped: {item.itemDef.displayName}");
        }
        else
        {
            if (leftHandSlot != null && leftHandSlot.itemDef.GetModule<EquippableModule>()?.isTwoHanded == true)
            {
                DropItemOnGround(leftHandSlot);
                rightHandSlot = null;
            }
            else if (leftHandSlot != null)
            {
                DropItemOnGround(leftHandSlot);
            }

            leftHandSlot = item;
            Debug.Log($"Left hand equipped: {item.itemDef.displayName}");
        }

        return true;
    }

    public void EquipPants(ItemInstance pantsItem)
    {
        if (pantsItem == null) return;

        ContainerModule container = pantsItem.itemDef.GetModule<ContainerModule>();
        if (container == null)
        {
            Debug.Log($"Item {pantsItem.itemDef.displayName} has no pockets.");
            return;
        }

        pantsSlot = pantsItem;
        activePantsGrids.Clear();

        foreach (PocketDefinition pocket in container.pockets)
        {
            InventoryGrid newGrid = new InventoryGrid(pocket.pocketName, pocket.width, pocket.height);
            activePantsGrids.Add(newGrid);

            Debug.Log($"Created logical pocket in memory: {pocket.pocketName} ({pocket.width}x{pocket.height})");
        }
    }

    public void EquipBelt(ItemInstance beltItem)
    {
        if (beltItem == null) return;

        beltSlot = beltItem;
        Debug.Log($"Equipped belt/accessory: {beltItem.itemDef.displayName}");
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

        Debug.Log($"Dropped physical item on ground: {item.itemDef.displayName}");
    }

    public List<InventoryGrid> GetActivePantsGrids()
    {
        return activePantsGrids;
    }
}