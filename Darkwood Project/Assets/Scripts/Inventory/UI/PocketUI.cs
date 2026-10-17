using System.Collections.Generic;
using UnityEngine;

public class PocketUI : MonoBehaviour
{
    [Header("Prefab References")]
    public Transform gridContainer;

    public InventoryGrid logicGrid;
    private GridSlotUI[,] uiSlots;

    public void Initialize(InventoryGrid grid, GridSlotUI[,] slots)
    {
        logicGrid = grid;
        uiSlots = slots;
    }

    public void RefreshEntireGrid()
    {
        for (int y = 0; y < logicGrid.Height; y++)
        {
            for (int x = 0; x < logicGrid.Width; x++)
            {
                uiSlots[x, y].RefreshColor();
            }
        }
    }

    public void SpawnSavedItems(GameObject itemPrefab)
    {
        List<ItemInstance> alreadySpawned = new List<ItemInstance>();

        for (int y = 0; y < logicGrid.Height; y++)
        {
            for (int x = 0; x < logicGrid.Width; x++)
            {
                ItemInstance item = logicGrid.GetItemAt(x, y);

                if (item != null && !alreadySpawned.Contains(item))
                {
                    alreadySpawned.Add(item);

                    Vector2Int origin = logicGrid.GetItemOrigin(item);

                    GameObject itemObj = Instantiate(itemPrefab, gridContainer);
                    DraggableItem dragScript = itemObj.GetComponent<DraggableItem>();

                    dragScript.itemInstance = item;
                    dragScript.currentSlot = uiSlots[origin.x, origin.y];
                    dragScript.currentEquipmentSlot = null;

                    dragScript.BuildVisualShape();
                    itemObj.transform.position = GetItemCenterPosition(item, origin.x, origin.y);
                    itemObj.transform.SetAsLastSibling();
                }
            }
        }
    }

    /// <summary>
    /// Calculates precise screen/world center position of an item across all its occupied slots.
    /// </summary>
    public Vector3 GetItemCenterPosition(ItemInstance item, int startX, int startY)
    {
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        Vector3[] corners = new Vector3[4];

        foreach (Vector2Int shapePos in item.GetCurrentGridShape())
        {
            int slotX = Mathf.Clamp(startX + shapePos.x, 0, logicGrid.Width - 1);
            int slotY = Mathf.Clamp(startY + shapePos.y, 0, logicGrid.Height - 1);

            RectTransform slotRect = uiSlots[slotX, slotY].GetComponent<RectTransform>();
            slotRect.GetWorldCorners(corners);

            min = Vector3.Min(min, corners[0]);
            max = Vector3.Max(max, corners[2]);
        }

        return (min + max) * 0.5f;
    }

    public void HighlightShape(ItemInstance item, int startX, int startY)
    {
        bool canPlace = logicGrid.CanPlaceItem(item, startX, startY);

        foreach (Vector2Int shapePos in item.GetCurrentGridShape())
        {
            int targetX = startX + shapePos.x;
            int targetY = startY + shapePos.y;

            if (targetX >= 0 && targetX < logicGrid.Width && targetY >= 0 && targetY < logicGrid.Height)
            {
                uiSlots[targetX, targetY].SetHighlight(canPlace);
            }
        }
    }

    public void ClearHighlight(ItemInstance item, int startX, int startY)
    {
        foreach (Vector2Int shapePos in item.GetCurrentGridShape())
        {
            int targetX = startX + shapePos.x;
            int targetY = startY + shapePos.y;

            if (targetX >= 0 && targetX < logicGrid.Width && targetY >= 0 && targetY < logicGrid.Height)
            {
                uiSlots[targetX, targetY].RefreshColor();
            }
        }
    }
}