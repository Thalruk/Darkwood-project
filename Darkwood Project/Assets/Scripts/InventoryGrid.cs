using UnityEngine;

public class InventoryGrid
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    private ItemInstance[,] grid;

    public InventoryGrid(int width, int height)
    {
        Width = width;
        Height = height;
        grid = new ItemInstance[width, height];
    }

    public bool CanPlaceItem(ItemInstance item, int startX, int startY)
    {
        if (item == null || item.itemDef.gridShape == null) return false;

        foreach (Vector2Int shapePos in item.itemDef.gridShape)
        {
            int targetX = startX + shapePos.x;
            int targetY = startY + shapePos.y;

            if (targetX < 0 || targetX >= Width || targetY < 0 || targetY >= Height) return false;
            if (grid[targetX, targetY] != null) return false;
        }

        return true;
    }

    public bool TryPlaceItem(ItemInstance item, int startX, int startY)
    {
        if (!CanPlaceItem(item, startX, startY)) return false;

        foreach (Vector2Int shapePos in item.itemDef.gridShape)
        {
            int targetX = startX + shapePos.x;
            int targetY = startY + shapePos.y;
            grid[targetX, targetY] = item;
        }
        return true;
    }

    public void RemoveItem(ItemInstance item)
    {
        if (item == null) return;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (grid[x, y] == item)
                {
                    grid[x, y] = null;
                }
            }
        }
    }
}