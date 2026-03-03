using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BufoGames.Data
{
    public class LevelValidator
    {
        private LevelDataSO _levelData;
        private string _validationMessage;
        private int _estimatedDifficulty;
        private int _minimumMoves;
        private List<int> _solutionMoves;

        private const int MAX_TIME_MS = 30000; // 30s timeout guard
        private const string TIMEOUT_MESSAGE = "Validation timed out. Try simplifying the level or lowering the complexity.";

        private byte[] _rotationState;
        private byte[] _solutionRotation;
        private byte[] _initialRotation;
        private PieceData[] _pieceDataCache;
        private int _pieceCount;
        private Vector2Int _sourcePos;
        private List<Vector2Int> _destPositions;
        private int _gridWidth;
        private int _gridHeight;
        private int _gridSize;
        
        private bool[] _visited;
        private int[] _bfsQueue;
        
        private List<int> _rotatableIndices;
        private bool _solutionFound;
        private int _iterationCount;
        private const int MAX_ITERATIONS = 1000000;
        
        // Constraint propagation
        private byte[][] _validRotations; // Per piece: which rotations are still valid
        private HashSet<int> _fixedIndices; // Indices whose rotation is determined
        
        public LevelValidator(LevelDataSO data)
        {
            _levelData = data;
            _minimumMoves = -1;
            _solutionMoves = new List<int>();
        }
        
        private System.Action<float> _progressCallback;

        public async Task<bool> ValidateAsync(System.Action<float> progress = null)
        {
            _progressCallback = progress;
            if (!ValidateBasicRequirements())
                return false;

            InitializeStructures();

            using var cts = new CancellationTokenSource(MAX_TIME_MS);
            try
            {
                bool result = await Task.Run(() => ValidateSolvability(cts.Token), cts.Token);

                if (result)
                {
                    CalculateDifficulty();
                    int destCount = _destPositions.Count;
                    string destInfo = destCount > 1 ? $"All {destCount} destinations reachable!" : "Destination reachable!";
                    _validationMessage = $"Level is solvable!\n{destInfo}\nMinimum moves: {_minimumMoves}\nDifficulty: {_estimatedDifficulty}/10";
                }
                else if (cts.IsCancellationRequested && string.IsNullOrEmpty(_validationMessage))
                {
                    _validationMessage = TIMEOUT_MESSAGE;
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                _validationMessage = TIMEOUT_MESSAGE;
                return false;
            }
        }

        public bool Validate()
        {
            if (!ValidateBasicRequirements())
                return false;
            
            InitializeStructures();
            
            if (!ValidateSolvability(CancellationToken.None))
                return false;
            
            CalculateDifficulty();
            
            int destCount = _destPositions.Count;
            string destInfo = destCount > 1 ? $"All {destCount} destinations reachable!" : "Destination reachable!";
            _validationMessage = $"Level is solvable!\n{destInfo}\nMinimum moves: {_minimumMoves}\nDifficulty: {_estimatedDifficulty}/10";
            return true;
        }
        
        private void InitializeStructures()
        {
            _gridWidth = _levelData.gridWidth;
            _gridHeight = _levelData.gridHeight;
            _pieceCount = _levelData.pieces.Count;
            _gridSize = _gridWidth * _gridHeight;
            
            _pieceDataCache = new PieceData[_gridSize];
            _rotationState = new byte[_gridSize];
            _solutionRotation = new byte[_gridSize];
            _initialRotation = new byte[_gridSize];
            _visited = new bool[_gridSize];
            _bfsQueue = new int[_gridSize];
            _rotatableIndices = new List<int>();
            _validRotations = new byte[_gridSize][];
            _fixedIndices = new HashSet<int>();
            
            for (int i = 0; i < _pieceCount; i++)
            {
                var piece = _levelData.pieces[i];
                int idx = piece.x + piece.z * _gridWidth;
                _pieceDataCache[idx] = piece;
                byte rot = (byte)(((piece.rotation % 360) + 360) % 360 / 90);
                _rotationState[idx] = rot;
                _solutionRotation[idx] = rot;
                _initialRotation[idx] = rot;
                
                bool rotatable = !piece.pieceType.IsStatic() && 
                    piece.pieceType != PieceType.CrossPipe &&
                    piece.pieceType != PieceType.Source &&
                    piece.pieceType != PieceType.Destination;
                
                if (rotatable)
                {
                    _rotatableIndices.Add(idx);
                    int uniqueRots = PipeConnectionHelper.GetUniqueRotationCount(piece.pieceType);
                    _validRotations[idx] = new byte[uniqueRots];
                    for (byte r = 0; r < uniqueRots; r++)
                        _validRotations[idx][r] = r;
                }
                else
                {
                    _fixedIndices.Add(idx);
                }
                
                if (piece.pieceType == PieceType.Source)
                    _sourcePos = new Vector2Int(piece.x, piece.z);
            }
            
            _destPositions = new List<Vector2Int>();
            foreach (var piece in _levelData.pieces)
            {
                if (piece.pieceType == PieceType.Destination)
                    _destPositions.Add(new Vector2Int(piece.x, piece.z));
            }
        }
        
        private bool ValidateBasicRequirements()
        {
            var source = _levelData.GetSource();
            if (source == null)
            {
                _validationMessage = "No source found! Add a source piece.";
                return false;
            }
            
            var destinations = _levelData.GetDestinations();
            if (destinations.Count == 0)
            {
                _validationMessage = "No destination found! Add at least one destination piece.";
                return false;
            }
            
            int pipeCount = 0;
            foreach (var p in _levelData.pieces)
            {
                if (p.pieceType != PieceType.Source && p.pieceType != PieceType.Destination)
                    pipeCount++;
            }
            
            if (pipeCount < 1)
            {
                _validationMessage = "No pipes found! Add at least one pipe.";
                return false;
            }
            
            if (!HasPotentialConnection(source))
            {
                _validationMessage = "Source has no adjacent pipes to connect to!";
                return false;
            }
            
            foreach (var dest in destinations)
            {
                if (!HasPotentialConnection(dest))
                {
                    _validationMessage = $"Destination at ({dest.x},{dest.z}) has no adjacent pipes!";
                    return false;
                }
            }
            
            string boundaryError = CheckStaticBoundaryViolations();
            if (boundaryError != null)
            {
                _validationMessage = boundaryError;
                return false;
            }
            
            return true;
        }
        
        private string CheckStaticBoundaryViolations()
        {
            string[] dirNames = { "Up", "Right", "Down", "Left" };
            
            foreach (var piece in _levelData.pieces)
            {
                bool isStatic = piece.pieceType.IsStatic();
                bool isSourceOrDest = piece.pieceType == PieceType.Source || piece.pieceType == PieceType.Destination;
                
                if (!isStatic && !isSourceOrDest) continue;
                
                var ports = PipeConnectionHelper.GetOpenPorts(piece.pieceType, piece.rotation);
                string typeName = piece.pieceType.ToString();
                
                foreach (int dir in ports)
                {
                    var offset = PipeConnectionHelper.DirOffset[dir];
                    int nx = piece.x + offset.x;
                    int nz = piece.z + offset.y;
                    
                    if (nx < 0 || nx >= _levelData.gridWidth || nz < 0 || nz >= _levelData.gridHeight)
                        return $"{typeName} at ({piece.x},{piece.z}) has port facing outside grid ({dirNames[dir]})!";
                    
                    var neighbor = _levelData.GetPieceAt(nx, nz);
                    
                    if (neighbor == null)
                        return $"{typeName} at ({piece.x},{piece.z}) has port facing empty cell ({dirNames[dir]})!";
                    
                    if (isStatic)
                    {
                        bool canConnect = CanNeighborConnectBack(neighbor, dir);
                        if (!canConnect)
                            return $"{typeName} at ({piece.x},{piece.z}) port ({dirNames[dir]}) cannot connect to neighbor at ({nx},{nz})!";
                    }
                }
            }
            
            return null;
        }
        
        private bool CanNeighborConnectBack(PieceData neighbor, int dirFromPiece)
        {
            int oppositeDir = PipeConnectionHelper.OppositeDir[dirFromPiece];
            
            if (neighbor.pieceType.IsStatic() || 
                neighbor.pieceType == PieceType.Source || 
                neighbor.pieceType == PieceType.Destination)
            {
                return PipeConnectionHelper.HasPort(neighbor.pieceType, neighbor.rotation, oppositeDir);
            }
            
            for (int rot = 0; rot < 360; rot += 90)
            {
                if (PipeConnectionHelper.HasPort(neighbor.pieceType, rot, oppositeDir))
                    return true;
            }
            
            return false;
        }
        
        private bool HasPotentialConnection(PieceData piece)
        {
            for (int dir = 0; dir < 4; dir++)
            {
                var offset = PipeConnectionHelper.DirOffset[dir];
                int nx = piece.x + offset.x;
                int nz = piece.z + offset.y;
                if (nx >= 0 && nx < _levelData.gridWidth && 
                    nz >= 0 && nz < _levelData.gridHeight &&
                    _levelData.GetPieceAt(nx, nz) != null)
                    return true;
            }
            return false;
        }
        
        private bool ValidateSolvability(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                _validationMessage = TIMEOUT_MESSAGE;
                return false;
            }

            // Constraint propagation first - reduce search space
            if (!PropagateConstraints(token))
            {
                if (!token.IsCancellationRequested)
                    _validationMessage = "Level is not solvable! (constraint propagation failed)";
                return false;
            }
            
            if (token.IsCancellationRequested)
            {
                _validationMessage = TIMEOUT_MESSAGE;
                return false;
            }
            
            // Check if already solved
            if (IsCurrentStateSolved())
            {
                _minimumMoves = 0;
                for (int i = 0; i < _gridSize; i++)
                    _solutionRotation[i] = _rotationState[i];
                return true;
            }
            
            // Sort by most constrained first
            SortRotatablesByConstraints();
            
            _solutionFound = false;
            _iterationCount = 0;
            
            BacktrackSolve(0, token);
            
            if (token.IsCancellationRequested)
            {
                _validationMessage = TIMEOUT_MESSAGE;
                return false;
            }
            
            if (_solutionFound)
            {
                CalculateMinimumMoves();
                return true;
            }
            
            _validationMessage = "Level is not solvable!";
            return false;
        }
        
        // Arc consistency - propagate constraints from fixed pieces
        private bool PropagateConstraints(CancellationToken token)
        {
            bool changed = true;
            int iterations = 0;
            
            while (changed && iterations < 100)
            {
                if (token.IsCancellationRequested)
                    return false;

                changed = false;
                iterations++;
                
                foreach (int idx in _rotatableIndices)
                {
                    if (token.IsCancellationRequested)
                        return false;

                    if (_validRotations[idx] == null || _validRotations[idx].Length == 0)
                        return false;
                    
                    var piece = _pieceDataCache[idx];
                    int cx = idx % _gridWidth;
                    int cz = idx / _gridWidth;
                    
                    var newValidRots = new List<byte>();
                    
                    foreach (byte rot in _validRotations[idx])
                    {
                        if (IsRotationValidForPiece(idx, rot))
                            newValidRots.Add(rot);
                    }
                    
                    if (newValidRots.Count == 0)
                        return false;
                    
                    if (newValidRots.Count < _validRotations[idx].Length)
                    {
                        _validRotations[idx] = newValidRots.ToArray();
                        changed = true;
                        
                        // If only one rotation left, fix it
                        if (newValidRots.Count == 1)
                        {
                            _rotationState[idx] = newValidRots[0];
                            _fixedIndices.Add(idx);
                        }
                    }
                }
            }
            
            return true;
        }
        
        private bool IsRotationValidForPiece(int idx, byte rot)
        {
            int cx = idx % _gridWidth;
            int cz = idx / _gridWidth;
            var piece = _pieceDataCache[idx];
            int rotDegrees = rot * 90;
            
            for (int dir = 0; dir < 4; dir++)
            {
                bool hasPort = PipeConnectionHelper.HasPort(piece.pieceType, rotDegrees, dir);
                var offset = PipeConnectionHelper.DirOffset[dir];
                int nx = cx + offset.x;
                int nz = cz + offset.y;
                
                // Check grid bounds
                if (nx < 0 || nx >= _gridWidth || nz < 0 || nz >= _gridHeight)
                    continue;
                
                int nIdx = nx + nz * _gridWidth;
                var neighbor = _pieceDataCache[nIdx];
                
                if (neighbor == null)
                    continue;
                
                int oppositeDir = PipeConnectionHelper.OppositeDir[dir];
                
                // Fixed neighbor (static, source, dest) - must match exactly
                if (_fixedIndices.Contains(nIdx))
                {
                    int nRotDegrees = _rotationState[nIdx] * 90;
                    bool neighborHasPort = PipeConnectionHelper.HasPort(neighbor.pieceType, nRotDegrees, oppositeDir);
                    
                    // Strict connection checks removed to allow unused ports/dead ends.
                    // Only enforce Source/Destination connectivity via BFS later.
                    
                    /*
                    // Static neighbor pointing at me - I must connect back
                    if (neighborHasPort && !hasPort)
                        return false;
                    
                    // I'm pointing at static neighbor - they must connect back
                    if (hasPort && neighbor.pieceType.IsStatic() && !neighborHasPort)
                        return false;
                    */
                }
            }
            
            return true;
        }
        
        private int[] _bfsDistances;

        private void SortRotatablesByConstraints()
        {
            // Calculate BFS distance from Source to prioritize path growing
            _bfsDistances = new int[_gridSize];
            for (int i = 0; i < _gridSize; i++) _bfsDistances[i] = int.MaxValue;

            Queue<int> q = new Queue<int>();
            if (_sourcePos.x >= 0 && _sourcePos.y >= 0)
            {
                int sourceIdx = _sourcePos.x + _sourcePos.y * _gridWidth;
                _bfsDistances[sourceIdx] = 0;
                q.Enqueue(sourceIdx);
            }

            while (q.Count > 0)
            {
                int curr = q.Dequeue();
                int cx = curr % _gridWidth;
                int cz = curr / _gridWidth;

                for (int dir = 0; dir < 4; dir++)
                {
                    var offset = PipeConnectionHelper.DirOffset[dir];
                    int nx = cx + offset.x;
                    int nz = cz + offset.y;

                    if (nx >= 0 && nx < _gridWidth && nz >= 0 && nz < _gridHeight)
                    {
                        int nIdx = nx + nz * _gridWidth;
                        // Only traverse if there is a piece
                        if (_pieceDataCache[nIdx] != null && _bfsDistances[nIdx] == int.MaxValue)
                        {
                            _bfsDistances[nIdx] = _bfsDistances[curr] + 1;
                            q.Enqueue(nIdx);
                        }
                    }
                }
            }

            _rotatableIndices.Sort((a, b) => {
                // 1. Distance from source (closer first)
                int distA = _bfsDistances[a];
                int distB = _bfsDistances[b];
                if (distA != distB) return distA.CompareTo(distB);

                // 2. Fewest valid rotations (MRV)
                int countA = _validRotations[a]?.Length ?? 4;
                int countB = _validRotations[b]?.Length ?? 4;
                if (countA != countB) return countA.CompareTo(countB);

                // 3. Constraint score (fallback)
                return GetConstraintScore(b).CompareTo(GetConstraintScore(a));
            });
        }
        
        private int GetConstraintScore(int idx)
        {
            int score = 0;
            int cx = idx % _gridWidth;
            int cz = idx / _gridWidth;
            
            for (int dir = 0; dir < 4; dir++)
            {
                var offset = PipeConnectionHelper.DirOffset[dir];
                int nx = cx + offset.x;
                int nz = cz + offset.y;
                
                if (nx < 0 || nx >= _gridWidth || nz < 0 || nz >= _gridHeight)
                {
                    score += 2;
                    continue;
                }
                
                int nIdx = nx + nz * _gridWidth;
                var neighbor = _pieceDataCache[nIdx];
                
                if (neighbor == null)
                    score += 1;
                else if (_fixedIndices.Contains(nIdx))
                    score += 5;
            }
            return score;
        }
        
        private void BacktrackSolve(int rotatableIdx, CancellationToken token)
        {
            if (_solutionFound || token.IsCancellationRequested) return;
            if (++_iterationCount > MAX_ITERATIONS) return;

            if (_iterationCount % 1000 == 0 && _progressCallback != null)
            {
                // Estimate progress? It's hard with backtracking.
                // Just pulse it or use iteration count vs max
                float p = (float)_iterationCount / MAX_ITERATIONS;
                _progressCallback(p);
            }

            // Optimization: Check if already solved (ignoring remaining pieces)
            // This allows us to stop early if the path is complete, ignoring decoys.
            if (IsCurrentStateSolved())
            {
                _solutionFound = true;
                for (int i = 0; i < _gridSize; i++)
                    _solutionRotation[i] = _rotationState[i];
                return;
            }

            if (rotatableIdx >= _rotatableIndices.Count)
            {
                return;
            }

            int idx = _rotatableIndices[rotatableIdx];
            
            // Skip already fixed pieces
            if (_fixedIndices.Contains(idx))
            {
                BacktrackSolve(rotatableIdx + 1, token);
                return;
            }
            
            var validRots = _validRotations[idx];
            if (validRots == null || validRots.Length == 0)
                return;

            // Sort rotations to try most likely connections first
            var sortedRots = validRots.OrderByDescending(r => GetConnectionScore(idx, r)).ToArray();

            byte originalRot = _rotationState[idx];

            foreach (byte r in sortedRots)
            {
                if (_solutionFound || token.IsCancellationRequested) return;

                _rotationState[idx] = r;

                if (IsLocallyConsistent(idx, rotatableIdx))
                {
                    BacktrackSolve(rotatableIdx + 1, token);
                }
            }

            if (!_solutionFound)
                _rotationState[idx] = originalRot;
        }

        private int GetConnectionScore(int idx, byte rot)
        {
            int score = 0;
            int cx = idx % _gridWidth;
            int cz = idx / _gridWidth;
            int rotDegrees = rot * 90;
            var piece = _pieceDataCache[idx];

            for (int dir = 0; dir < 4; dir++)
            {
                if (!PipeConnectionHelper.HasPort(piece.pieceType, rotDegrees, dir))
                    continue;

                var offset = PipeConnectionHelper.DirOffset[dir];
                int nx = cx + offset.x;
                int nz = cz + offset.y;

                if (nx < 0 || nx >= _gridWidth || nz < 0 || nz >= _gridHeight)
                    continue;

                int nIdx = nx + nz * _gridWidth;

                // Prioritize connecting to neighbors closer to source (likely already fixed)
                if (_bfsDistances != null && _bfsDistances[nIdx] < _bfsDistances[idx])
                {
                    if (_pieceDataCache[nIdx] != null)
                    {
                        int nRotDegrees = _rotationState[nIdx] * 90;
                        int oppositeDir = PipeConnectionHelper.OppositeDir[dir];
                        if (PipeConnectionHelper.HasPort(_pieceDataCache[nIdx].pieceType, nRotDegrees, oppositeDir))
                        {
                            score += 10;
                        }
                    }
                }
            }
            return score;
        }

        private bool IsLocallyConsistent(int idx, int currentOrder)
        {
            // Relaxed consistency check:
            // 1. Allow ports to point to boundaries (walls)
            // 2. Allow ports to point to empty cells
            // 3. Allow mismatches (unused ports pointing to non-connecting neighbors)
            // This is necessary because a valid solution might have a T-Junction where one arm is blocked/unused.
            
            // We only enforce that if two pieces DO connect, they must be compatible? 
            // No, even that is too strict if one is just pointing at the side of another.
            
            // The only strict rule we might want is:
            // If a neighbor is STATIC and requires a connection, we must provide it?
            // But even then, maybe the static piece's port is the unused one.
            
            // For now, we return true to allow the BFS to determine actual connectivity.
            // To keep some pruning, we could check if the piece is completely isolated (no connections possible),
            // but that requires checking all neighbors.
            
            return true; 
        }
        
        private void CalculateMinimumMoves()
        {
            _minimumMoves = 0;
            _solutionMoves.Clear();
            
            for (int i = 0; i < _gridSize; i++)
            {
                if (_pieceDataCache[i] == null) continue;
                if (!_rotatableIndices.Contains(i)) continue;
                if (_pieceDataCache[i].isDecoy) continue; // Decoys don't affect solvability
                
                int currentRot = _initialRotation[i];
                int targetRot = _solutionRotation[i];
                
                var piece = _pieceDataCache[i];
                int solvedMask = PipeConnectionHelper.GetPortMask(piece.pieceType, targetRot * 90);
                
                // Find minimum clockwise 90° clicks to reach any equivalent solved rotation
                int minClicks = 4;
                for (int rot = 0; rot < 4; rot++)
                {
                    if (PipeConnectionHelper.GetPortMask(piece.pieceType, rot * 90) == solvedMask)
                    {
                        int clicks = (rot - currentRot + 4) % 4;
                        if (clicks < minClicks) minClicks = clicks;
                    }
                }
                
                _minimumMoves += minClicks;
                for (int j = 0; j < minClicks; j++)
                    _solutionMoves.Add(i);
            }
        }
        
        private bool IsCurrentStateSolved()
        {
            System.Array.Clear(_visited, 0, _visited.Length);
            
            int sourceIdx = _sourcePos.x + _sourcePos.y * _gridWidth;
            var sourcePiece = _pieceDataCache[sourceIdx];
            if (sourcePiece == null) return false;
            
            _visited[sourceIdx] = true;
            _bfsQueue[0] = sourceIdx;
            int queueStart = 0, queueEnd = 1;
            
            int connectedDests = 0;
            int totalDests = _destPositions.Count;
            
            while (queueStart < queueEnd)
            {
                int currentIdx = _bfsQueue[queueStart++];
                int cx = currentIdx % _gridWidth;
                int cz = currentIdx / _gridWidth;
                
                var piece = _pieceDataCache[currentIdx];
                if (piece == null) continue;
                
                if (piece.pieceType == PieceType.Destination)
                {
                    connectedDests++;
                    if (connectedDests == totalDests)
                        return true;
                }
                
                int rotDegrees = _rotationState[currentIdx] * 90;
                
                for (int dir = 0; dir < 4; dir++)
                {
                    if (!PipeConnectionHelper.HasPort(piece.pieceType, rotDegrees, dir))
                        continue;
                    
                    var offset = PipeConnectionHelper.DirOffset[dir];
                    int nx = cx + offset.x;
                    int nz = cz + offset.y;
                    
                    // Decoy pieces are allowed to have dangling ports
                    // Only enforce strict port rules on path (non-decoy) pieces
                    if (piece.isDecoy)
                        continue; // Skip port validation for decoys, they don't participate in solution
                    
                    // Kural: path'teki her port mutlaka dolu ve karşılıklı bağlı olmalı
                    if (nx < 0 || nx >= _gridWidth || nz < 0 || nz >= _gridHeight)
                        return false; // port dışarı bakamaz
                    
                    int nIdx = nx + nz * _gridWidth;
                    var neighbor = _pieceDataCache[nIdx];
                    if (neighbor == null)
                        return false; // port boş hücreye bakamaz
                    
                    int nRotDegrees = _rotationState[nIdx] * 90;
                    int oppositeDir = PipeConnectionHelper.OppositeDir[dir];
                    
                    if (!PipeConnectionHelper.HasPort(neighbor.pieceType, nRotDegrees, oppositeDir))
                        return false; // komşu geri bağlanmıyor
                    
                    if (!_visited[nIdx])
                    {
                        _visited[nIdx] = true;
                        _bfsQueue[queueEnd++] = nIdx;
                    }
                }
            }
            
            return connectedDests == totalDests;
        }
        
        private void CalculateDifficulty()
        {
            // Use the same formula as ProceduralLevelGenerator.CalculateDifficulty
            // so both generate and validate produce consistent results.
            float score = 0f;
            
            // Grid size contribution (max 2.5)
            score += Mathf.Min((_gridWidth + _gridHeight) * 0.25f, 2.5f);

            // Path piece count (non-decoy pieces, max 2.5)
            int pathCount = 0;
            int decoyCount = 0;
            int destCount = 0;
            int staticCount = 0;
            var pipeTypesUsed = new HashSet<PieceType>();
            
            foreach (var p in _levelData.pieces)
            {
                if (p.isDecoy)
                {
                    decoyCount++;
                    continue;
                }
                pathCount++;
                
                if (p.pieceType == PieceType.Destination) destCount++;
                if (p.pieceType == PieceType.Source || p.pieceType == PieceType.Destination) continue;
                
                if (p.pieceType.IsStatic()) staticCount++;
                
                // Track unique base pipe types
                var baseType = p.pieceType switch
                {
                    PieceType.StaticStraightPipe => PieceType.StraightPipe,
                    PieceType.StaticCornerPipe => PieceType.CornerPipe,
                    PieceType.StaticTJunctionPipe => PieceType.TJunctionPipe,
                    PieceType.StaticCrossPipe => PieceType.CrossPipe,
                    _ => p.pieceType
                };
                pipeTypesUsed.Add(baseType);
            }
            
            score += Mathf.Min(pathCount * 0.25f, 2.5f);
            
            // Min moves contribution (max 2.0)
            score += Mathf.Min(_minimumMoves * 0.2f, 2f);
            
            // Destination count contribution (max 1.5)
            score += Mathf.Min((destCount - 1) * 0.7f, 1.5f);
            
            // Pipe type variety contribution (max 1.0)
            score += Mathf.Min((pipeTypesUsed.Count - 1) * 0.4f, 1f);
            
            // Static pipe ratio reduces difficulty
            int pathPipes = pathCount - 1 - destCount; // exclude source + destinations
            float staticRatio = pathPipes > 0 ? (float)staticCount / pathPipes : 0f;
            score -= staticRatio * 1.5f;
            
            // Decoy ratio increases difficulty
            int totalPieces = _levelData.pieces.Count;
            float decoyRatio = totalPieces > 0 ? (float)decoyCount / totalPieces : 0f;
            score += decoyRatio * 1.0f;
            
            _estimatedDifficulty = Mathf.Clamp(Mathf.RoundToInt(score), 1, 10);
        }
        
        public string GetValidationMessage() => _validationMessage;
        public int GetEstimatedDifficulty() => _estimatedDifficulty;
        public int GetMinimumMoves() => _minimumMoves;
        public List<Vector2Int> GetSolutionPath()
        {
            var path = new List<Vector2Int>();
            foreach (int idx in _solutionMoves)
                path.Add(new Vector2Int(idx % _gridWidth, idx / _gridWidth));
            return path;
        }
    }
}

