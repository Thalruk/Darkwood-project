using System.Collections.Generic;
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
        ClearContainer(pocketsContainer);
        if (lootContainer != null)
        {
            ClearContainer(lootContainer);
        }

        BuildPocketGroup("Backpack", equipmentManager.GetActiveBackpackGrids());
        BuildPocketGroup("Body", equipmentManager.GetActiveBodyGrids());
        BuildPocketGroup("Belt", equipmentManager.GetActiveBeltGrids());
        BuildPocketGroup("Pants", equipmentManager.GetActivePantsGrids());

        if (lootContainer != null && equipmentManager.playerTransform != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(equipmentManager.playerTransform.position, scanRadius);

            if (hits.Length > 0)
            {
                float availableWidth = leftPanelBounds != null ? leftPanelBounds.rect.width - 36f : 300f;
                int dynamicColumns = Mathf.FloorToInt(availableWidth / 66f);
                if (dynamicColumns < 1)
                {
                    dynamicColumns = 1;
                }

                InventoryGrid vicinityGrid = new InventoryGrid(dynamicColumns, 40, true);

                foreach (Collider2D hit in hits)
                {
                    WorldItem worldItem = hit.GetComponent<WorldItem>();
                    if (worldItem != null && worldItem.itemInstance != null && worldItem.itemInstance.itemDef != null)
                    {
                        worldItem.itemInstance.worldLink = worldItem;

                        bool placed = false;
                        for (int y = 0; y < vicinityGrid.Height && !placed; y++)
                        {
                            for (int x = 0; x < vicinityGrid.Width && !placed; x++)
                            {
                                if (vicinityGrid.TryPlaceItem(worldItem.itemInstance, x, y))
                                {
                                    placed = true;
                                }
                            }
                        }

                        if (!placed)
                        {
                            Debug.LogWarning($"[InventoryUI] Vicinity grid is full! Cannot show: {worldItem.itemInstance.itemDef.displayName}");
                        }
                    }
                }
                CreatePocketUI(vicinityGrid, lootContainer);
            }
        }

        Canvas.ForceUpdateCanvases();
        if (pocketsContainer is RectTransform pocketsRect)
        {
            RebuildLayoutBottomUp(pocketsRect);
        }
        if (lootContainer is RectTransform lootRect)
        {
            RebuildLayoutBottomUp(lootRect);
        }

        foreach (PocketUI pocketUI in pocketsContainer.GetComponentsInChildren<PocketUI>())
        {
            pocketUI.SpawnSavedItems(draggableItemPrefab);
        }

        if (lootContainer != null)
        {
            foreach (PocketUI pocketUI in lootContainer.GetComponentsInChildren<PocketUI>())
            {
                pocketUI.SpawnSavedItems(draggableItemPrefab);
            }
        }

        if (inventoryWindow != null)
        {
            foreach (EquipmentSlotUI equipSlot in inventoryWindow.GetComponentsInChildren<EquipmentSlotUI>(true))
            {
                equipSlot.RefreshSlot();
            }
        }
    }

    private void ClearContainer(Transform container)
    {
        if (container == null) return;
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            child.SetParent(null);
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    private void RebuildLayoutBottomUp(RectTransform root)
    {
        if (root == null) return;
        foreach (RectTransform child in root) RebuildLayoutBottomUp(child);
        LayoutRebuilder.ForceRebuildLayoutImmediate(root);
    }

    private void BuildPocketGroup(string title, List<InventoryGrid> grids)
    {
        if (grids.Count == 0 || containerGroupPrefab == null) return;

        GameObject groupObj = Instantiate(containerGroupPrefab, pocketsContainer);
        ContainerGroupUI groupUI = groupObj.GetComponent<ContainerGroupUI>();

        if (groupUI != null)
        {
            groupUI.Initialize(title);
            foreach (InventoryGrid logicGrid in grids)
            {
                CreatePocketUI(logicGrid, groupUI.pocketsContainer);
            }
        }
        else
        {
            Debug.LogError($"[InventoryUI] BŁĄD: Twój prefab dla {title} nie ma podpiętego skryptu ContainerGroupUI!");
        }
    }
    private void CreatePocketUI(InventoryGrid logicGrid, Transform container)
    {
        if (container == null)
        {
            Debug.LogError("[InventoryUI] BŁĄD: 'pocketsContainer' nie jest podpięty wewnątrz skryptu w ContainerGroupPrefab!");
            return;
        }

        GameObject newPocketObj = Instantiate(pocketPrefab, container);
        PocketUI pocketUI = newPocketObj.GetComponent<PocketUI>();

        if (pocketUI == null)
        {
            Debug.LogError("[InventoryUI] BŁĄD: Twój prefab 'PocketPrefab' nie ma przypisanego skryptu PocketUI!");
            return;
        }

        if (pocketUI.gridContainer == null)
        {
            Debug.LogError("[InventoryUI] BŁĄD: 'gridContainer' nie jest podpięty wewnątrz skryptu w PocketPrefab!");
            return;
        }

        GridLayoutGroup layout = pocketUI.gridContainer.GetComponent<GridLayoutGroup>();
        if (layout != null)
        {
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = logicGrid.Width;
        }
        else Debug.LogError("[InventoryUI] BŁĄD: Brakuje komponentu GridLayoutGroup wewnątrz GridContainer w PocketPrefab!");

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