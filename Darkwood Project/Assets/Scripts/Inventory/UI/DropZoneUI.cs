using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZoneUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image bgImage;
    private Color defaultColor;
    private Color hoverColor = new Color(0.6f, 0.1f, 0.1f, 0.5f);

    private void Awake()
    {
        bgImage = GetComponent<Image>();
        if (bgImage != null) defaultColor = bgImage.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DraggableItem.currentlyDraggedItem != null && bgImage != null) bgImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (bgImage != null) bgImage.color = defaultColor;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (bgImage != null) bgImage.color = defaultColor;

        if (eventData.pointerDrag != null)
        {
            DraggableItem droppedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (droppedItem != null)
            {
                if (droppedItem.itemInstance.worldLink == null)
                {
                    InventoryUI.Instance.equipmentManager.DropItemOnGround(droppedItem.itemInstance);
                }

                if (droppedItem.currentSlot != null)
                {
                    droppedItem.currentSlot.parentPocket.logicGrid.RemoveItem(droppedItem.itemInstance);
                }

                if (droppedItem.currentEquipmentSlot != null)
                {
                    droppedItem.currentEquipmentSlot.ClearSlotMemory();
                }

                Destroy(droppedItem.gameObject);

                InventoryUI.Instance.RefreshUI();
            }
        }
    }
}