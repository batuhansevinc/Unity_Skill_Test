using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BatuhanSevinc.ScriptableObjects;
using BufoGames.Abstract.Controllers;
using BufoGames.Constants;
using BufoGames.Data;
using BufoGames.Grid;
using BufoGames.Pieces;
using BufoGames.Validators;

namespace BufoGames.Controller
{
    public class LevelController : MonoBehaviour, ILevelController
    {
        [SerializeField, Range(1, 10)] public int gridSize = 4;
        [SerializeField] private GameEvent levelCompletedEvent;
        [SerializeField] private GameEvent fireworksEvent;
        [SerializeField] private GameEvent startEndGameAnimationsEvent;
        
        private GridManager _gridManager;
        private ConnectionValidator _connectionValidator;
        private LevelGridData _gridData;
        private List<PieceBase> _allPieces;
        private SourceController _source;
        private DestinationController _destination;
        private bool _isLevelComplete;
        private bool _isInitialized;
        
        public bool IsLevelComplete => _isLevelComplete;
        public int PieceCount => _allPieces?.Count ?? 0;
        
        public void Initialize(int size, List<PieceBase> pieces, SourceController source, DestinationController destination)
        {
            gridSize = size;
            _allPieces = pieces ?? new List<PieceBase>();
            _source = source;
            _destination = destination;
            
            _gridData = new LevelGridData(gridSize);
            _gridManager = new GridManager(gridSize);
            _gridManager.BuildMap(_allPieces);
            _connectionValidator = new ConnectionValidator(_gridManager, _allPieces.Count);
            
            SubscribeToPipeEvents();
            _isInitialized = true;
            
            StartCoroutine(InitialConnectionCheck());
        }
        
        public void SetGameEvents(GameEvent levelCompleted, GameEvent fireworks, GameEvent startEndAnimations)
        {
            levelCompletedEvent = levelCompleted;
            fireworksEvent = fireworks;
            startEndGameAnimationsEvent = startEndAnimations;
        }
        
        private void SubscribeToPipeEvents()
        {
            foreach (var piece in _allPieces)
            {
                if (piece is PipeController pipe)
                {
                    pipe.OnRotationCompleted.AddListener(OnPieceRotated);
                }
                else if (piece is SourceController source)
                {
                    source.OnRotationCompleted.AddListener(OnPieceRotated);
                }
                else if (piece is DestinationController dest)
                {
                    dest.OnRotationCompleted.AddListener(OnPieceRotated);
                }
            }
        }
        
        private void UnsubscribeFromPipeEvents()
        {
            if (_allPieces == null) return;
            
            foreach (var piece in _allPieces)
            {
                if (piece is PipeController pipe)
                {
                    pipe.OnRotationCompleted.RemoveListener(OnPieceRotated);
                }
                else if (piece is SourceController source)
                {
                    source.OnRotationCompleted.RemoveListener(OnPieceRotated);
                }
                else if (piece is DestinationController dest)
                {
                    dest.OnRotationCompleted.RemoveListener(OnPieceRotated);
                }
            }
        }
        
        private IEnumerator InitialConnectionCheck()
        {
            yield return null;
            CheckLevelCompletion();
        }
        
        public int GetGridSize() => gridSize;
        
        public float GetXInterval() => _gridData?.XInterval ?? LevelConstants.X_INTERVAL;
        
        public float GetZInterval() => _gridData?.ZInterval ?? LevelConstants.Z_INTERVAL;
        
        public void InitializeLevel()
        {
            if (!_isInitialized)
            {
                StartCoroutine(LateInitialize());
            }
        }
        
        private IEnumerator LateInitialize()
        {
            yield return new WaitForSeconds(LevelConstants.INITIALIZATION_DELAY);
            
            if (!_isInitialized)
            {
                _gridData = new LevelGridData(gridSize);
            }
        }
        
        public void CheckLevelCompletion()
        {
            if (!_isInitialized || _isLevelComplete) return;
            if (_source == null || _allPieces == null || _allPieces.Count == 0) return;
            
            bool allConnected = _connectionValidator.ValidateAllConnections(_source, _allPieces);
            
            if (allConnected)
            {
                _isLevelComplete = true;
                StartCoroutine(OnLevelCompleted());
            }
        }
        
        private void OnPieceRotated()
        {
            CheckLevelCompletion();
        }
        
        public LevelGridData GetGridData() => _gridData;
        
        public void CheckConnectionsFromSource()
        {
            CheckLevelCompletion();
        }
        
        public PieceBase GetPieceAt(int x, int z)
        {
            return _gridManager?.GetPieceAt(x, z);
        }
        
        public string GetConnectionStats()
        {
            if (_connectionValidator == null) return "Not initialized";
            return $"{_connectionValidator.LastConnectedCount}/{_connectionValidator.LastTotalCount} connected";
        }
        
        private IEnumerator OnLevelCompleted()
        {
            startEndGameAnimationsEvent?.InvokeEvents();
            
            yield return new WaitForSeconds(LevelConstants.COMPLETION_ANIMATION_DURATION);
            
            fireworksEvent?.InvokeEvents();
            levelCompletedEvent?.InvokeEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromPipeEvents();
        }
    }
}
