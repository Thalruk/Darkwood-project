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

    private void Awake() { image = GetComponent<Image>(); }

    public void RefreshColor()
    {
        if (parentPocket.logicGrid.GetItemAt(x, y) != null) image.color = occupiedColor;
        else image.color = emptyColor;
    }

    public void SetHighlight(bool isValid) { image.color = isValid ? validColor : invalidColor; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DraggableItem.currentlyDraggedItem == null) return;
        parentPocket.HighlightShape(DraggableItem.currentlyDraggedItem.itemInstance, x, y);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (DraggableItem.currentlyDraggedItem == null) return;
        parentPocket.ClearHighlight(DraggableItem.currentlyDraggedItem.itemInstance, x, y);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableItem droppedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (droppedItem != null)
            {
                if (parentPocket.logicGrid.TryPlaceItem(droppedItem.itemInstance, x, y))
                {
                    if (droppedItem.currentEquipmentSlot != null)
                    {
                        droppedItem.currentEquipmentSlot.ClearSlotMemory();
                        droppedItem.currentEquipmentSlot = null;
                    }

                    if (droppedItem.itemInstance.worldLink != null)
                    {
                        Debug.Log($"[GridSlotUI] Picked up physical item from ground: {droppedItem.itemInstance.itemDef.displayName}");
                        Destroy(droppedItem.itemInstance.worldLink.gameObject);
                        droppedItem.itemInstance.worldLink = null;
                    }

                    droppedItem.currentSlot = this;
                    droppedItem.transform.SetParent(transform.parent);
                    droppedItem.transform.position = transform.position;
                    droppedItem.transform.SetAsLastSibling();
                }
                else
                {
                    Debug.LogWarning($"[GridSlotUI] Cannot place {droppedItem.itemInstance.itemDef.displayName} here. No space!");
                }

                parentPocket.RefreshEntireGrid();
            }
        }
    }
}