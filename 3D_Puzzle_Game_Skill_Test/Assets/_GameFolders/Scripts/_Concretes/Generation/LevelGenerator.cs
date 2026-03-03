using UnityEngine;
using System;
using System.Collections.Generic;
using DG.Tweening;
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
        private List<DestinationController> _destinationControllers = new List<DestinationController>();

        private List<(Transform transform, float finalY)> _tileTransforms = new List<(Transform, float)>();
        private List<(Transform transform, float finalY)> _pieceTransforms = new List<(Transform, float)>();
        private Sequence _spawnSequence;

        public List<PieceBase> SpawnedPieces => _spawnedPieces;
        public SourceController SourceController => _sourceController;
        public List<DestinationController> DestinationControllers => _destinationControllers;

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
            _destinationControllers.Clear();
            _tileTransforms.Clear();
            _pieceTransforms.Clear();
            KillSpawnAnimation();

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
            controller.Initialize(levelData.gridWidth, levelData.gridHeight, _spawnedPieces, _sourceController, _destinationControllers);

            return levelRoot;
        }

        private void GenerateTiles(GameObject parent, LevelDataSO levelData, ThemeDataSO theme)
        {
            int gridWidth = levelData.gridWidth;
            int gridHeight = levelData.gridHeight;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    GameObject tilePrefab = ((x + z) % 2 == 0) ? theme.tileAPrefab : theme.tileBPrefab;
                    if (tilePrefab == null) continue;

                    Vector3 position = GetGridPosition(x, z);
                    Vector3 spawnPosition = new Vector3(position.x, position.y + LevelConstants.SPAWN_DROP_HEIGHT, position.z);
                    GameObject tile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity, parent.transform);
                    tile.name = $"Tile_{x}_{z}";
                    tile.transform.localScale = Vector3.zero;

                    TileController tileController = tile.AddComponent<TileController>();
                    tileController.SetOriginalPosition(position);
                    _tileMap[(x, z)] = tileController;
                    _tileTransforms.Add((tile.transform, position.y));
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
            Vector3 spawnPosition = new Vector3(position.x, position.y + LevelConstants.SPAWN_DROP_HEIGHT, position.z);
            GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity, parent.transform);
            instance.name = $"{pieceData.pieceType}_{pieceData.x}_{pieceData.z}";
            instance.transform.localScale = Vector3.zero;

            _pieceTransforms.Add((instance.transform, position.y));
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

            _destinationControllers.Add(dest);
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
            
            // Mark as static if it's a static pipe type
            if (pieceData.pieceType.IsStatic())
            {
                pipe.SetStatic(true);
            }
            
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

        public void PlaySpawnAnimation(Action onComplete)
        {
            KillSpawnAnimation();

            if (_tileTransforms.Count == 0 && _pieceTransforms.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            ShuffleList(_tileTransforms);
            ShuffleList(_pieceTransforms);

            _spawnSequence = DOTween.Sequence();

            // --- Phase 1: Tiles drop with a single soft bounce ---
            float tileStagger = Mathf.Min(
                LevelConstants.SPAWN_STAGGER_INTERVAL,
                LevelConstants.SPAWN_TOTAL_MAX_DURATION / Mathf.Max(1, _tileTransforms.Count)
            );
            float tileDuration = LevelConstants.TILE_DROP_DURATION;

            for (int i = 0; i < _tileTransforms.Count; i++)
            {
                var (t, finalY) = _tileTransforms[i];
                if (t == null) continue;

                float insertTime = i * tileStagger;
                // Smooth drop with one gentle settle (OutQuart for minimal overshoot)
                _spawnSequence.Insert(insertTime, t.DOMoveY(finalY, tileDuration).SetEase(Ease.OutQuart));
                _spawnSequence.Insert(insertTime, t.DOScale(Vector3.one, tileDuration * 0.4f).SetEase(Ease.OutBack));
            }

            // --- Phase 2: Pieces drop cleanly after all tiles have landed ---
            float tilesEndTime = (_tileTransforms.Count - 1) * tileStagger + tileDuration + LevelConstants.PHASE_GAP;

            float pieceStagger = Mathf.Min(
                LevelConstants.SPAWN_STAGGER_INTERVAL,
                LevelConstants.SPAWN_TOTAL_MAX_DURATION / Mathf.Max(1, _pieceTransforms.Count)
            );
            float pieceDuration = LevelConstants.PIECE_DROP_DURATION;

            for (int i = 0; i < _pieceTransforms.Count; i++)
            {
                var (t, finalY) = _pieceTransforms[i];
                if (t == null) continue;

                float insertTime = tilesEndTime + i * pieceStagger;
                // Clean drop, no bounce — single smooth motion
                _spawnSequence.Insert(insertTime, t.DOMoveY(finalY, pieceDuration).SetEase(Ease.OutCubic));
                _spawnSequence.Insert(insertTime, t.DOScale(Vector3.one, pieceDuration * 0.4f).SetEase(Ease.OutCubic));
            }

            _spawnSequence.OnComplete(() => onComplete?.Invoke());
        }

        public void KillSpawnAnimation()
        {
            _spawnSequence?.Kill();
            _spawnSequence = null;
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private void OnDestroy()
        {
            KillSpawnAnimation();
        }
    }
}