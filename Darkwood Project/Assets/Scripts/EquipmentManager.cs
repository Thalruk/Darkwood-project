using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [Header("Hand slots")]
    public ItemInstance rightHandSlot;
    public ItemInstance leftHandSlot;

    [Header("Armor slots")]
    public ItemInstance pantsSlot;
    public ItemInstance pants;

    private List<InventoryGrid> activePantsGrids = new List<InventoryGrid>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EquipPants(pants);
            FindObjectOfType<InventoryUI>().RefreshUI();
        }
    }
    public bool TryEquip(ItemInstance item, bool toRightHand)
    {
        if (item == null) return false;

        EquippableModule equipModule = item.itemDef.GetModule<EquippableModule>();
        if (equipModule == null)
        {
            Debug.Log($"Przedmiot {item.itemDef.displayName} nie jest do trzymania w rękach!");
            return false;
        }

        if (equipModule.isTwoHanded)
        {
            if (rightHandSlot != null) DropItem(rightHandSlot);
            if (leftHandSlot != null && leftHandSlot != rightHandSlot) DropItem(leftHandSlot);

            rightHandSlot = item;
            leftHandSlot = item;

            Debug.Log($"Założono oburącz: {item.itemDef.displayName}");
            return true;
        }

        if (toRightHand)
        {
            if (rightHandSlot != null && rightHandSlot.itemDef.GetModule<EquippableModule>()?.isTwoHanded == true)
            {
                DropItem(rightHandSlot);
                leftHandSlot = null;
            }
            else if (rightHandSlot != null)
            {
                DropItem(rightHandSlot);
            }

            rightHandSlot = item;
            Debug.Log($"Prawa ręka chwyta: {item.itemDef.displayName}");
        }
        else
        {
            if (leftHandSlot != null && leftHandSlot.itemDef.GetModule<EquippableModule>()?.isTwoHanded == true)
            {
                DropItem(leftHandSlot);
                rightHandSlot = null;
            }
            else if (leftHandSlot != null)
            {
                DropItem(leftHandSlot);
            }

            leftHandSlot = item;
            Debug.Log($"Lewa ręka chwyta: {item.itemDef.displayName}");
        }

        return true;
    }

    public void EquipPants(ItemInstance pantsItem)
    {
        if (pantsItem == null) return;

        ContainerModule container = pantsItem.itemDef.GetModule<ContainerModule>();
        if (container == null)
        {
            Debug.Log($"Przedmiot {pantsItem.itemDef.displayName} nie posiada kieszeni.");
            return;
        }

        pantsSlot = pantsItem;
        activePantsGrids.Clear();
        foreach (PocketDefinition pocket in container.pockets)
        {
            InventoryGrid newGrid = new InventoryGrid(pocket.width, pocket.height);
            activePantsGrids.Add(newGrid);

            Debug.Log($"Utworzono logiczną kieszeń w pamięci: {pocket.pocketName} ({pocket.width}x{pocket.height})");
        }
    }
    private void DropItem(ItemInstance item)
    {
        if (item == null) return;

        Debug.Log($"<color=orange>Wyrzucono na ziemię: {item.itemDef.displayName}</color>");
    }
    public List<InventoryGrid> GetActivePantsGrids()
    {
        return activePantsGrids;
    }
}

