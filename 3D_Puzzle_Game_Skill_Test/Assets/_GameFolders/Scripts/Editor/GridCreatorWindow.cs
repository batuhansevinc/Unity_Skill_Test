using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Assignment01.Controller;

public class GridCreatorWindow : EditorWindow
{
    public GameObject TileA; // Siyah
    public GameObject TileB; // Beyaz

    public int gridSize = 4;
    public string levelName = "Level1";

    public LevelReferencesSO levelReferences; // Referanslar için eklenen Scriptable Object

    private const float X_INTERVAL = 0.71f;
    private const float Z_INTERVAL = 0.71f;

    [MenuItem("Tools/Grid Creator")]
    public static void ShowWindow()
    {
        GetWindow<GridCreatorWindow>("Grid Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);

        gridSize = EditorGUILayout.IntSlider("Grid Size", gridSize, 2, 10);
        TileA = (GameObject)EditorGUILayout.ObjectField("Tile A", TileA, typeof(GameObject), false);
        TileB = (GameObject)EditorGUILayout.ObjectField("Tile B", TileB, typeof(GameObject), false);
        levelReferences = (LevelReferencesSO)EditorGUILayout.ObjectField("Level References", levelReferences, typeof(LevelReferencesSO), false); // GUI üzerinde LevelReferencesSO için bir alan oluşturduk.
        levelName = EditorGUILayout.TextField("Level Name", levelName);

        if (GUILayout.Button("Create Grid"))
        {
            CreateGrid();
        }

        if (GUILayout.Button("Save Grid"))
        {
            SaveGrid(levelName);
        }
    }

    void CreateGrid()
    {
        GameObject levelParent = new GameObject(levelName);
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                GameObject toInstantiate = ((x + z) % 2 == 0) ? TileA : TileB;
                GameObject instance = Instantiate(toInstantiate, new Vector3(x * X_INTERVAL, 0, z * Z_INTERVAL), Quaternion.identity);
                instance.transform.SetParent(levelParent.transform);
            }
        }

        if (!levelParent.GetComponent<LevelController>())
        {
            LevelController controller = levelParent.AddComponent<LevelController>(); 

            // Referansları set edelim.
            if (levelReferences != null)
            {
                //controller.levelReferences = levelReferences;
                controller.gridSize = gridSize;

            }
            else
            {
                Debug.LogError("LevelReferencesSO is not set in the Grid Creator Window.");
            }
        }
    }

    void SaveGrid(string levelName)
    {
        GameObject levelParent = GameObject.Find(levelName);
        if (levelParent)
        {
            string path = "Assets/_GameFolders/Prefabs/Levels/" + levelName + ".prefab";
            if (!AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)))
            {
                PrefabUtility.SaveAsPrefabAsset(levelParent, path);
                DestroyImmediate(levelParent);
            }
            else
            {
                Debug.LogError("A prefab with this name already exists. Please choose a different name or delete the existing one.");
            }
        }
        else
        {
            Debug.LogError("Level Parent not found. Make sure you've created the grid first.");
        }
    }
}
