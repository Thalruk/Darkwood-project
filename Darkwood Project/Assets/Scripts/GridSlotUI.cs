using UnityEngine;
using UnityEngine.EventSystems;

public class GridSlotUI : MonoBehaviour, IDropHandler
{
    public int x;
    public int y;
    public InventoryGrid parentGrid;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableItem droppedItem = eventData.pointerDrag.GetComponent<DraggableItem>();

            if (droppedItem != null)
            {
                bool canPlace = parentGrid.TryPlaceItem(droppedItem.itemInstance, x, y);

                if (canPlace)
                {
                    Debug.Log($"<color=green>Sukces!</color> Przedmiot {droppedItem.itemInstance.itemDef.displayName} wylądował na koordynatach X:{x} Y:{y}");

                    droppedItem.transform.SetParent(transform);

                    droppedItem.transform.localPosition = Vector3.zero;
                }
                else
                {
                    Debug.Log($"<color=red>Kolizja!</color> Przedmiot nie mieści się na X:{x} Y:{y}");
                }
            }
        }
    }
}