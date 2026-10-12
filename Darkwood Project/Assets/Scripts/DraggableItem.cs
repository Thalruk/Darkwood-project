using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Referencja do pamięci")]
    public ItemInstance itemInstance;

    [Header("Wizualizacja (Tetris)")]
    public float cellSize = 64f;
    public float spacing = 2f;
    public GridSlotUI currentSlot;
    private Transform fallbackParent;
    private Vector3 fallbackPosition;

    public static DraggableItem currentlyDraggedItem;
    private Image rootImage;

    private void Awake()
    {
        rootImage = GetComponent<Image>();

        LayoutElement le = gameObject.GetComponent<LayoutElement>();
        if (le == null) le = gameObject.AddComponent<LayoutElement>();
        le.ignoreLayout = true;
    }

    private void Start()
    {
        BuildVisualShape();
    }

    private void BuildVisualShape()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // 1. Niewidzialny korzeń zawsze reprezentuje kratkę (0,0) i snapuje do myszki
        RectTransform rootRect = GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(cellSize, cellSize);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootImage.color = new Color(0, 0, 0, 0);

        if (itemInstance == null || itemInstance.itemDef == null) return;

        // 2. Tworzymy kontener dla grafiki
        GameObject visualObj = new GameObject("WeaponGraphic");
        visualObj.transform.SetParent(transform, false);
        RectTransform visualRect = visualObj.AddComponent<RectTransform>();

        // 3. Obliczamy CAŁKOWITY fizyczny rozmiar obszaru zajmowanego przez przedmiot
        // Bierzemy pod uwagę szerokość w kratkach + wszystkie przerwy między nimi
        float fullWidth = (itemInstance.itemDef.shapeWidth * cellSize) + ((itemInstance.itemDef.shapeWidth - 1) * spacing);
        float fullHeight = (itemInstance.itemDef.shapeHeight * cellSize) + ((itemInstance.itemDef.shapeHeight - 1) * spacing);

        // Ustawiamy mnożnik skali, jeśli chcesz mieć margines (np. 90% docelowego rozmiaru)
        float scale = 0.9f;
        float scaledWidth = fullWidth * scale;
        float scaledHeight = fullHeight * scale;

        // 4. Matematyka pozycjonowania
        // Obliczamy lewy górny róg KORZENIA (nasz fizyczny start)
        float startX = -cellSize / 2f;
        float startY = cellSize / 2f;

        // Obliczamy ile pustego miejsca (marginesu) powstaje z ucięcia 10% rozmiaru.
        // Dzielimy to na 2, żeby margines był po równo z obu stron.
        float marginX = (fullWidth - scaledWidth) / 2f;
        float marginY = (fullHeight - scaledHeight) / 2f;

        // 5. Aplikujemy wyliczone wartości
        visualRect.anchorMin = new Vector2(0.5f, 0.5f);
        visualRect.anchorMax = new Vector2(0.5f, 0.5f);

        // Zaczepiamy obrazek w jego lewym górnym rogu
        visualRect.pivot = new Vector2(0, 1);

        // Narzucamy wymiary odpowiadające pełnemu obszarowi broni
        visualRect.sizeDelta = new Vector2(scaledWidth, scaledHeight);

        // Przesuwamy go do lewego górnego rogu (0,0) + wciskamy lekko do środka marginesem
        visualRect.anchoredPosition = new Vector2(startX + marginX, startY - marginY);

        Image visualImage = visualObj.AddComponent<Image>();
        visualImage.raycastTarget = false;

        if (itemInstance.itemDef.icon != null)
        {
            visualImage.sprite = itemInstance.itemDef.icon;

            // To polecenie jest tutaj najważniejsze!
            // Widząc, że RectTransform (pudełko) zajmuje pełne np. 2x2 kratki, 
            // Unity automatycznie i idealnie wyśrodkuje pistolet względem CAŁEGO tego obszaru.
            visualImage.preserveAspect = true;
        }
        else
        {
            visualImage.color = new Color(0.9f, 0.4f, 0.4f, 0.8f);
        }
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

                currentSlot.parentPocket.logicGrid.TryPlaceItem(itemInstance, currentSlot.x, currentSlot.y);
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
}