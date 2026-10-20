using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [Header("Hand Slots")]
    public ItemInstance rightHandSlot;
    public ItemInstance leftHandSlot;

    [Header("Armor & Clothing Slots")]
    public ItemInstance headSlot;
    public ItemInstance backpackSlot;
    public ItemInstance bodySlot;
    public ItemInstance beltSlot;
    public ItemInstance pantsSlot;

    [Header("World Interaction")]
    public Transform playerTransform;
    public GameObject worldItemPrefab;

    private List<InventoryGrid> activeBackpackGrids = new List<InventoryGrid>();
    private List<InventoryGrid> activeBodyGrids = new List<InventoryGrid>();
    private List<InventoryGrid> activeBeltGrids = new List<InventoryGrid>();
    private List<InventoryGrid> activePantsGrids = new List<InventoryGrid>();

    /// <summary>
    /// Validates whether an ItemInstance reference actually contains valid item data.
    /// </summary>
    public bool HasValidItem(ItemInstance item)
    {
        return item != null && item.itemDef != null;
    }

    /// <summary>
    /// Safely clears a hand slot, ensuring two-handed weapons are removed from both hands simultaneously.
    /// </summary>
    public void ClearHandSlot(bool isRightHand)
    {
        if (rightHandSlot != null && rightHandSlot == leftHandSlot)
        {
            rightHandSlot = null;
            leftHandSlot = null;
            return;
        }

        if (isRightHand)
        {
            rightHandSlot = null;
        }
        else
        {
            leftHandSlot = null;
        }
    }

    /// <summary>
    /// Removes all items from the provided grids and spawns them on the ground around the player.
    /// </summary>
    public void SpillAndClearGrids(List<InventoryGrid> grids)
    {
        if (grids == null || grids.Count == 0)
        {
            return;
        }

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
            DropItemOnGround(item);
        }

        grids.Clear();
    }

    /// <summary>
    /// Attempts to automatically place an item into available pockets following top-to-bottom UI order:
    /// Backpack -> Body -> Belt -> Pants.
    /// </summary>
    public bool TryStoreInPockets(ItemInstance item)
    {
        if (TryPlaceInGridList(activeBackpackGrids, item)) return true;
        if (TryPlaceInGridList(activeBodyGrids, item)) return true;
        if (TryPlaceInGridList(activeBeltGrids, item)) return true;
        if (TryPlaceInGridList(activePantsGrids, item)) return true;

        return false;
    }

    private bool TryPlaceInGridList(List<InventoryGrid> grids, ItemInstance item)
    {
        int initialRotation = item.rotationStep;

        foreach (InventoryGrid grid in grids)
        {
            if (grid.OwnerItem == item)
            {
                continue;
            }

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    for (int rot = 0; rot < 4; rot++)
                    {
                        item.rotationStep = (initialRotation + rot) % 4;
                        if (grid.TryPlaceItem(item, x, y))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        item.rotationStep = initialRotation;
        return false;
    }

    /// <summary>
    /// Stores an unequipped item into pockets if space allows; otherwise drops it on the ground.
    /// </summary>
    public void HandleUnequippedItem(ItemInstance item)
    {
        if (!HasValidItem(item))
        {
            return;
        }

        if (TryStoreInPockets(item))
        {
            return;
        }

        DropItemOnGround(item);
    }

    /// <summary>
    /// Equips a weapon to the specified hand, properly handling two-handed weapon conflicts.
    /// </summary>
    public bool TryEquip(ItemInstance item, bool toRightHand)
    {
        if (!HasValidItem(item))
        {
            return false;
        }

        EquippableModule equipModule = item.itemDef.GetModule<EquippableModule>();
        if (equipModule == null || equipModule.equipType != EquipmentType.Weapon)
        {
            return false;
        }

        if (equipModule.isTwoHanded)
        {
            if (rightHandSlot == item && leftHandSlot == item)
            {
                return true;
            }

            ItemInstance oldRight = rightHandSlot;
            ItemInstance oldLeft = leftHandSlot;

            rightHandSlot = item;
            leftHandSlot = item;

            if (HasValidItem(oldRight) && oldRight != item)
            {
                HandleUnequippedItem(oldRight);
            }
            if (HasValidItem(oldLeft) && oldLeft != oldRight && oldLeft != item)
            {
                HandleUnequippedItem(oldLeft);
            }

            return true;
        }

        if (HasValidItem(rightHandSlot) && rightHandSlot == leftHandSlot)
        {
            ItemInstance oldTwoHanded = rightHandSlot;
            rightHandSlot = null;
            leftHandSlot = null;

            if (toRightHand)
            {
                rightHandSlot = item;
            }
            else
            {
                leftHandSlot = item;
            }

            if (oldTwoHanded != item)
            {
                HandleUnequippedItem(oldTwoHanded);
            }
            return true;
        }

        if (toRightHand)
        {
            if (rightHandSlot == item)
            {
                return true;
            }

            if (leftHandSlot == item)
            {
                leftHandSlot = null;
            }

            ItemInstance oldRight = rightHandSlot;
            rightHandSlot = item;

            if (HasValidItem(oldRight))
            {
                HandleUnequippedItem(oldRight);
            }
        }
        else
        {
            if (leftHandSlot == item)
            {
                return true;
            }

            if (rightHandSlot == item)
            {
                rightHandSlot = null;
            }

            ItemInstance oldLeft = leftHandSlot;
            leftHandSlot = item;

            if (HasValidItem(oldLeft))
            {
                HandleUnequippedItem(oldLeft);
            }
        }

        return true;
    }

    public bool EquipHead(ItemInstance headItem)
    {
        if (!HasValidItem(headItem))
        {
            return false;
        }

        EquippableModule equipModule = headItem.itemDef.GetModule<EquippableModule>();
        if (equipModule == null || equipModule.equipType != EquipmentType.Head)
        {
            return false;
        }

        if (headSlot == headItem)
        {
            return true;
        }

        ItemInstance oldHead = headSlot;
        headSlot = headItem;

        if (HasValidItem(oldHead))
        {
            HandleUnequippedItem(oldHead);
        }
        return true;
    }

    public bool EquipBackpack(ItemInstance backpackItem)
    {
        return EquipContainerSlot(backpackItem, EquipmentType.Backpack, ref backpackSlot, activeBackpackGrids, true);
    }

    public bool EquipBody(ItemInstance bodyItem)
    {
        return EquipContainerSlot(bodyItem, EquipmentType.Body, ref bodySlot, activeBodyGrids, false);
    }

    public bool EquipBelt(ItemInstance beltItem)
    {
        return EquipContainerSlot(beltItem, EquipmentType.Belt, ref beltSlot, activeBeltGrids, false);
    }

    public bool EquipPants(ItemInstance pantsItem)
    {
        return EquipContainerSlot(pantsItem, EquipmentType.Pants, ref pantsSlot, activePantsGrids, true);
    }

    /// <summary>
    /// Unified helper for equipping armor/clothing items that may optionally or mandatorily contain pockets.
    /// </summary>
    private bool EquipContainerSlot(
        ItemInstance newItem,
        EquipmentType expectedType,
        ref ItemInstance slotRef,
        List<InventoryGrid> targetGridList,
        bool requireContainerModule)
    {
        if (!HasValidItem(newItem))
        {
            return false;
        }

        EquippableModule equipModule = newItem.itemDef.GetModule<EquippableModule>();
        if (equipModule == null || equipModule.equipType != expectedType)
        {
            return false;
        }

        ContainerModule container = newItem.itemDef.GetModule<ContainerModule>();
        if (requireContainerModule && container == null)
        {
            return false;
        }

        if (slotRef == newItem)
        {
            return true;
        }

        ItemInstance oldItem = slotRef;
        List<InventoryGrid> oldGrids = new List<InventoryGrid>(targetGridList);

        slotRef = newItem;
        targetGridList.Clear();

        if (container != null && container.pockets != null)
        {
            foreach (PocketDefinition pocket in container.pockets)
            {
                targetGridList.Add(new InventoryGrid(pocket.width, pocket.height, false, newItem));
            }
        }

        if (HasValidItem(oldItem))
        {
            SpillAndClearGrids(oldGrids);
            HandleUnequippedItem(oldItem);
        }

        return true;
    }

    /// <summary>
    /// Instantiates a physical WorldItem representation of the item around the player's position.
    /// </summary>
    public void DropItemOnGround(ItemInstance item)
    {
        if (!HasValidItem(item) || worldItemPrefab == null || playerTransform == null)
        {
            return;
        }

        item.rotationStep = 0;
        Vector3 dropOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0f);
        GameObject droppedObj = Instantiate(worldItemPrefab, playerTransform.position + dropOffset, Quaternion.identity);

        WorldItem worldItem = droppedObj.GetComponent<WorldItem>();
        if (worldItem != null)
        {
            worldItem.Setup(item);
        }
    }

    public List<InventoryGrid> GetActiveBackpackGrids() { return activeBackpackGrids; }
    public List<InventoryGrid> GetActiveBodyGrids() { return activeBodyGrids; }
    public List<InventoryGrid> GetActiveBeltGrids() { return activeBeltGrids; }
    public List<InventoryGrid> GetActivePantsGrids() { return activePantsGrids; }
}