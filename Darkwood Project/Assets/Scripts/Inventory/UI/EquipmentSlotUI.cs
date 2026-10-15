using UnityEngine;
using UnityEngine.EventSystems;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler
{
    public enum SlotType { Head, Body, Backpack, Belt, Pants, RightHand, LeftHand }

    [Header("Slot Configuration")]
    public SlotType slotType;

    [Header("Memory Connection")]
    public EquipmentManager equipmentManager;
    public GameObject draggableItemPrefab;

    private void OnEnable()
    {
        RefreshSlot();
    }

    public void RefreshSlot()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        if (equipmentManager == null || draggableItemPrefab == null) return;

        ItemInstance itemToSpawn = null;

        switch (slotType)
        {
            case SlotType.RightHand: itemToSpawn = equipmentManager.rightHandSlot; break;
            case SlotType.LeftHand: itemToSpawn = equipmentManager.leftHandSlot; break;
            case SlotType.Pants: itemToSpawn = equipmentManager.pantsSlot; break;
            case SlotType.Belt: itemToSpawn = equipmentManager.beltSlot; break;
        }

        if (itemToSpawn != null)
        {
            GameObject itemObj = Instantiate(draggableItemPrefab, transform);
            DraggableItem dragScript = itemObj.GetComponent<DraggableItem>();

            dragScript.itemInstance = itemToSpawn;
            dragScript.currentSlot = null;

            RectTransform rect = itemObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
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
                    case SlotType.RightHand:
                        success = equipmentManager.TryEquip(droppedItem.itemInstance, true);
                        break;
                    case SlotType.LeftHand:
                        success = equipmentManager.TryEquip(droppedItem.itemInstance, false);
                        break;
                    case SlotType.Pants:
                        equipmentManager.EquipPants(droppedItem.itemInstance);
                        success = true;
                        break;
                    case SlotType.Belt:
                        equipmentManager.EquipBelt(droppedItem.itemInstance);
                        success = true;
                        break;
                }

                if (success)
                {
                    if (droppedItem.currentSlot != null)
                    {
                        droppedItem.currentSlot.parentPocket.logicGrid.RemoveItem(droppedItem.itemInstance);
                        droppedItem.currentSlot = null;
                    }

                    Destroy(droppedItem.gameObject);
                    RefreshSlot();
                    InventoryUI.Instance.RefreshUI();
                }
            }
        }
    }
}