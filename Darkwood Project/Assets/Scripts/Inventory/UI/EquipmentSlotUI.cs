using UnityEngine;
using UnityEngine.EventSystems;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler
{
    public enum SlotType { Head, Body, Backpack, Belt, Pants, RightHand, LeftHand }

    [Header("Slot Configuration")]
    public SlotType slotType;

    [Header("Memory Connection")]
    public GameObject draggableItemPrefab;

    private void OnEnable() { RefreshSlot(); }

    public void RefreshSlot()
    {
        foreach (Transform child in transform) { Destroy(child.gameObject); }

        if (InventoryUI.Instance == null || InventoryUI.Instance.equipmentManager == null || draggableItemPrefab == null) return;

        ItemInstance itemToSpawn = null;
        switch (slotType)
        {
            case SlotType.RightHand: itemToSpawn = InventoryUI.Instance.equipmentManager.rightHandSlot; break;
            case SlotType.LeftHand: itemToSpawn = InventoryUI.Instance.equipmentManager.leftHandSlot; break;
            case SlotType.Pants: itemToSpawn = InventoryUI.Instance.equipmentManager.pantsSlot; break;
            case SlotType.Belt: itemToSpawn = InventoryUI.Instance.equipmentManager.beltSlot; break;
            case SlotType.Backpack: itemToSpawn = InventoryUI.Instance.equipmentManager.backpackSlot; break;
        }

        if (itemToSpawn != null)
        {
            GameObject itemObj = Instantiate(draggableItemPrefab, transform);
            DraggableItem dragScript = itemObj.GetComponent<DraggableItem>();

            dragScript.itemInstance = itemToSpawn;
            dragScript.currentSlot = null;
            dragScript.currentEquipmentSlot = this;

            RectTransform rect = itemObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
        }
    }

    public void ClearSlotMemory()
    {
        if (InventoryUI.Instance == null || InventoryUI.Instance.equipmentManager == null) return;
        switch (slotType)
        {
            case SlotType.RightHand: InventoryUI.Instance.equipmentManager.rightHandSlot = null; break;
            case SlotType.LeftHand: InventoryUI.Instance.equipmentManager.leftHandSlot = null; break;
            case SlotType.Pants: InventoryUI.Instance.equipmentManager.pantsSlot = null; break;
            case SlotType.Belt: InventoryUI.Instance.equipmentManager.beltSlot = null; break;
            case SlotType.Backpack: InventoryUI.Instance.equipmentManager.backpackSlot = null; break;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableItem droppedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (droppedItem != null)
            {
                bool success = false;
                switch (slotType)
                {
                    case SlotType.RightHand: success = InventoryUI.Instance.equipmentManager.TryEquip(droppedItem.itemInstance, true); break;
                    case SlotType.LeftHand: success = InventoryUI.Instance.equipmentManager.TryEquip(droppedItem.itemInstance, false); break;
                    case SlotType.Pants: InventoryUI.Instance.equipmentManager.EquipPants(droppedItem.itemInstance); success = true; break;
                    case SlotType.Belt: InventoryUI.Instance.equipmentManager.EquipBelt(droppedItem.itemInstance); success = true; break;
                    case SlotType.Backpack: InventoryUI.Instance.equipmentManager.EquipBackpack(droppedItem.itemInstance); success = true; break;
                }

                if (success)
                {
                    Debug.Log($"[EquipmentSlotUI] Successfully equipped {droppedItem.itemInstance.itemDef.displayName} to {slotType}");

                    if (droppedItem.currentSlot != null)
                    {
                        droppedItem.currentSlot.parentPocket.logicGrid.RemoveItem(droppedItem.itemInstance);
                        droppedItem.currentSlot = null;
                    }

                    if (droppedItem.currentEquipmentSlot != null && droppedItem.currentEquipmentSlot != this)
                    {
                        droppedItem.currentEquipmentSlot.ClearSlotMemory();
                    }

                    if (droppedItem.itemInstance.worldLink != null)
                    {
                        Debug.Log($"[EquipmentSlotUI] Removed physical {droppedItem.itemInstance.itemDef.displayName} from ground.");
                        Destroy(droppedItem.itemInstance.worldLink.gameObject);
                        droppedItem.itemInstance.worldLink = null;
                    }

                    Destroy(droppedItem.gameObject);
                    RefreshSlot();
                    InventoryUI.Instance.RefreshUI();
                }
                else
                {
                    Debug.LogWarning($"[EquipmentSlotUI] Failed to equip {droppedItem.itemInstance.itemDef.displayName} to {slotType}. Check EquippableModule.");
                }
            }
        }
    }
}