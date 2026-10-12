using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ItemData itemData = (ItemData)target;

        GUILayout.Space(15);
        GUILayout.Label("Wizualny Edytor Kształtu (Tetris)", EditorStyles.boldLabel);

        if (itemData.shapeGrid == null || itemData.shapeGrid.Length != itemData.shapeWidth * itemData.shapeHeight)
        {
            itemData.shapeGrid = new bool[itemData.shapeWidth * itemData.shapeHeight];
        }

        EditorGUI.BeginChangeCheck();

        for (int y = 0; y < itemData.shapeHeight; y++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            for (int x = 0; x < itemData.shapeWidth; x++)
            {
                int index = y * itemData.shapeWidth + x;
                itemData.shapeGrid[index] = EditorGUILayout.Toggle(itemData.shapeGrid[index], GUILayout.Width(20));
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        if (EditorGUI.EndChangeCheck())
        {
            List<Vector2Int> newShape = new List<Vector2Int>();

            for (int y = 0; y < itemData.shapeHeight; y++)
            {
                for (int x = 0; x < itemData.shapeWidth; x++)
                {
                    if (itemData.shapeGrid[y * itemData.shapeWidth + x])
                    {
                        newShape.Add(new Vector2Int(x, y));
                    }
                }
            }

            itemData.gridShape = newShape.ToArray();
            EditorUtility.SetDirty(itemData);
        }
    }
}