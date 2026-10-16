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
    public float cellSize = 64;
    public float spacing = 2f;

    [HideInInspector] public GridSlotUI currentSlot;
    [HideInInspector] public EquipmentSlotUI currentEquipmentSlot;

    private Transform fallbackParent;
    private Vector3 fallbackPosition;
    private Image rootImage;
    private Vector3 dragOffset;

    public static DraggableItem currentlyDraggedItem;

    private void Awake()
    {
        rootImage = GetComponent<Image>();
        LayoutElement le = gameObject.GetComponent<LayoutElement>();
        if (le == null) le = gameObject.AddComponent<LayoutElement>();
        le.ignoreLayout = true;
    }

    private void Start() { BuildVisualShape(); }

    private void BuildVisualShape()
    {
        foreach (Transform child in transform) { Destroy(child.gameObject); }

        RectTransform rootRect = GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(cellSize, cellSize);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootImage.color = new Color(0, 0, 0, 0);

        if (itemInstance == null || itemInstance.itemDef == null) return;

        GameObject visualObj = new GameObject("ItemGraphic");
        visualObj.transform.SetParent(transform, false);
        RectTransform visualRect = visualObj.AddComponent<RectTransform>();

        if (currentEquipmentSlot != null)
        {
            RectTransform slotRect = currentEquipmentSlot.GetComponent<RectTransform>();
            float equipSize = slotRect != null ? slotRect.rect.width : 128f;

            visualRect.anchorMin = new Vector2(0.5f, 0.5f);
            visualRect.anchorMax = new Vector2(0.5f, 0.5f);
            visualRect.pivot = new Vector2(0.5f, 0.5f);
            visualRect.sizeDelta = new Vector2(equipSize * 0.9f, equipSize * 0.9f);
            visualRect.anchoredPosition = Vector2.zero;
        }
        else
        {
            float fullWidth = (itemInstance.itemDef.shapeWidth * cellSize) + ((itemInstance.itemDef.shapeWidth - 1) * spacing);
            float fullHeight = (itemInstance.itemDef.shapeHeight * cellSize) + ((itemInstance.itemDef.shapeHeight - 1) * spacing);

            float scale = 0.9f;
            float startX = -cellSize / 2f;
            float startY = cellSize / 2f;
            float marginX = (fullWidth - (fullWidth * scale)) / 2f;
            float marginY = (fullHeight - (fullHeight * scale)) / 2f;

            visualRect.anchorMin = new Vector2(0.5f, 0.5f);
            visualRect.anchorMax = new Vector2(0.5f, 0.5f);
            visualRect.pivot = new Vector2(0, 1);
            visualRect.sizeDelta = new Vector2(fullWidth * scale, fullHeight * scale);
            visualRect.anchoredPosition = new Vector2(startX + marginX, startY - marginY);
        }

        Image visualImage = visualObj.AddComponent<Image>();
        visualImage.raycastTarget = false;

        if (itemInstance.itemDef.icon != null)
        {
            visualImage.sprite = itemInstance.itemDef.icon;
            visualImage.preserveAspect = true;
        }
        else { visualImage.color = new Color(0.9f, 0.4f, 0.4f, 0.8f); }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        currentlyDraggedItem = this;
        fallbackParent = transform.parent;
        fallbackPosition = transform.position;

        dragOffset = transform.position - (Vector3)eventData.position;

        Canvas canvas = GetComponentInParent<Canvas>();
        transform.SetParent(canvas != null ? canvas.transform : transform.root);
        transform.SetAsLastSibling();

        rootImage.raycastTarget = false;

        if (currentSlot != null)
        {
            currentSlot.parentPocket.logicGrid.RemoveItem(itemInstance);
        }

        OnItemDragStart?.Invoke(itemInstance);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        transform.position = (Vector3)eventData.position + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        OnItemDragEnd?.Invoke();

        if (this == null || gameObject == null) return;

        rootImage.raycastTarget = true;
        currentlyDraggedItem = null;

        Canvas canvas = GetComponentInParent<Canvas>();
        Transform rootTransform = canvas != null ? canvas.transform : transform.root;

        if (transform.parent == rootTransform)
        {
            if (currentSlot != null)
            {
                transform.SetParent(currentSlot.transform.parent);
                transform.position = currentSlot.transform.position;
                transform.SetAsLastSibling();
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
            Debug.Log($"[DraggableItem] Prawy Przycisk Myszy kliknięty na: {itemInstance.itemDef.displayName}");
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