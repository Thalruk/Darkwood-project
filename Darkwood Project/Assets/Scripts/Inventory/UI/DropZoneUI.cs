using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZoneUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image bgImage;
    private Color defaultColor;
    private Color hoverColor = new Color(0.6f, 0.1f, 0.1f, 0.5f);

    [Header("Memory Connection")]
    public EquipmentManager equipmentManager;

    private void Awake()
    {
        bgImage = GetComponent<Image>();
        if (bgImage != null)
        {
            defaultColor = bgImage.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DraggableItem.currentlyDraggedItem != null && bgImage != null)
        {
            bgImage.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (bgImage != null)
        {
            bgImage.color = defaultColor;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (bgImage != null) bgImage.color = defaultColor;

        if (eventData.pointerDrag != null)
        {
            DraggableItem droppedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (droppedItem != null)
            {
                if (droppedItem.currentSlot != null)
                {
                    droppedItem.currentSlot.parentPocket.logicGrid.RemoveItem(droppedItem.itemInstance);
                    droppedItem.currentSlot.parentPocket.RefreshEntireGrid();
                }

                if (equipmentManager != null)
                {
                    equipmentManager.DropItemOnGround(droppedItem.itemInstance);
                }

                Destroy(droppedItem.gameObject);
            }
        }
    }
}