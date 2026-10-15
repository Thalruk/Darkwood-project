using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("Backend References")]
    public EquipmentManager equipmentManager;

    [Header("UI References")]
    public GameObject inventoryWindow;
    public Transform pocketsContainer;
    public GameObject pocketPrefab;
    public GameObject slotPrefab;
    public GameObject draggableItemPrefab;
    public RectTransform leftPanelBounds;
    public GameObject containerGroupPrefab;
    [Header("Loot Scanner")]
    public Transform lootContainer;
    public float scanRadius = 3f;

    private bool isMenuOpen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (inventoryWindow != null) inventoryWindow.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I)) ToggleMenu();
    }

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        if (inventoryWindow != null) inventoryWindow.SetActive(isMenuOpen);
        if (isMenuOpen)
        {
            Canvas.ForceUpdateCanvases();
            RefreshUI();
        }
    }

    public void RefreshUI()
    {
        foreach (Transform child in pocketsContainer) Destroy(child.gameObject);
        if (lootContainer != null) foreach (Transform child in lootContainer) Destroy(child.gameObject);

        var activePantsGrids = equipmentManager.GetActivePantsGrids();
        if (activePantsGrids.Count > 0 && containerGroupPrefab != null)
        {
            GameObject pantsGroupObj = Instantiate(containerGroupPrefab, pocketsContainer);
            ContainerGroupUI pantsGroupUI = pantsGroupObj.GetComponent<ContainerGroupUI>();
            pantsGroupUI.Initialize("Pants");

            foreach (InventoryGrid logicGrid in activePantsGrids)
            {
                CreatePocketUI(logicGrid, pantsGroupUI.pocketsContainer);
            }
        }

        var activeBackpackGrids = equipmentManager.GetActiveBackpackGrids();
        if (activeBackpackGrids.Count > 0 && containerGroupPrefab != null)
        {
            GameObject backpackGroupObj = Instantiate(containerGroupPrefab, pocketsContainer);
            ContainerGroupUI backpackGroupUI = backpackGroupObj.GetComponent<ContainerGroupUI>();
            backpackGroupUI.Initialize("Backpack");

            foreach (InventoryGrid logicGrid in activeBackpackGrids)
            {
                CreatePocketUI(logicGrid, backpackGroupUI.pocketsContainer);
            }
        }


        if (lootContainer != null && equipmentManager.playerTransform != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(equipmentManager.playerTransform.position, scanRadius);

            if (hits.Length > 0)
            {
                Debug.Log($"[InventoryUI] Scanned {hits.Length} items on the ground.");

                float availableWidth = leftPanelBounds.rect.width - 20f;

                int dynamicColumns = Mathf.FloorToInt(availableWidth / 66f);
                if (dynamicColumns < 1) dynamicColumns = 1;

                Debug.Log($"[InventoryUI] Dynamic grid generated: {dynamicColumns} columns based on {availableWidth}px width.");

                InventoryGrid vicinityGrid = new InventoryGrid("Ground", dynamicColumns, 40);

                foreach (Collider2D hit in hits)
                {
                    WorldItem worldItem = hit.GetComponent<WorldItem>();
                    if (worldItem != null && worldItem.itemInstance != null)
                    {
                        worldItem.itemInstance.worldLink = worldItem;

                        bool placed = false;
                        for (int y = 0; y < vicinityGrid.Height && !placed; y++)
                        {
                            for (int x = 0; x < vicinityGrid.Width && !placed; x++)
                            {
                                if (vicinityGrid.TryPlaceItem(worldItem.itemInstance, x, y))
                                    placed = true;
                            }
                        }

                        if (!placed) Debug.LogWarning($"[InventoryUI] Vicinity grid is full! Cannot show {worldItem.itemInstance.itemDef.displayName}");
                    }
                }
                CreatePocketUI(vicinityGrid, lootContainer);
            }
        }

        Canvas.ForceUpdateCanvases();

        foreach (PocketUI pocketUI in pocketsContainer.GetComponentsInChildren<PocketUI>())
            pocketUI.SpawnSavedItems(draggableItemPrefab);

        if (lootContainer != null)
            foreach (PocketUI pocketUI in lootContainer.GetComponentsInChildren<PocketUI>())
                pocketUI.SpawnSavedItems(draggableItemPrefab);
    }

    private void CreatePocketUI(InventoryGrid logicGrid, Transform container)
    {
        GameObject newPocketObj = Instantiate(pocketPrefab, container);
        PocketUI pocketUI = newPocketObj.GetComponent<PocketUI>();

        GridLayoutGroup layout = pocketUI.gridContainer.GetComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = logicGrid.Width;

        GridSlotUI[,] uiSlots = new GridSlotUI[logicGrid.Width, logicGrid.Height];

        for (int y = 0; y < logicGrid.Height; y++)
        {
            for (int x = 0; x < logicGrid.Width; x++)
            {
                GameObject slotObj = Instantiate(slotPrefab, pocketUI.gridContainer);
                GridSlotUI slotUI = slotObj.GetComponent<GridSlotUI>();

                if (slotUI != null)
                {
                    slotUI.x = x;
                    slotUI.y = y;
                    slotUI.parentPocket = pocketUI;
                    uiSlots[x, y] = slotUI;
                }
            }
        }

        pocketUI.Initialize(logicGrid, uiSlots);
        pocketUI.RefreshEntireGrid();
    }
}