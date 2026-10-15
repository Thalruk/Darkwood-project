using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public ItemInstance itemInstance;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Setup(ItemInstance instance)
    {
        itemInstance = instance;

        if (itemInstance != null && itemInstance.itemDef != null && itemInstance.itemDef.icon != null)
        {
            spriteRenderer.sprite = itemInstance.itemDef.icon;
        }
    }
}