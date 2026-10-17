using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public static event Action<ItemInstance> OnItemDragStart;
    public static event Action OnItemDragEnd;

    [Header("Memory Reference")]
    public ItemInstance itemInstance;

    [Header("Visualization Settings")]
    public float cellSize = 64f;
    public float spacing = 2f;

    [HideInInspector] public GridSlotUI currentSlot;
    [HideInInspector] public EquipmentSlotUI currentEquipmentSlot;
    [HideInInspector] public Vector2Int grabbedCellOffset;

    private Transform fallbackParent;
    private Vector3 fallbackPosition;
    private int originalRotationStep;
    private Image rootImage;
    private Vector3 dragOffset;

    public static DraggableItem currentlyDraggedItem;

    private void Awake()
    {
        rootImage = GetComponent<Image>();
        LayoutElement le = gameObject.GetComponent<LayoutElement>();
        if (le == null)
        {
            le = gameObject.AddComponent<LayoutElement>();
        }
        le.ignoreLayout = true;
    }

    private void Update()
    {
        if (currentlyDraggedItem == this && Input.GetKeyDown(KeyCode.R))
        {
            RotateDraggedItem();
        }
    }

    private void RotateDraggedItem()
    {
        if (itemInstance == null || itemInstance.itemDef == null)
        {
            return;
        }

        if (GridSlotUI.currentlyHoveredSlot != null)
        {
            GridSlotUI slot = GridSlotUI.currentlyHoveredSlot;
            slot.parentPocket.ClearHighlight(itemInstance, slot.x - grabbedCellOffset.x, slot.y - grabbedCellOffset.y);
        }

        itemInstance.RotateClockwise();
        BuildVisualShape();

        Vector2Int newDims = itemInstance.GetDimensions();
        grabbedCellOffset = new Vector2Int(newDims.x / 2, newDims.y / 2);

        Canvas canvas = GetComponentInParent<Canvas>();
        Transform rootTransform = canvas != null ? canvas.transform : transform.root;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rootTransform as RectTransform,
            Input.mousePosition,
            canvas != null ? canvas.worldCamera : null,
            out Vector3 worldMousePos
        );

        float scaleFactor = canvas != null ? canvas.scaleFactor : 1f;
        float step = (cellSize + spacing) * scaleFactor;

        float centerCellX = (newDims.x - 1) * 0.5f;
        float centerCellY = (newDims.y - 1) * 0.5f;

        float shiftX = (centerCellX - grabbedCellOffset.x) * step;
        float shiftY = (grabbedCellOffset.y - centerCellY) * step;

        dragOffset = new Vector3(shiftX, shiftY, 0f);
        transform.position = worldMousePos + dragOffset;

        if (GridSlotUI.currentlyHoveredSlot != null)
        {
            GridSlotUI slot = GridSlotUI.currentlyHoveredSlot;
            slot.parentPocket.HighlightShape(itemInstance, slot.x - grabbedCellOffset.x, slot.y - grabbedCellOffset.y);
        }
    }

    public void BuildVisualShape()
    {
        if (rootImage == null)
        {
            rootImage = GetComponent<Image>();
        }

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        RectTransform rootRect = GetComponent<RectTransform>();
        rootImage.color = new Color(0, 0, 0, 0);

        if (itemInstance == null || itemInstance.itemDef == null)
        {
            return;
        }

        GameObject visualObj = new GameObject("ItemGraphic");
        visualObj.transform.SetParent(transform, false);
        RectTransform visualRect = visualObj.AddComponent<RectTransform>();

        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);

        visualRect.anchorMin = new Vector2(0.5f, 0.5f);
        visualRect.anchorMax = new Vector2(0.5f, 0.5f);
        visualRect.pivot = new Vector2(0.5f, 0.5f);

        if (currentEquipmentSlot != null && currentlyDraggedItem != this)
        {
            RectTransform slotRect = currentEquipmentSlot.GetComponent<RectTransform>();
            float equipSize = (slotRect != null && slotRect.rect.width > 0) ? slotRect.rect.width : 128f;

            rootRect.sizeDelta = new Vector2(equipSize, equipSize);
            rootRect.anchoredPosition = Vector2.zero;

            visualRect.sizeDelta = new Vector2(equipSize * 0.9f, equipSize * 0.9f);
            visualRect.anchoredPosition = Vector2.zero;
            visualRect.localEulerAngles = Vector3.zero;
        }
        else
        {
            Vector2Int currentDims = itemInstance.GetDimensions();
            float fullWidth = (currentDims.x * cellSize) + ((currentDims.x - 1) * spacing);
            float fullHeight = (currentDims.y * cellSize) + ((currentDims.y - 1) * spacing);

            rootRect.sizeDelta = new Vector2(fullWidth, fullHeight);

            Vector2Int baseDims = itemInstance.GetBaseDimensions();
            float baseWidth = (baseDims.x * cellSize) + ((baseDims.x - 1) * spacing);
            float baseHeight = (baseDims.y * cellSize) + ((baseDims.y - 1) * spacing);

            float scale = 0.9f;
            visualRect.sizeDelta = new Vector2(baseWidth * scale, baseHeight * scale);
            visualRect.anchoredPosition = Vector2.zero;
            visualRect.localEulerAngles = new Vector3(0f, 0f, -90f * itemInstance.rotationStep);
        }

        Image visualImage = visualObj.AddComponent<Image>();
        visualImage.raycastTarget = false;

        if (itemInstance.itemDef.icon != null)
        {
            visualImage.sprite = itemInstance.itemDef.icon;
            visualImage.preserveAspect = true;
        }
        else
        {
            visualImage.color = new Color(0.9f, 0.4f, 0.4f, 0.8f);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        currentlyDraggedItem = this;
        fallbackParent = transform.parent;
        fallbackPosition = transform.position;
        originalRotationStep = itemInstance.rotationStep;

        RectTransform rectTransform = GetComponent<RectTransform>();

        if (currentEquipmentSlot != null || currentSlot == null)
        {
            grabbedCellOffset = Vector2Int.zero;
            if (currentEquipmentSlot != null)
            {
                BuildVisualShape();
            }
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localMousePos
            );

            Vector2Int dims = itemInstance.GetDimensions();
            float fromLeft = localMousePos.x + (rectTransform.rect.width * 0.5f);
            float fromTop = (rectTransform.rect.height * 0.5f) - localMousePos.y;

            float step = cellSize + spacing;
            int offsetX = Mathf.Clamp(Mathf.FloorToInt(fromLeft / step), 0, dims.x - 1);
            int offsetY = Mathf.Clamp(Mathf.FloorToInt(fromTop / step), 0, dims.y - 1);

            grabbedCellOffset = new Vector2Int(offsetX, offsetY);
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        Transform rootTransform = canvas != null ? canvas.transform : transform.root;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rootTransform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldMousePos
        );

        dragOffset = transform.position - worldMousePos;

        transform.SetParent(rootTransform);
        transform.SetAsLastSibling();

        rootImage.raycastTarget = false;

        if (currentSlot != null)
        {
            currentSlot.parentPocket.logicGrid.RemoveItem(itemInstance);
            currentSlot.parentPocket.RefreshEntireGrid();
        }

        OnItemDragStart?.Invoke(itemInstance);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            transform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPoint
        );
        transform.position = worldPoint + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        OnItemDragEnd?.Invoke();

        if (this == null || gameObject == null)
        {
            return;
        }

        rootImage.raycastTarget = true;
        currentlyDraggedItem = null;

        Canvas canvas = GetComponentInParent<Canvas>();
        Transform rootTransform = canvas != null ? canvas.transform : transform.root;

        if (transform.parent == rootTransform)
        {
            itemInstance.rotationStep = originalRotationStep;
            BuildVisualShape();

            if (currentSlot != null)
            {
                currentSlot.parentPocket.logicGrid.TryPlaceItem(itemInstance, currentSlot.x, currentSlot.y);
                transform.SetParent(currentSlot.transform.parent);
                transform.position = currentSlot.parentPocket.GetItemCenterPosition(itemInstance, currentSlot.x, currentSlot.y);
                transform.SetAsLastSibling();
                currentSlot.parentPocket.RefreshEntireGrid();
            }
            else
            {
                transform.SetParent(fallbackParent);
                transform.position = fallbackPosition;
                transform.SetAsLastSibling();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            EquipmentManager em = InventoryUI.Instance.equipmentManager;

            if (currentEquipmentSlot != null)
            {
                currentEquipmentSlot.ClearSlotMemory();
                em.HandleUnequippedItem(itemInstance);

                DestroyWorldLink();
                Destroy(gameObject);
                InventoryUI.Instance.RefreshUI();
                return;
            }

            EquippableModule equipModule = itemInstance.itemDef.GetModule<EquippableModule>();
            if (equipModule == null)
            {
                return;
            }

            bool success = false;
            switch (equipModule.equipType)
            {
                case EquipmentType.Weapon:
                    if (!em.HasValidItem(em.rightHandSlot))
                    {
                        success = em.TryEquip(itemInstance, true);
                    }
                    else if (!em.HasValidItem(em.leftHandSlot) && !equipModule.isTwoHanded)
                    {
                        success = em.TryEquip(itemInstance, false);
                    }
                    else
                    {
                        success = em.TryEquip(itemInstance, true);
                    }
                    break;

                case EquipmentType.Pants:
                    success = em.EquipPants(itemInstance);
                    break;
                case EquipmentType.Belt:
                    success = em.EquipBelt(itemInstance);
                    break;
                case EquipmentType.Backpack:
                    success = em.EquipBackpack(itemInstance);
                    break;
            }

            if (success)
            {
                if (currentSlot != null)
                {
                    currentSlot.parentPocket.logicGrid.RemoveItem(itemInstance);
                }

                DestroyWorldLink();
                Destroy(gameObject);
                InventoryUI.Instance.RefreshUI();
            }
        }
    }

    public void DestroyWorldLink()
    {
        if (itemInstance != null && itemInstance.worldLink != null)
        {
            Collider2D[] cols = itemInstance.worldLink.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D c in cols)
            {
                c.enabled = false;
            }

            Destroy(itemInstance.worldLink.gameObject);
            itemInstance.worldLink = null;
        }
    }

    private void OnDestroy()
    {
        if (currentlyDraggedItem == this)
        {
            OnItemDragEnd?.Invoke();
            currentlyDraggedItem = null;
        }
    }
}