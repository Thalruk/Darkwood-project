using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Referencje Backendowe")]
    public EquipmentManager equipmentManager;

    [Header("Referencje UI")]
    public Transform pocketsContainer;
    public GameObject pocketPrefab;
    public GameObject slotPrefab;

    public void RefreshUI()
    {
        foreach (Transform child in pocketsContainer)
        {
            Destroy(child.gameObject);
        }

        var activeGrids = equipmentManager.GetActivePantsGrids();

        foreach (InventoryGrid logicGrid in activeGrids)
        {
            GameObject newPocketObj = Instantiate(pocketPrefab, pocketsContainer);

            GridLayoutGroup layout = newPocketObj.GetComponent<GridLayoutGroup>();
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = logicGrid.Width;

            int totalSlots = logicGrid.Width * logicGrid.Height;
            for (int y = 0; y < logicGrid.Height; y++)
            {
                for (int x = 0; x < logicGrid.Width; x++)
                {
                    GameObject slotObj = Instantiate(slotPrefab, newPocketObj.transform);
                    GridSlotUI slotUI = slotObj.GetComponent<GridSlotUI>();
                    if (slotUI != null)
                    {
                        slotUI.x = x;
                        slotUI.y = y;
                        slotUI.parentGrid = logicGrid;
                    }
                }
            }
        }
    }
}