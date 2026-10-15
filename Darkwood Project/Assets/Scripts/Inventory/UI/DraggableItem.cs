using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Memory Reference")]
    public ItemInstance itemInstance;

    [Header("Visualization Settings")]
    public float cellSize = 64f;
    public float spacing = 2f;

    [HideInInspector] public GridSlotUI currentSlot;
    [HideInInspector] public EquipmentSlotUI currentEquipmentSlot;

    private Transform fallbackParent;
    private Vector3 fallbackPosition;
    private Image rootImage;

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

        GameObject visualObj = new GameObject("WeaponGraphic");
        visualObj.transform.SetParent(transform, false);
        RectTransform visualRect = visualObj.AddComponent<RectTransform>();

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
        currentlyDraggedItem = this;
        fallbackParent = transform.parent;
        fallbackPosition = transform.position;

        Canvas canvas = GetComponentInParent<Canvas>();
        transform.SetParent(canvas != null ? canvas.transform : transform.root);
        transform.SetAsLastSibling();

        rootImage.raycastTarget = false;

        if (currentSlot != null)
        {
            currentSlot.parentPocket.logicGrid.RemoveItem(itemInstance);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
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
                Debug.Log($"[DraggableItem] Invalid drop, returning {itemInstance.itemDef.displayName} to previous spot.");
            }
        }
    }
}