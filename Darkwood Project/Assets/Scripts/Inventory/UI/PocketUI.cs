using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PocketUI : MonoBehaviour
{
    [Header("Prefab References")]
    public TextMeshProUGUI titleText;
    public Transform gridContainer;

    public InventoryGrid logicGrid;
    private GridSlotUI[,] uiSlots;

    public void Initialize(InventoryGrid grid, GridSlotUI[,] slots)
    {
        logicGrid = grid;
        uiSlots = slots;

        if (titleText != null)
        {
            titleText.text = logicGrid.PocketName;
        }
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

                    GameObject itemObj = Instantiate(itemPrefab, gridContainer);
                    DraggableItem dragScript = itemObj.GetComponent<DraggableItem>();

                    dragScript.itemInstance = item;
                    dragScript.currentSlot = uiSlots[x, y];

                    itemObj.transform.position = uiSlots[x, y].transform.position;
                    itemObj.transform.SetAsLastSibling();
                }
            }
        }
    }

    public void HighlightShape(ItemInstance item, int startX, int startY)
    {
        bool canPlace = logicGrid.CanPlaceItem(item, startX, startY);

        foreach (Vector2Int shapePos in item.itemDef.gridShape)
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
        foreach (Vector2Int shapePos in item.itemDef.gridShape)
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