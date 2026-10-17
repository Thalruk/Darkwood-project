using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridSlotUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int x;
    public int y;
    public PocketUI parentPocket;
    private Image image;

    private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    private Color occupiedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    private Color validColor = new Color(0f, 1f, 0f, 0.5f);
    private Color invalidColor = new Color(1f, 0f, 0f, 0.5f);

    public static GridSlotUI currentlyHoveredSlot;
    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void RefreshColor()
    {
        if (parentPocket.logicGrid.GetItemAt(x, y) != null)
        {
            image.color = occupiedColor;
        }
        else
        {
            image.color = emptyColor;
        }
    }

    public void SetHighlight(bool isValid)
    {
        image.color = isValid ? validColor : invalidColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        currentlyHoveredSlot = this;
        if (DraggableItem.currentlyDraggedItem == null)
        {
            return;
        }

        Vector2Int offset = DraggableItem.currentlyDraggedItem.grabbedCellOffset;
        parentPocket.HighlightShape(DraggableItem.currentlyDraggedItem.itemInstance, x - offset.x, y - offset.y);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentlyHoveredSlot == this)
        {
            currentlyHoveredSlot = null;
        }

        if (DraggableItem.currentlyDraggedItem == null)
        {
            return;
        }

        Vector2Int offset = DraggableItem.currentlyDraggedItem.grabbedCellOffset;
        parentPocket.ClearHighlight(DraggableItem.currentlyDraggedItem.itemInstance, x - offset.x, y - offset.y);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableItem droppedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (droppedItem != null)
            {
                // 1. Upuszczanie na ziemię (Lewy Panel)
                if (parentPocket.logicGrid.IsGround)
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
                    return;
                }

                // 2. Upuszczanie do kieszeni (Prawy Panel)
                int targetX = x - droppedItem.grabbedCellOffset.x;
                int targetY = y - droppedItem.grabbedCellOffset.y;

                if (parentPocket.logicGrid.TryPlaceItem(droppedItem.itemInstance, targetX, targetY))
                {
                    if (droppedItem.currentEquipmentSlot != null)
                    {
                        droppedItem.currentEquipmentSlot.ClearSlotMemory();
                        droppedItem.currentEquipmentSlot = null;
                    }

                    droppedItem.DestroyWorldLink();
                    Destroy(droppedItem.gameObject);

                    // Odświeżamy UI TYLKO wtedy, gdy faktycznie udało się włożyć przedmiot w nowe miejsce!
                    InventoryUI.Instance.RefreshUI();
                }
                else
                {
                    // Jeśli się nie udało (np. kolizja z innym przedmiotem lub wyjście poza siatkę),
                    // czyścimy podświetlenie, a OnEndDrag sam odłoży przedmiot na stare miejsce.
                    parentPocket.RefreshEntireGrid();
                }
            }
        }
    }
}