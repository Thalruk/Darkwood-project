using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Podstawowe Informacje")]
    public string itemID;
    public string displayName;
    public Sprite icon;

    [Header("Kształt Ekwipunku (Tetris)")]
    public Vector2Int[] gridShape = new Vector2Int[] { Vector2Int.zero };

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