using System;
using UnityEngine;

[Serializable]
public class ItemInstance
{
    public ItemData itemDef;

    public int currentStack = 1;
    public int currentAmmoInMag = 0;

    // 0 = 0°, 1 = 90°, 2 = 180°, 3 = 270° (zgodnie z ruchem wskazówek zegara)
    public int rotationStep = 0;

    [NonSerialized] public WorldItem worldLink;

    public ItemInstance(ItemData data, int amount = 1)
    {
        itemDef = data;
        currentStack = amount;
        rotationStep = 0;

        StackableModule stackModule = data.GetModule<StackableModule>();
        if (stackModule != null && currentStack > stackModule.maxStackSize)
        {
            currentStack = stackModule.maxStackSize;
        }
    }

    public void RotateClockwise()
    {
        rotationStep = (rotationStep + 1) % 4;
    }

    public Vector2Int GetBaseDimensions()
    {
        if (itemDef == null || itemDef.gridShape == null || itemDef.gridShape.Length == 0)
        {
            return Vector2Int.one;
        }

        int maxX = 0;
        int maxY = 0;
        foreach (Vector2Int pos in itemDef.gridShape)
        {
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y > maxY) maxY = pos.y;
        }

        return new Vector2Int(maxX + 1, maxY + 1);
    }

    public Vector2Int GetDimensions()
    {
        Vector2Int baseDims = GetBaseDimensions();

        if (rotationStep % 2 != 0)
        {
            return new Vector2Int(baseDims.y, baseDims.x);
        }

        return baseDims;
    }

    public Vector2Int[] GetCurrentGridShape()
    {
        if (itemDef == null || itemDef.gridShape == null)
        {
            return new Vector2Int[] { Vector2Int.zero };
        }

        int step = ((rotationStep % 4) + 4) % 4;
        if (step == 0)
        {
            return itemDef.gridShape;
        }

        Vector2Int baseDims = GetBaseDimensions();
        int baseWidth = baseDims.x;
        int baseHeight = baseDims.y;

        Vector2Int[] rotatedShape = new Vector2Int[itemDef.gridShape.Length];
        for (int i = 0; i < itemDef.gridShape.Length; i++)
        {
            Vector2Int p = itemDef.gridShape[i];
            switch (step)
            {
                case 1: // 90 stopni w prawo
                    rotatedShape[i] = new Vector2Int(baseHeight - 1 - p.y, p.x);
                    break;
                case 2: // 180 stopni
                    rotatedShape[i] = new Vector2Int(baseWidth - 1 - p.x, baseHeight - 1 - p.y);
                    break;
                case 3: // 270 stopni w prawo
                    rotatedShape[i] = new Vector2Int(p.y, baseWidth - 1 - p.x);
                    break;
            }
        }

        return rotatedShape;
    }
}