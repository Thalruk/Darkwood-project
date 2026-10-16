using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler
{
    public enum SlotType { Head, Body, Backpack, Belt, Pants, RightHand, LeftHand }

    [Header("Slot Configuration")]
    public SlotType slotType;

    [Header("Memory Connection")]
    public GameObject draggableItemPrefab;

    private Image backgroundImage;
    private Color defaultColor;
    private Color disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);

    private void Awake()
    {
        backgroundImage = GetComponent<Image>();
        if (backgroundImage != null)
        {
            defaultColor = backgroundImage.color;
        }
    }

    private void OnEnable()
    {
        RefreshSlot();
        DraggableItem.OnItemDragStart += HandleDragStart;
        DraggableItem.OnItemDragEnd += HandleDragEnd;
    }

    private void OnDisable()
    {
        DraggableItem.OnItemDragStart -= HandleDragStart;
        DraggableItem.OnItemDragEnd -= HandleDragEnd;
    }

    private void HandleDragStart(ItemInstance item)
    {
        if (backgroundImage == null) return;

        if (CanAcceptItem(item))
        {
            backgroundImage.color = defaultColor;
        }
        else
        {
            backgroundImage.color = disabledColor;
        }
    }

    private void HandleDragEnd()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = defaultColor;
        }
    }

    private bool CanAcceptItem(ItemInstance item)
    {
        if (item == null || item.itemDef == null) return false;

        EquippableModule equipModule = item.itemDef.GetModule<EquippableModule>();
        if (equipModule == null) return false;

        switch (slotType)
        {
            case SlotType.RightHand:
            case SlotType.LeftHand:
                return equipModule.equipType == EquipmentType.Weapon;
            case SlotType.Head: return equipModule.equipType == EquipmentType.Head;
            case SlotType.Body: return equipModule.equipType == EquipmentType.Body;
            case SlotType.Pants: return equipModule.equipType == EquipmentType.Pants;
            case SlotType.Belt: return equipModule.equipType == EquipmentType.Belt;
            case SlotType.Backpack: return equipModule.equipType == EquipmentType.Backpack;
        }
        return false;
    }

    public void RefreshSlot()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<DraggableItem>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        if (InventoryUI.Instance == null || InventoryUI.Instance.equipmentManager == null || draggableItemPrefab == null) return;

        ItemInstance itemToSpawn = null;
        switch (slotType)
        {
            case SlotType.RightHand: itemToSpawn = InventoryUI.Instance.equipmentManager.rightHandSlot; break;
            case SlotType.LeftHand: itemToSpawn = InventoryUI.Instance.equipmentManager.leftHandSlot; break;
            case SlotType.Pants: itemToSpawn = InventoryUI.Instance.equipmentManager.pantsSlot; break;
            case SlotType.Belt: itemToSpawn = InventoryUI.Instance.equipmentManager.beltSlot; break;
            case SlotType.Backpack: itemToSpawn = InventoryUI.Instance.equipmentManager.backpackSlot; break;
        }

        if (itemToSpawn != null && itemToSpawn.itemDef != null)
        {
            GameObject itemObj = Instantiate(draggableItemPrefab, transform);
            DraggableItem dragScript = itemObj.GetComponent<DraggableItem>();

            dragScript.itemInstance = itemToSpawn;
            dragScript.currentSlot = null;
            dragScript.currentEquipmentSlot = this;

            RectTransform rect = itemObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
        }
    }

    public void ClearSlotMemory()
    {
        if (InventoryUI.Instance == null || InventoryUI.Instance.equipmentManager == null) return;
        switch (slotType)
        {
            case SlotType.RightHand: InventoryUI.Instance.equipmentManager.rightHandSlot = null; break;
            case SlotType.LeftHand: InventoryUI.Instance.equipmentManager.leftHandSlot = null; break;
            case SlotType.Belt: InventoryUI.Instance.equipmentManager.beltSlot = null; break;
            case SlotType.Pants:
                InventoryUI.Instance.equipmentManager.SpillAndClearGrids(InventoryUI.Instance.equipmentManager.GetActivePantsGrids());
                InventoryUI.Instance.equipmentManager.pantsSlot = null;
                break;
            case SlotType.Backpack:
                InventoryUI.Instance.equipmentManager.SpillAndClearGrids(InventoryUI.Instance.equipmentManager.GetActiveBackpackGrids());
                InventoryUI.Instance.equipmentManager.backpackSlot = null;
                break;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DraggableItem droppedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (droppedItem == null) return;

        bool success = false;
        switch (slotType)
        {
            case SlotType.RightHand: success = InventoryUI.Instance.equipmentManager.TryEquip(droppedItem.itemInstance, true); break;
            case SlotType.LeftHand: success = InventoryUI.Instance.equipmentManager.TryEquip(droppedItem.itemInstance, false); break;
            case SlotType.Pants: success = InventoryUI.Instance.equipmentManager.EquipPants(droppedItem.itemInstance); break;
            case SlotType.Belt: success = InventoryUI.Instance.equipmentManager.EquipBelt(droppedItem.itemInstance); break;
            case SlotType.Backpack: success = InventoryUI.Instance.equipmentManager.EquipBackpack(droppedItem.itemInstance); break;
        }

        if (success)
        {
            if (droppedItem.currentSlot != null)
            {
                droppedItem.currentSlot.parentPocket.logicGrid.RemoveItem(droppedItem.itemInstance);
                droppedItem.currentSlot = null;
            }

            if (droppedItem.currentEquipmentSlot != null && droppedItem.currentEquipmentSlot != this)
            {
                droppedItem.currentEquipmentSlot.ClearSlotMemory();
            }

            droppedItem.DestroyWorldLink(); // <- Ekstrakcja! Jedna linijka zamiast siedmiu!
            Destroy(droppedItem.gameObject);

            RefreshSlot();
            InventoryUI.Instance.RefreshUI();
        }
    }
}