using UnityEngine;
using System.Collections.Generic;
using BufoGames.Constants;
using BufoGames.Controller;
using BufoGames.Data;
using BufoGames.Pieces;
using BufoGames.Tiles;

namespace BufoGames.Generation
{
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private ThemeDataSO defaultTheme;

        private List<PieceBase> _spawnedPieces = new List<PieceBase>();
        private Dictionary<(int x, int z), TileController> _tileMap = new Dictionary<(int, int), TileController>();
        private SourceController _sourceController;
        private DestinationController _destinationController;

        public List<PieceBase> SpawnedPieces => _spawnedPieces;
        public SourceController SourceController => _sourceController;
        public DestinationController DestinationController => _destinationController;

        public void SetDefaultTheme(ThemeDataSO theme)
        {
            defaultTheme = theme;
        }

        public GameObject GenerateLevel(LevelDataSO levelData)
        {
            if (levelData == null) return null;

            _spawnedPieces.Clear();
            _tileMap.Clear();
            _sourceController = null;
            _destinationController = null;

            GameObject levelRoot = new GameObject($"Level_{levelData.levelIndex}");

            ThemeDataSO theme = defaultTheme;
            if (theme == null) return levelRoot;

            GameObject gridParent = new GameObject("Grid");
            gridParent.transform.SetParent(levelRoot.transform);
            GenerateTiles(gridParent, levelData, theme);

            GameObject piecesParent = new GameObject("Pieces");
            piecesParent.transform.SetParent(levelRoot.transform);
            SpawnPieces(piecesParent, levelData, theme);

            LevelController controller = levelRoot.AddComponent<LevelController>();
            controller.Initialize(levelData.gridSize, _spawnedPieces, _sourceController, _destinationController);

            return levelRoot;
        }

        private void GenerateTiles(GameObject parent, LevelDataSO levelData, ThemeDataSO theme)
        {
            int gridSize = levelData.gridSize;

            for (int x = 0; x < gridSize; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    GameObject tilePrefab = ((x + z) % 2 == 0) ? theme.tileAPrefab : theme.tileBPrefab;
                    if (tilePrefab == null) continue;

                    Vector3 position = GetGridPosition(x, z);
                    GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, parent.transform);
                    tile.name = $"Tile_{x}_{z}";

                    TileController tileController = tile.AddComponent<TileController>();
                    _tileMap[(x, z)] = tileController;
                }
            }
        }

        private void SpawnPieces(GameObject parent, LevelDataSO levelData, ThemeDataSO theme)
        {
            foreach (var pieceData in levelData.pieces)
            {
                SpawnPiece(parent, pieceData, theme);
            }
        }

        private void SpawnPiece(GameObject parent, PieceData pieceData, ThemeDataSO theme)
        {
            GameObject prefab = theme.GetPipePrefab(pieceData.pieceType);
            if (prefab == null) return;

            Vector3 position = GetGridPosition(pieceData.x, pieceData.z);
            GameObject instance = Instantiate(prefab, position, Quaternion.identity, parent.transform);
            instance.name = $"{pieceData.pieceType}_{pieceData.x}_{pieceData.z}";

            ConfigurePiece(instance, pieceData);
        }

        private void ConfigurePiece(GameObject instance, PieceData pieceData)
        {
            switch (pieceData.pieceType)
            {
                case PieceType.Source:
                    ConfigureSource(instance, pieceData);
                    break;
                case PieceType.Destination:
                    ConfigureDestination(instance, pieceData);
                    break;
                default:
                    ConfigurePipe(instance, pieceData);
                    break;
            }
        }

        private void ConfigureSource(GameObject instance, PieceData pieceData)
        {
            SourceController source = instance.GetComponent<SourceController>();
            if (source == null)
            {
                source = instance.AddComponent<SourceController>();
            }

            source.SetGridPosition(pieceData.x, pieceData.z);
            AssignTileController(source, pieceData.x, pieceData.z);

            Transform visual = FindVisualChild(instance.transform);
            if (visual != null)
            {
                source.SetVisualTransform(visual);
            }

            source.SetInitialRotation(pieceData.rotation);
            EnsureCollider(instance);

            _sourceController = source;
            _spawnedPieces.Add(source);
        }

        private void ConfigureDestination(GameObject instance, PieceData pieceData)
        {
            DestinationController dest = instance.GetComponent<DestinationController>();
            if (dest == null)
            {
                dest = instance.AddComponent<DestinationController>();
            }

            dest.SetGridPosition(pieceData.x, pieceData.z);
            AssignTileController(dest, pieceData.x, pieceData.z);
            dest.SetInitialRotation(pieceData.rotation);
            EnsureCollider(instance);

            _destinationController = dest;
            _spawnedPieces.Add(dest);
        }

        private void ConfigurePipe(GameObject instance, PieceData pieceData)
        {
            PipeController pipe = instance.GetComponent<PipeController>();
            if (pipe == null)
            {
                pipe = instance.AddComponent<PipeController>();
            }

            pipe.SetPieceType(pieceData.pieceType);
            pipe.SetGridPosition(pieceData.x, pieceData.z);
            AssignTileController(pipe, pieceData.x, pieceData.z);
            pipe.SetInitialRotation(pieceData.rotation);
            EnsureCollider(instance);

            _spawnedPieces.Add(pipe);
        }

        private void AssignTileController(PieceBase piece, int x, int z)
        {
            if (_tileMap.TryGetValue((x, z), out TileController tile))
            {
                piece.SetTileController(tile);
            }
        }

        private Transform FindVisualChild(Transform parent)
        {
            Transform visual = parent.Find("Visual");
            if (visual != null) return visual;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.GetComponent<MeshRenderer>() != null)
                {
                    return child;
                }
            }

            return null;
        }

        private void EnsureCollider(GameObject instance)
        {
            Collider col = instance.GetComponent<Collider>();
            if (col == null)
            {
                col = instance.GetComponentInChildren<Collider>();
            }

            if (col == null)
            {
                BoxCollider box = instance.AddComponent<BoxCollider>();
                box.size = new Vector3(0.6f, 0.5f, 0.6f);
                box.center = new Vector3(0, 0.25f, 0);
                box.isTrigger = false;
            }
        }

        private Vector3 GetGridPosition(int x, int z)
        {
            return new Vector3(
                x * LevelConstants.X_INTERVAL,
                0,
                z * LevelConstants.Z_INTERVAL
            );
        }
    }
}