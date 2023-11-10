using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Assignment01.Controller;

namespace Assignment01.ScriptableObjects
{
    [CreateAssetMenu(fileName = "GridCreator", menuName = "Custom/Grid Creator")]
    public class GridCreatorSO : ScriptableObject
    {
        [Header("Grid Settings")]
        public GameObject TileA;
        public GameObject TileB;
        [Range(0, 10)]
        [SerializeField] int gridSize;

        [Header("Level Settings")]
        [LabelText("Level Index")]
        public int levelIndex;

        const float X_INTERVAL = 0.71f;
        const float Z_INTERVAL = 0.71f;

        private List<GameObject> gridObjects = new List<GameObject>();

        [Button("Create Grid", ButtonSizes.Large)]
        public void CreateGrid()
        {
            // Önceden oluşturulmuş objeleri temizle
            foreach (var obj in gridObjects)
            {
                if(obj != null)
                {
                    DestroyImmediate(obj);
                }
            }
            gridObjects.Clear();

            // Gridi oluşturma
            for (int x = 0; x < gridSize; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    GameObject toInstantiate = ((x + z) % 2 == 0) ? TileA : TileB;
                    GameObject instance = Instantiate(toInstantiate, new Vector3(x * X_INTERVAL, 0, z * Z_INTERVAL), Quaternion.identity);
                    gridObjects.Add(instance);
                }
            }
        }

#if UNITY_EDITOR
        [Button("Save Grid as Prefab", ButtonSizes.Large)]
        public void SaveGridAsPrefab()
        {
            string savePath = "Assets/_GameFolders/Prefabs/Levels/Level" + levelIndex + ".prefab";
            GameObject levelParent = new GameObject("Level" + levelIndex);
            LevelController controller = levelParent.AddComponent<LevelController>();  // LevelController scriptini ekleyin
            controller.gridSize = gridSize;  // Grid boyutunu ayarla

            foreach (var obj in gridObjects)
            {
                obj.transform.SetParent(levelParent.transform);
            }

            // Prefabı oluşturma ve kaydetme
            PrefabUtility.SaveAsPrefabAsset(levelParent, savePath);

            // Oluşturduğumuz parent objeyi sahneden sil
            DestroyImmediate(levelParent);
        }
#endif
        
    }
}
