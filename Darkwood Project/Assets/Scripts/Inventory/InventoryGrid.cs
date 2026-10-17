using System.Collections.Generic;
using UnityEngine;

public class InventoryGrid
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool IsGround { get; private set; }

    private ItemInstance[,] grid;
    private Dictionary<ItemInstance, Vector2Int> itemOrigins = new Dictionary<ItemInstance, Vector2Int>();

    public InventoryGrid(int width, int height, bool isGround = false)
    {
        Width = width;
        Height = height;
        IsGround = isGround;
        grid = new ItemInstance[width, height];
    }

    /// <summary>
    /// Checks if an item can be placed at given coordinates without overlapping or going out of bounds.
    /// </summary>
    public bool CanPlaceItem(ItemInstance item, int startX, int startY)
    {
        if (item == null || item.itemDef == null || item.itemDef.gridShape == null)
        {
            return false;
        }
        if (grid == null)
        {
            return false;
        }

        foreach (Vector2Int shapePos in item.GetCurrentGridShape())
        {
            int targetX = startX + shapePos.x;
            int targetY = startY + shapePos.y;

            if (targetX < 0 || targetX >= Width || targetY < 0 || targetY >= Height)
            {
                return false;
            }
            if (grid[targetX, targetY] != null)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Places an item on the grid and registers its origin coordinates.
    /// </summary>
    public bool TryPlaceItem(ItemInstance item, int startX, int startY)
    {
        if (!CanPlaceItem(item, startX, startY))
        {
            return false;
        }

        foreach (Vector2Int shapePos in item.GetCurrentGridShape())
        {
            int targetX = startX + shapePos.x;
            int targetY = startY + shapePos.y;
            grid[targetX, targetY] = item;
        }

        itemOrigins[item] = new Vector2Int(startX, startY);
        return true;
    }

    /// <summary>
    /// Clears grid cells occupied by the specified item and removes its origin record.
    /// </summary>
    public void RemoveItem(ItemInstance item)
    {
        if (item == null)
        {
            return;
        }

        itemOrigins.Remove(item);

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

    public Vector2Int GetItemOrigin(ItemInstance item)
    {
        return itemOrigins.TryGetValue(item, out Vector2Int origin) ? origin : Vector2Int.zero;
    }

    public ItemInstance GetItemAt(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return null;
        }
        return grid[x, y];
    }
}