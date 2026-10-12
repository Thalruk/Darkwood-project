using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Podstawowe Informacje")]
    public string itemID;
    public string displayName;
    public Sprite icon;

    [Header("Wymiary Obszaru Kształtu")]
    public int shapeWidth = 1;
    public int shapeHeight = 1;

    [HideInInspector] public bool[] shapeGrid = new bool[1] { true };
    [HideInInspector] public Vector2Int[] gridShape = new Vector2Int[] { Vector2Int.zero };

    [Header("Moduły Przedmiotu")]
    [SerializeReference, SubclassSelector]
    public List<ItemModule> modules = new List<ItemModule>();
    public T GetModule<T>() where T : ItemModule
    {
        foreach (var module in modules)
        {
            if (module is T)
            {
                return (T)module;
            }
        }
        return null;
    }
}