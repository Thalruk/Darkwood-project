using System;
using UnityEngine;

[Serializable]
public class ItemInstance
{
    public ItemData itemDef;

    public int currentStack = 1;      
    public int currentAmmoInMag = 0;

    public ItemInstance(ItemData data, int amount = 1)
    {
        itemDef = data;
        currentStack = amount;

        StackableModule stackModule = data.GetModule<StackableModule>();
        if (stackModule != null && currentStack > stackModule.maxStackSize)
        {
            currentStack = stackModule.maxStackSize;
        }
    }
}