using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [Header("Hand slots")]
    public ItemData rightHandSlot;
    public ItemData leftHandSlot;

    [Header("Armor slots")]
    public ItemData pantsSlot;
    public ItemData pants;

    private List<InventoryGrid> activePantsGrids = new List<InventoryGrid>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EquipPants(pants);
            FindObjectOfType<InventoryUI>().RefreshUI();
        }
    }
    public bool TryEquip(ItemData item, bool toRightHand)
    {
        if (item == null) return false;

        EquippableModule equipModule = item.GetModule<EquippableModule>();
        if (equipModule == null)
        {
            Debug.Log($"Przedmiot {item.displayName} nie jest do trzymania w rękach!");
            return false;
        }

        if (equipModule.isTwoHanded)
        {
            if (rightHandSlot != null) DropItem(rightHandSlot);
            if (leftHandSlot != null && leftHandSlot != rightHandSlot) DropItem(leftHandSlot);

            rightHandSlot = item;
            leftHandSlot = item;

            Debug.Log($"Założono oburącz: {item.displayName}");
            return true;
        }

        if (toRightHand)
        {
            if (rightHandSlot != null && rightHandSlot.GetModule<EquippableModule>()?.isTwoHanded == true)
            {
                DropItem(rightHandSlot);
                leftHandSlot = null;
            }
            else if (rightHandSlot != null)
            {
                DropItem(rightHandSlot);
            }

            rightHandSlot = item;
            Debug.Log($"Prawa ręka chwyta: {item.displayName}");
        }
        else
        {
            if (leftHandSlot != null && leftHandSlot.GetModule<EquippableModule>()?.isTwoHanded == true)
            {
                DropItem(leftHandSlot);
                rightHandSlot = null;
            }
            else if (leftHandSlot != null)
            {
                DropItem(leftHandSlot);
            }

            leftHandSlot = item;
            Debug.Log($"Lewa ręka chwyta: {item.displayName}");
        }

        return true;
    }

    public void EquipPants(ItemData pantsItem)
    {
        if (pantsItem == null) return;

        ContainerModule container = pantsItem.GetModule<ContainerModule>();
        if (container == null)
        {
            Debug.Log($"Przedmiot {pantsItem.displayName} nie posiada kieszeni.");
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
    private void DropItem(ItemData item)
    {
        if (item == null) return;

        Debug.Log($"<color=orange>Wyrzucono na ziemię: {item.displayName}</color>");
    }
    public List<InventoryGrid> GetActivePantsGrids()
    {
        return activePantsGrids;
    }
}

