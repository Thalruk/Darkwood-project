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
        if (backgroundImage == null)
        {
            return;
        }

        backgroundImage.color = CanAcceptItem(item) ? defaultColor : disabledColor;
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
        if (item == null || item.itemDef == null)
        {
            return false;
        }

        EquippableModule equipModule = item.itemDef.GetModule<EquippableModule>();
        if (equipModule == null)
        {
            return false;
        }

        switch (slotType)
        {
            case SlotType.RightHand:
            case SlotType.LeftHand:
                return equipModule.equipType == EquipmentType.Weapon;
            case SlotType.Head:
                return equipModule.equipType == EquipmentType.Head;
            case SlotType.Body:
                return equipModule.equipType == EquipmentType.Body;
            case SlotType.Pants:
                return equipModule.equipType == EquipmentType.Pants;
            case SlotType.Belt:
                return equipModule.equipType == EquipmentType.Belt;
            case SlotType.Backpack:
                return equipModule.equipType == EquipmentType.Backpack;
        }

        return false;
    }

    /// <summary>
    /// Rebuilds the visual item representation inside this equipment slot based on EquipmentManager state.
    /// </summary>
    public void RefreshSlot()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<DraggableItem>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        if (InventoryUI.Instance == null || InventoryUI.Instance.equipmentManager == null || draggableItemPrefab == null)
        {
            return;
        }

        EquipmentManager em = InventoryUI.Instance.equipmentManager;
        ItemInstance itemToSpawn = null;

        switch (slotType)
        {
            case SlotType.RightHand:
                itemToSpawn = em.rightHandSlot;
                break;
            case SlotType.LeftHand:
                itemToSpawn = em.leftHandSlot;
                break;
            case SlotType.Head:
                itemToSpawn = em.headSlot;
                break;
            case SlotType.Body:
                itemToSpawn = em.bodySlot;
                break;
            case SlotType.Pants:
                itemToSpawn = em.pantsSlot;
                break;
            case SlotType.Belt:
                itemToSpawn = em.beltSlot;
                break;
            case SlotType.Backpack:
                itemToSpawn = em.backpackSlot;
                break;
        }

        if (itemToSpawn != null && itemToSpawn.itemDef != null)
        {
            GameObject itemObj = Instantiate(draggableItemPrefab, transform);
            DraggableItem dragScript = itemObj.GetComponent<DraggableItem>();

            dragScript.itemInstance = itemToSpawn;
            dragScript.currentSlot = null;
            dragScript.currentEquipmentSlot = this;

            dragScript.BuildVisualShape();
        }
    }

    /// <summary>
    /// Clears the backend reference for this slot in EquipmentManager (spilling container contents if applicable).
    /// </summary>
    public void ClearSlotMemory()
    {
        if (InventoryUI.Instance == null || InventoryUI.Instance.equipmentManager == null)
        {
            return;
        }

        EquipmentManager em = InventoryUI.Instance.equipmentManager;

        switch (slotType)
        {
            case SlotType.RightHand:
                em.ClearHandSlot(true);
                break;
            case SlotType.LeftHand:
                em.ClearHandSlot(false);
                break;
            case SlotType.Head:
                em.headSlot = null;
                break;
            case SlotType.Backpack:
                em.SpillAndClearGrids(em.GetActiveBackpackGrids());
                em.backpackSlot = null;
                break;
            case SlotType.Body:
                em.SpillAndClearGrids(em.GetActiveBodyGrids());
                em.bodySlot = null;
                break;
            case SlotType.Belt:
                em.SpillAndClearGrids(em.GetActiveBeltGrids());
                em.beltSlot = null;
                break;
            case SlotType.Pants:
                em.SpillAndClearGrids(em.GetActivePantsGrids());
                em.pantsSlot = null;
                break;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            return;
        }

        DraggableItem droppedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (droppedItem == null)
        {
            return;
        }

        EquipmentManager em = InventoryUI.Instance.equipmentManager;
        bool success = false;

        switch (slotType)
        {
            case SlotType.RightHand:
                success = em.TryEquip(droppedItem.itemInstance, true);
                break;
            case SlotType.LeftHand:
                success = em.TryEquip(droppedItem.itemInstance, false);
                break;
            case SlotType.Head:
                success = em.EquipHead(droppedItem.itemInstance);
                break;
            case SlotType.Body:
                success = em.EquipBody(droppedItem.itemInstance);
                break;
            case SlotType.Pants:
                success = em.EquipPants(droppedItem.itemInstance);
                break;
            case SlotType.Belt:
                success = em.EquipBelt(droppedItem.itemInstance);
                break;
            case SlotType.Backpack:
                success = em.EquipBackpack(droppedItem.itemInstance);
                break;
        }

        if (success)
        {
            if (droppedItem.currentSlot != null)
            {
                droppedItem.currentSlot.parentPocket.logicGrid.RemoveItem(droppedItem.itemInstance);
                droppedItem.currentSlot = null;
            }

            droppedItem.DestroyWorldLink();
            Destroy(droppedItem.gameObject);

            InventoryUI.Instance.RefreshUI();
        }
    }
}