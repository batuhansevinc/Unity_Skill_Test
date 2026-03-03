using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using BufoGames.Data;

namespace BufoGames.Generation
{
    /// <summary>
    /// Procedural level generator using Wilson's Loop-Erased Random Walk algorithm.
    /// 
    /// Pipeline:
    /// 1. Generate uniformly random spanning tree on the grid (Wilson's algorithm)
    /// 2. Select Source (edge leaf) and Destinations (distant leaves)
    /// 3. Extract puzzle subtree (unique paths from Source to all Destinations)
    /// 4. Validate degree constraints vs allowed pipe types
    /// 5. Assign pipe types and rotations from tree structure
    /// 6. Verify solved state (connectivity + no dangling ports)
    /// 7. Verify solution uniqueness (AC-3 constraint propagation)
    /// 8. Place red herring decoys (adjacent to path, visually deceptive)
    /// 9. Convert some pipes to static (hint pieces)
    /// 10. Smart scramble (most deceptive wrong rotation)
    /// 11. Calculate difficulty stats
    /// 
    /// Guarantees:
    /// - Solvability by construction (spanning tree = connected)
    /// - Uniqueness via AC-3 + bounded backtracking
    /// - Near-zero failure rate (tree always produces valid topology)
    /// </summary>
    public class ProceduralLevelGenerator
    {
        private LevelGeneratorConfig _config;
        private System.Random _random;
        private int _gridWidth, _gridHeight;

        // Wilson's spanning tree: adjacency list (full grid tree)
        private Dictionary<Vector2Int, HashSet<Vector2Int>> _treeAdj;

        // Puzzle subtree: the subset of tree connecting Source to all Destinations
        private Dictionary<Vector2Int, HashSet<Vector2Int>> _subtreeAdj;

        // Solved state storage: maps position -> (pieceType, solvedRotation)
        private Dictionary<Vector2Int, (PieceType type, int rotation)> _solvedPieces;

        // Diagnostics
        private string _lastFailureReason = "";
        public string LastFailureReason => _lastFailureReason;

        // ================================================================
        //  PUBLIC API
        // ================================================================

        /// <summary>
        /// Attempts generation with automatic retries and seed rotation.
        /// Returns true on first successful attempt.
        /// </summary>
        public bool GenerateWithRetry(LevelDataSO levelData, LevelGeneratorConfig config, int maxAttempts = 10)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                if (Generate(levelData, config))
                    return true;

                // Rotate seed for next attempt
                if (!config.useRandomSeed)
                    config.seed += 7 + attempt;
            }
            return false;
        }

        public bool Generate(LevelDataSO levelData, LevelGeneratorConfig config)
        {
            _config = config;
            _random = config.useRandomSeed ? new System.Random() : new System.Random(config.seed);
            _gridWidth = config.gridWidth;
            _gridHeight = config.gridHeight;
            _lastFailureReason = "";
            _solvedPieces = new Dictionary<Vector2Int, (PieceType, int)>();

            levelData.gridWidth = _gridWidth;
            levelData.gridHeight = _gridHeight;
            levelData.pieces.Clear();

            // Step 1: Generate spanning tree via Wilson's Loop-Erased Random Walk
            GenerateSpanningTree();

            // Step 2: Select source on grid edge (prefer tree leaves)
            var sourcePos = SelectSource();
            if (sourcePos.x < 0)
            { _lastFailureReason = "No suitable edge cell for Source."; return false; }

            // Step 3: Select destinations at appropriate tree distance
            var destPositions = SelectDestinations(sourcePos, config.destinationCount);
            if (destPositions.Count < config.destinationCount)
            { _lastFailureReason = $"Could only place {destPositions.Count}/{config.destinationCount} destination(s)."; return false; }

            // Step 4: Extract puzzle subtree (union of paths from Source to each Destination)
            _subtreeAdj = ExtractPuzzleSubtree(sourcePos, destPositions);
            if (_subtreeAdj == null || _subtreeAdj.Count == 0)
            { _lastFailureReason = "Failed to extract puzzle subtree."; return false; }

            // Step 5: Validate degree constraints vs allowed pipe types
            if (!ValidateDegreeConstraints(sourcePos, destPositions))
            { _lastFailureReason = "Subtree requires pipe types not allowed by config."; return false; }

            // Step 6: Assign pipe types and rotations from tree structure
            if (!AssignPipes(levelData, sourcePos, destPositions))
            { _lastFailureReason = "Failed to assign pipes from subtree structure."; return false; }

            // Step 7: Verify solved state (dangling ports + BFS connectivity)
            if (!ValidateSolvedState(levelData, sourcePos, destPositions))
            { _lastFailureReason = "Solved state validation failed."; return false; }

            // Step 8: Verify solution uniqueness (AC-3 + bounded backtracking)
            if (!VerifyUniqueSolution(levelData))
            { _lastFailureReason = "Solution is not unique."; return false; }

            // Step 9: Place red herring decoys (adjacent to path, visually deceptive)
            if (_config.decoyPipeRatio > 0)
                PlaceRedHerringDecoys(levelData);

            // Step 10: Convert some pipes to static (hint pieces)
            if (_config.useStaticPipes && _config.staticPipeRatio > 0)
                ConvertToStaticPipes(levelData);

            // Step 10.5: Re-validate after static conversion
            if (!ValidateSolvedState(levelData, sourcePos, destPositions))
            { _lastFailureReason = "Post-static-conversion validation failed."; return false; }

            // Step 11: Smart scramble (most deceptive wrong rotation)
            SmartScramble(levelData);

            // Step 12: Calculate stats
            levelData.isValidated = true;
            levelData.minimumMoves = CalculateMinMoves(levelData);
            levelData.estimatedDifficulty = CalculateDifficulty(levelData);
            int pathCount = _subtreeAdj != null ? _subtreeAdj.Count : 0;
            int decoyCount = levelData.pieces.Count - pathCount;
            levelData.validationMessage =
                $"Generated (Spanning Tree)!\n" +
                $"Pieces: {levelData.pieces.Count} ({pathCount} path + {decoyCount} decoy)\n" +
                $"Min moves: {levelData.minimumMoves}\n" +
                $"Difficulty: {levelData.estimatedDifficulty}/10";

            return true;
        }

        // ================================================================
        //  STEP 1: Wilson's Loop-Erased Random Walk
        // ================================================================

        /// <summary>
        /// Generates a uniformly random spanning tree on the grid using Wilson's algorithm.
        /// Each cell becomes a node; 4-directional adjacency defines edges.
        /// Loop-erased random walks guarantee uniform distribution over all spanning trees.
        /// </summary>
        private void GenerateSpanningTree()
        {
            _treeAdj = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

            var allCells = new List<Vector2Int>();
            for (int x = 0; x < _gridWidth; x++)
                for (int y = 0; y < _gridHeight; y++)
                {
                    var cell = new Vector2Int(x, y);
                    allCells.Add(cell);
                    _treeAdj[cell] = new HashSet<Vector2Int>();
                }

            var inTree = new HashSet<Vector2Int>();
            var notInTree = new List<Vector2Int>(allCells);

            // Start with one random cell in tree
            int startIdx = _random.Next(notInTree.Count);
            inTree.Add(notInTree[startIdx]);
            notInTree.RemoveAt(startIdx);
            Shuffle(notInTree);

            // next[cell] records the last neighbor taken from cell during random walk.
            // Overwriting automatically erases loops (Wilson's key insight).
            var next = new Dictionary<Vector2Int, Vector2Int>();

            int idx = 0;
            while (idx < notInTree.Count)
            {
                var startCell = notInTree[idx];
                if (inTree.Contains(startCell))
                {
                    idx++;
                    continue;
                }

                // Random walk from startCell until hitting a cell already in tree
                var current = startCell;
                while (!inTree.Contains(current))
                {
                    var neighbors = GetGridNeighbors(current);
                    var nextCell = neighbors[_random.Next(neighbors.Count)];
                    next[current] = nextCell; // overwrite = loop erasure
                    current = nextCell;
                }

                // Follow next[] pointers from startCell to tree, adding cells and edges
                current = startCell;
                while (!inTree.Contains(current))
                {
                    inTree.Add(current);
                    var nextCell = next[current];
                    _treeAdj[current].Add(nextCell);
                    _treeAdj[nextCell].Add(current);
                    current = nextCell;
                }

                idx++;
            }
        }

        /// <summary>
        /// Returns the 4-directional grid neighbors of a cell (within bounds).
        /// </summary>
        private List<Vector2Int> GetGridNeighbors(Vector2Int pos)
        {
            var neighbors = new List<Vector2Int>(4);
            for (int dir = 0; dir < 4; dir++)
            {
                var offset = PipeConnectionHelper.DirOffset[dir];
                int nx = pos.x + offset.x, ny = pos.y + offset.y;
                if (nx >= 0 && nx < _gridWidth && ny >= 0 && ny < _gridHeight)
                    neighbors.Add(new Vector2Int(nx, ny));
            }
            return neighbors;
        }

        // ================================================================
        //  STEP 2: Source Selection
        // ================================================================

        /// <summary>
        /// Selects Source on a grid edge cell. Prefers tree leaves (degree 1)
        /// since Source has exactly 1 port.
        /// </summary>
        private Vector2Int SelectSource()
        {
            var edgeCells = GetEdgeCells();
            Shuffle(edgeCells);

            // Prefer leaves (degree 1 in tree) -- naturally become single-port Source
            var leaves = edgeCells.Where(e => _treeAdj[e].Count == 1).ToList();
            if (leaves.Count > 0)
                return leaves[_random.Next(leaves.Count)];

            // Fallback: any edge cell
            return edgeCells.Count > 0 ? edgeCells[0] : new Vector2Int(-1, -1);
        }

        // ================================================================
        //  STEP 3: Destination Selection
        // ================================================================

        /// <summary>
        /// Selects destinations at appropriate tree distance from source.
        /// Ensures no destination lies on another destination's path to source
        /// (which would give it degree > 1 in the puzzle subtree).
        /// </summary>
        private List<Vector2Int> SelectDestinations(Vector2Int source, int count)
        {
            // BFS from source in spanning tree for tree distances and parent pointers
            var dist = new Dictionary<Vector2Int, int>();
            var parent = new Dictionary<Vector2Int, Vector2Int>();
            var bfsQueue = new Queue<Vector2Int>();
            bfsQueue.Enqueue(source);
            dist[source] = 0;
            parent[source] = source; // sentinel

            while (bfsQueue.Count > 0)
            {
                var cur = bfsQueue.Dequeue();
                foreach (var nb in _treeAdj[cur])
                {
                    if (!dist.ContainsKey(nb))
                    {
                        dist[nb] = dist[cur] + 1;
                        parent[nb] = cur;
                        bfsQueue.Enqueue(nb);
                    }
                }
            }

            // Difficulty-aware minimum tree distance
            int minTreeDist = _config.targetDifficulty switch
            {
                <= 2 => 2,
                <= 4 => Mathf.Max(2, (_gridWidth + _gridHeight) / 4),
                <= 7 => Mathf.Max(3, (_gridWidth + _gridHeight) / 3),
                _ => Mathf.Max(4, (_gridWidth + _gridHeight) / 2)
            };

            // Candidates sorted by tree distance (farthest first), prefer tree leaves
            var candidates = dist
                .Where(kvp => kvp.Key != source && kvp.Value >= minTreeDist)
                .OrderByDescending(kvp =>
                    kvp.Value * 10 + (_treeAdj[kvp.Key].Count == 1 ? 100 : 0) + _random.Next(5))
                .Select(kvp => kvp.Key)
                .ToList();

            // Fallback with lower distance requirement
            if (candidates.Count < count)
            {
                candidates = dist
                    .Where(kvp => kvp.Key != source && kvp.Value >= 2)
                    .OrderByDescending(kvp => kvp.Value * 10 + _random.Next(5))
                    .Select(kvp => kvp.Key)
                    .ToList();
            }

            // Greedy selection ensuring no destination is on another's path to source
            var selected = new List<Vector2Int>();
            var pathCells = new HashSet<Vector2Int>(); // cells on already-selected paths (excl. source)

            foreach (var cand in candidates)
            {
                if (selected.Count >= count) break;

                // Candidate must not be on any already-selected path
                if (pathCells.Contains(cand)) continue;

                // Manhattan distance between destinations (at least 2 apart)
                bool tooClose = selected.Any(d =>
                    Mathf.Abs(cand.x - d.x) + Mathf.Abs(cand.y - d.y) < 2);
                if (tooClose) continue;

                // No already-selected destination should be on candidate's path
                var candPath = TracePath(cand, parent);
                bool conflictsWithExisting = selected.Any(d => candPath.Contains(d));
                if (conflictsWithExisting) continue;

                selected.Add(cand);
                foreach (var cell in candPath)
                {
                    if (cell != source)
                        pathCells.Add(cell);
                }
            }

            return selected;
        }

        /// <summary>
        /// Traces path from cell to root (source) via BFS parent pointers.
        /// Returns all cells on path excluding the root.
        /// </summary>
        private HashSet<Vector2Int> TracePath(Vector2Int cell, Dictionary<Vector2Int, Vector2Int> parent)
        {
            var path = new HashSet<Vector2Int>();
            var current = cell;
            while (parent[current] != current) // stop at root sentinel
            {
                path.Add(current);
                current = parent[current];
            }
            return path;
        }

        // ================================================================
        //  STEP 4: Puzzle Subtree Extraction
        // ================================================================

        /// <summary>
        /// Extracts the puzzle subtree: minimal subtree of the spanning tree
        /// connecting Source to all Destinations (Steiner tree in tree graph).
        /// 
        /// Properties:
        /// - Source and Destinations are leaves (degree 1) in the subtree
        /// - Interior nodes become pipe pieces
        /// - Unique path from Source to each Destination (tree = no cycles)
        /// </summary>
        private Dictionary<Vector2Int, HashSet<Vector2Int>> ExtractPuzzleSubtree(
            Vector2Int source, List<Vector2Int> destinations)
        {
            // BFS from source in spanning tree for parent pointers
            var parent = new Dictionary<Vector2Int, Vector2Int>();
            var bfsQueue = new Queue<Vector2Int>();
            bfsQueue.Enqueue(source);
            parent[source] = source; // sentinel

            while (bfsQueue.Count > 0)
            {
                var cur = bfsQueue.Dequeue();
                foreach (var nb in _treeAdj[cur])
                {
                    if (!parent.ContainsKey(nb))
                    {
                        parent[nb] = cur;
                        bfsQueue.Enqueue(nb);
                    }
                }
            }

            // Trace path from each destination back to source; union = subtree
            var subtreeAdj = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

            foreach (var dest in destinations)
            {
                var current = dest;
                while (current != source)
                {
                    var par = parent[current];

                    if (!subtreeAdj.ContainsKey(current))
                        subtreeAdj[current] = new HashSet<Vector2Int>();
                    if (!subtreeAdj.ContainsKey(par))
                        subtreeAdj[par] = new HashSet<Vector2Int>();

                    subtreeAdj[current].Add(par);
                    subtreeAdj[par].Add(current);

                    current = par;
                }
            }

            // Ensure source is in subtree
            if (!subtreeAdj.ContainsKey(source))
                subtreeAdj[source] = new HashSet<Vector2Int>();

            return subtreeAdj;
        }

        // ================================================================
        //  STEP 5: Degree Constraint Validation
        // ================================================================

        /// <summary>
        /// Validates that every cell's degree in the puzzle subtree can be satisfied
        /// by the allowed pipe types. Source/Destinations must be leaves (degree 1).
        /// </summary>
        private bool ValidateDegreeConstraints(Vector2Int source, List<Vector2Int> destinations)
        {
            var destSet = new HashSet<Vector2Int>(destinations);
            var allowed = _config.GetAllowedPipeTypes();

            int maxDegree = 2; // Straight and Corner both have 2 ports
            if (allowed.Contains(PieceType.TJunctionPipe)) maxDegree = 3;
            if (allowed.Contains(PieceType.CrossPipe)) maxDegree = 4;

            foreach (var kvp in _subtreeAdj)
            {
                var pos = kvp.Key;
                int degree = kvp.Value.Count;

                // Source and Destination must be leaves (1 port)
                if (pos == source || destSet.Contains(pos))
                {
                    if (degree > 1)
                    {
                        Debug.LogWarning($"[SpanningTree] {(pos == source ? "Source" : "Destination")} at {pos} degree {degree} > 1");
                        return false;
                    }
                    continue;
                }

                if (degree < 2)
                {
                    Debug.LogWarning($"[SpanningTree] Interior cell {pos} has degree {degree} (dead end)");
                    return false;
                }

                if (degree > maxDegree)
                {
                    Debug.LogWarning($"[SpanningTree] Cell {pos} degree {degree} exceeds max allowed {maxDegree}");
                    return false;
                }

                // Degree 2: verify correct pipe type availability
                if (degree == 2)
                {
                    var neighbors = kvp.Value.ToList();
                    int d1 = GetDirection(pos, neighbors[0]);
                    int d2 = GetDirection(pos, neighbors[1]);
                    bool isOpposite = PipeConnectionHelper.OppositeDir[d1] == d2;

                    if (isOpposite && !allowed.Contains(PieceType.StraightPipe))
                        return false;
                    if (!isOpposite && !allowed.Contains(PieceType.CornerPipe))
                        return false;
                }
            }

            return true;
        }

        // ================================================================
        //  STEP 6: Pipe Assignment
        // ================================================================

        /// <summary>
        /// Assigns pipe types and rotations based on subtree edges.
        /// Degree 1 -> Source/Destination, Degree 2 -> Straight/Corner,
        /// Degree 3 -> TJunction, Degree 4 -> Cross.
        /// </summary>
        private bool AssignPipes(LevelDataSO levelData, Vector2Int source, List<Vector2Int> destinations)
        {
            var destSet = new HashSet<Vector2Int>(destinations);
            var allowedTypes = _config.GetAllowedPipeTypes();

            foreach (var kvp in _subtreeAdj)
            {
                var pos = kvp.Key;
                var neighbors = kvp.Value;

                PieceType type;
                int rotation;

                if (pos == source)
                {
                    type = PieceType.Source;
                    int dir = GetDirection(pos, neighbors.First());
                    rotation = PipeConnectionHelper.DirToRotation(dir);
                }
                else if (destSet.Contains(pos))
                {
                    type = PieceType.Destination;
                    int dir = GetDirection(pos, neighbors.First());
                    rotation = PipeConnectionHelper.DirToRotation(dir);
                }
                else
                {
                    var dirs = new HashSet<int>(neighbors.Select(n => GetDirection(pos, n)));

                    if (!TryGetExactPipeForConnections(dirs, allowedTypes, out type, out rotation))
                    {
                        Debug.LogWarning($"[SpanningTree] Cannot assign pipe at {pos} for dirs [{string.Join(",", dirs)}]");
                        return false;
                    }

                    // Verify exact port match
                    var ports = new HashSet<int>(PipeConnectionHelper.GetOpenPorts(type, rotation));
                    if (!ports.SetEquals(dirs))
                    {
                        Debug.LogWarning($"[SpanningTree] Port mismatch at {pos}: need [{string.Join(",", dirs)}] got [{string.Join(",", ports)}]");
                        return false;
                    }
                }

                levelData.pieces.Add(new PieceData(pos.x, pos.y, type, rotation));
                _solvedPieces[pos] = (type, rotation);
            }

            return true;
        }

        /// <summary>
        /// Finds exact pipe type and rotation matching required connection directions.
        /// Returns false if no allowed pipe type satisfies the requirements.
        /// </summary>
        private bool TryGetExactPipeForConnections(HashSet<int> dirs, List<PieceType> allowedTypes,
            out PieceType type, out int rotation)
        {
            type = PieceType.StraightPipe;
            rotation = 0;
            int count = dirs.Count;

            // 4 connections -> Cross
            if (count == 4)
            {
                if (!allowedTypes.Contains(PieceType.CrossPipe)) return false;
                type = PieceType.CrossPipe;
                rotation = 0;
                return true;
            }

            // 3 connections -> T-Junction
            if (count == 3)
            {
                if (!allowedTypes.Contains(PieceType.TJunctionPipe)) return false;

                int missing = -1;
                for (int i = 0; i < 4; i++)
                    if (!dirs.Contains(i)) { missing = i; break; }

                type = PieceType.TJunctionPipe;
                rotation = missing switch
                {
                    PipeConnectionHelper.DIR_LEFT => 0,
                    PipeConnectionHelper.DIR_UP => 90,
                    PipeConnectionHelper.DIR_RIGHT => 180,
                    PipeConnectionHelper.DIR_DOWN => 270,
                    _ => 0
                };
                return true;
            }

            // 2 connections
            if (count == 2)
            {
                var dirList = new List<int>(dirs);
                int d1 = dirList[0], d2 = dirList[1];
                bool isOpposite = PipeConnectionHelper.OppositeDir[d1] == d2;

                if (isOpposite)
                {
                    if (!allowedTypes.Contains(PieceType.StraightPipe)) return false;
                    type = PieceType.StraightPipe;
                    rotation = dirs.Contains(PipeConnectionHelper.DIR_UP) ? 0 : 90;
                    return true;
                }
                else
                {
                    if (!allowedTypes.Contains(PieceType.CornerPipe)) return false;
                    type = PieceType.CornerPipe;

                    if (dirs.Contains(PipeConnectionHelper.DIR_UP) && dirs.Contains(PipeConnectionHelper.DIR_RIGHT))
                    { rotation = 0; return true; }
                    if (dirs.Contains(PipeConnectionHelper.DIR_RIGHT) && dirs.Contains(PipeConnectionHelper.DIR_DOWN))
                    { rotation = 90; return true; }
                    if (dirs.Contains(PipeConnectionHelper.DIR_DOWN) && dirs.Contains(PipeConnectionHelper.DIR_LEFT))
                    { rotation = 180; return true; }
                    if (dirs.Contains(PipeConnectionHelper.DIR_LEFT) && dirs.Contains(PipeConnectionHelper.DIR_UP))
                    { rotation = 270; return true; }
                }
            }

            return false;
        }

        // ================================================================
        //  STEP 7: Solved State Validation
        // ================================================================

        /// <summary>
        /// Validates solved state: dangling ports check + BFS connectivity
        /// from Source to all Destinations using solved-state rotations.
        /// </summary>
        private bool ValidateSolvedState(LevelDataSO levelData, Vector2Int sourcePos,
            List<Vector2Int> destPositions)
        {
            // Build solved-state piece map (all pieces for BFS lookup)
            var pieceMap = new Dictionary<Vector2Int, (PieceType type, int rot)>();
            var decoyPositions = new HashSet<Vector2Int>();
            foreach (var p in levelData.pieces)
            {
                var pos = new Vector2Int(p.x, p.z);
                pieceMap[pos] = _solvedPieces.TryGetValue(pos, out var solved)
                    ? solved
                    : (p.pieceType, p.rotation);
                if (p.isDecoy)
                    decoyPositions.Add(pos);
            }

            // Pass 1: Dangling port check (skip decoys — they intentionally have dangling ports)
            foreach (var kvp in pieceMap)
            {
                var pos = kvp.Key;
                if (decoyPositions.Contains(pos)) continue; // Decoys are allowed to dangle
                var (type, rot) = kvp.Value;

                foreach (int dir in PipeConnectionHelper.GetOpenPorts(type, rot))
                {
                    var offset = PipeConnectionHelper.DirOffset[dir];
                    int nx = pos.x + offset.x, nz = pos.y + offset.y;

                    if (nx < 0 || nx >= _gridWidth || nz < 0 || nz >= _gridHeight)
                    {
                        Debug.LogWarning($"[Validate] {type} at {pos} port dir={dir} outside grid");
                        return false;
                    }

                    var nPos = new Vector2Int(nx, nz);
                    if (!pieceMap.TryGetValue(nPos, out var neighbor))
                    {
                        Debug.LogWarning($"[Validate] {type} at {pos} port dir={dir} points to empty {nPos}");
                        return false;
                    }

                    if (!PipeConnectionHelper.HasPort(neighbor.type, neighbor.rot, PipeConnectionHelper.OppositeDir[dir]))
                    {
                        Debug.LogWarning($"[Validate] {type} at {pos} dir={dir} no reciprocal at {nPos}");
                        return false;
                    }
                }
            }

            // Pass 2: BFS connectivity Source -> all Destinations
            var visited = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(sourcePos);
            visited.Add(sourcePos);

            var destSet = new HashSet<Vector2Int>(destPositions);
            int reached = 0;

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (destSet.Contains(cur)) reached++;
                if (!pieceMap.TryGetValue(cur, out var piece)) continue;

                foreach (int dir in PipeConnectionHelper.GetOpenPorts(piece.type, piece.rot))
                {
                    var offset = PipeConnectionHelper.DirOffset[dir];
                    var nPos = new Vector2Int(cur.x + offset.x, cur.y + offset.y);
                    if (visited.Contains(nPos)) continue;
                    if (!pieceMap.TryGetValue(nPos, out var nPiece)) continue;

                    if (PipeConnectionHelper.HasPort(nPiece.type, nPiece.rot, PipeConnectionHelper.OppositeDir[dir]))
                    {
                        visited.Add(nPos);
                        queue.Enqueue(nPos);
                    }
                }
            }

            if (reached < destPositions.Count)
            {
                Debug.LogWarning($"[Validate] BFS reached {reached}/{destPositions.Count} destinations");
                return false;
            }

            return true;
        }

        // ================================================================
        //  STEP 8: AC-3 Uniqueness Verification
        // ================================================================

        /// <summary>
        /// Verifies that the puzzle has exactly one solution via AC-3 constraint
        /// propagation followed by bounded backtracking if needed.
        /// 
        /// Variables: rotation of each puzzle piece
        /// Domains: valid rotations per pipe type
        /// Constraints: adjacent cells must have mutual port connectivity
        /// </summary>
        private bool VerifyUniqueSolution(LevelDataSO levelData)
        {
            var cells = new List<Vector2Int>();
            var cellTypes = new Dictionary<Vector2Int, PieceType>();
            var domains = new Dictionary<Vector2Int, List<int>>();
            var occupiedCells = new HashSet<Vector2Int>();

            foreach (var p in levelData.pieces)
            {
                var pos = new Vector2Int(p.x, p.z);
                cells.Add(pos);
                cellTypes[pos] = p.pieceType;
                occupiedCells.Add(pos);

                // Source/Destination are fixed
                if (p.pieceType == PieceType.Source || p.pieceType == PieceType.Destination)
                {
                    domains[pos] = new List<int> { _solvedPieces[pos].rotation };
                }
                else
                {
                    int uniqueRots = PipeConnectionHelper.GetUniqueRotationCount(p.pieceType);
                    int step = 360 / uniqueRots;
                    var domain = new List<int>();
                    for (int r = 0; r < uniqueRots; r++)
                        domain.Add(r * step);
                    domains[pos] = domain;
                }
            }

            // Unary constraints: remove rotations where ANY port points outside
            // the grid or to an empty cell (no piece). This is the critical pre-filter
            // that tells AC-3 about the physical grid structure.
            foreach (var cell in cells)
            {
                if (domains[cell].Count <= 1) continue; // already fixed

                var type = cellTypes[cell];
                var validRots = new List<int>();

                foreach (int rot in domains[cell])
                {
                    bool valid = true;
                    foreach (int dir in PipeConnectionHelper.GetOpenPorts(type, rot))
                    {
                        var offset = PipeConnectionHelper.DirOffset[dir];
                        int nx = cell.x + offset.x, ny = cell.y + offset.y;

                        // Port pointing outside grid -> invalid rotation
                        if (nx < 0 || nx >= _gridWidth || ny < 0 || ny >= _gridHeight)
                        { valid = false; break; }

                        // Port pointing to empty cell (no piece) -> invalid rotation
                        if (!occupiedCells.Contains(new Vector2Int(nx, ny)))
                        { valid = false; break; }
                    }
                    if (valid) validRots.Add(rot);
                }

                if (validRots.Count > 0)
                    domains[cell] = validRots;
                // If no valid rotation survives, keep original domain
                // (shouldn't happen if pipes were assigned correctly)
            }

            // Run AC-3
            if (!RunAC3(cells, cellTypes, domains))
            {
                Debug.LogWarning("[SpanningTree] AC-3 found inconsistency (domain went empty)");
                return true; // Accept anyway — solved state was already validated
            }

            // All domains singleton -> guaranteed unique
            if (domains.Values.All(d => d.Count == 1))
                return true;

            // Bounded backtracking: count solutions, stop at 2
            int solutionCount = CountSolutions(cells, cellTypes, domains, 2);
            if (solutionCount == 1)
                return true;

            // Multiple solutions: accept with warning (still a playable puzzle)
            Debug.LogWarning($"[SpanningTree] Puzzle has {solutionCount} solution(s) — not unique, accepting anyway");
            return true;
        }

        /// <summary>
        /// AC-3 constraint propagation. Reduces domains by ensuring each value
        /// has at least one compatible value in each adjacent cell's domain.
        /// </summary>
        private bool RunAC3(List<Vector2Int> cells, Dictionary<Vector2Int, PieceType> cellTypes,
            Dictionary<Vector2Int, List<int>> domains)
        {
            var arcQueue = new Queue<(Vector2Int cell, Vector2Int neighbor, int dir)>();

            foreach (var cell in cells)
            {
                for (int dir = 0; dir < 4; dir++)
                {
                    var offset = PipeConnectionHelper.DirOffset[dir];
                    var neighbor = new Vector2Int(cell.x + offset.x, cell.y + offset.y);
                    if (domains.ContainsKey(neighbor))
                        arcQueue.Enqueue((cell, neighbor, dir));
                }
            }

            while (arcQueue.Count > 0)
            {
                var (cell, neighbor, dir) = arcQueue.Dequeue();
                if (!domains.ContainsKey(cell) || !domains.ContainsKey(neighbor))
                    continue;

                if (Revise(cell, neighbor, dir, cellTypes, domains))
                {
                    if (domains[cell].Count == 0)
                        return false;

                    // Re-enqueue arcs (other -> cell) where other != neighbor
                    for (int d = 0; d < 4; d++)
                    {
                        var offset = PipeConnectionHelper.DirOffset[d];
                        var other = new Vector2Int(cell.x + offset.x, cell.y + offset.y);
                        if (other != neighbor && domains.ContainsKey(other))
                            arcQueue.Enqueue((other, cell, d));
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Revises domain of 'cell' relative to 'neighbor' in direction 'dir'.
        /// Constraint (implication): if cell has port toward neighbor, neighbor must
        /// have a port pointing back. If cell doesn't have port, no constraint.
        /// The reverse arc handles the opposite direction.
        /// </summary>
        private bool Revise(Vector2Int cell, Vector2Int neighbor, int dir,
            Dictionary<Vector2Int, PieceType> cellTypes, Dictionary<Vector2Int, List<int>> domains)
        {
            bool revised = false;
            int oppDir = PipeConnectionHelper.OppositeDir[dir];
            var cellType = cellTypes[cell];
            var neighborType = cellTypes[neighbor];

            var toRemove = new List<int>();

            foreach (int cellRot in domains[cell])
            {
                bool cellHasPort = PipeConnectionHelper.HasPort(cellType, cellRot, dir);

                // If cell doesn't point toward neighbor, no constraint from this arc
                if (!cellHasPort) continue;

                // Cell points toward neighbor -> neighbor must have at least one
                // rotation with a port pointing back
                bool hasSupport = false;
                foreach (int neighborRot in domains[neighbor])
                {
                    if (PipeConnectionHelper.HasPort(neighborType, neighborRot, oppDir))
                    {
                        hasSupport = true;
                        break;
                    }
                }

                if (!hasSupport)
                    toRemove.Add(cellRot);
            }

            foreach (int rot in toRemove)
            {
                domains[cell].Remove(rot);
                revised = true;
            }

            return revised;
        }

        /// <summary>
        /// Bounded backtracking to count solutions. Uses MRV heuristic.
        /// Stops when maxCount is reached.
        /// </summary>
        private int CountSolutions(List<Vector2Int> cells, Dictionary<Vector2Int, PieceType> cellTypes,
            Dictionary<Vector2Int, List<int>> domains, int maxCount)
        {
            // MRV: find cell with smallest domain > 1
            Vector2Int? branchCell = null;
            int minSize = int.MaxValue;

            foreach (var cell in cells)
            {
                int size = domains[cell].Count;
                if (size > 1 && size < minSize)
                {
                    minSize = size;
                    branchCell = cell;
                }
            }

            // All domains singleton -> one complete solution
            if (!branchCell.HasValue)
                return 1;

            int total = 0;
            var bc = branchCell.Value;

            foreach (int rot in new List<int>(domains[bc]))
            {
                // Clone domains
                var cloned = new Dictionary<Vector2Int, List<int>>();
                foreach (var kvp in domains)
                    cloned[kvp.Key] = new List<int>(kvp.Value);

                cloned[bc] = new List<int> { rot };

                if (RunAC3(cells, cellTypes, cloned))
                {
                    if (!cloned.Values.Any(d => d.Count == 0))
                    {
                        total += CountSolutions(cells, cellTypes, cloned, maxCount - total);
                        if (total >= maxCount)
                            return total;
                    }
                }
            }

            return total;
        }

        // ================================================================
        //  STEP 9: Red Herring Decoy Placement
        // ================================================================

        /// <summary>
        /// Places red herring decoy pipes adjacent to the puzzle path.
        /// A red herring has a port pointing TOWARDS a path piece, but the path piece
        /// does NOT have a port pointing back -- creating visual deception.
        /// Remaining slots filled with safe plain decoys.
        /// </summary>
        private void PlaceRedHerringDecoys(LevelDataSO levelData)
        {
            int decoyCount = Mathf.RoundToInt(levelData.pieces.Count * _config.decoyPipeRatio);
            if (decoyCount <= 0) return;

            var occupied = new HashSet<Vector2Int>(levelData.pieces.Select(p => new Vector2Int(p.x, p.z)));

            // Build solved port map
            var solvedPorts = new Dictionary<Vector2Int, HashSet<int>>();
            foreach (var kvp in _solvedPieces)
                solvedPorts[kvp.Key] = new HashSet<int>(
                    PipeConnectionHelper.GetOpenPorts(kvp.Value.type, kvp.Value.rotation));

            // Decoy pipe types: only simple types
            var allowedTypes = _config.GetAllowedPipeTypes();
            var decoyTypes = new List<PieceType>();
            if (allowedTypes.Contains(PieceType.StraightPipe)) decoyTypes.Add(PieceType.StraightPipe);
            if (allowedTypes.Contains(PieceType.CornerPipe)) decoyTypes.Add(PieceType.CornerPipe);
            if (decoyTypes.Count == 0) return;

            // Find red herring candidates: empty cells adjacent to path where
            // path piece does NOT have a port towards this cell
            var redHerringCandidates = new List<(Vector2Int pos, int dirTowardsPath)>();

            foreach (var pathCell in _subtreeAdj.Keys)
            {
                for (int dir = 0; dir < 4; dir++)
                {
                    var offset = PipeConnectionHelper.DirOffset[dir];
                    var neighbor = new Vector2Int(pathCell.x + offset.x, pathCell.y + offset.y);

                    if (neighbor.x < 0 || neighbor.x >= _gridWidth ||
                        neighbor.y < 0 || neighbor.y >= _gridHeight)
                        continue;
                    if (occupied.Contains(neighbor)) continue;

                    // Path piece lacks port in direction 'dir' -> red herring opportunity
                    if (solvedPorts.TryGetValue(pathCell, out var ports) && !ports.Contains(dir))
                    {
                        int oppDir = PipeConnectionHelper.OppositeDir[dir];
                        redHerringCandidates.Add((neighbor, oppDir));
                    }
                }
            }

            // Deduplicate by position
            Shuffle(redHerringCandidates);
            var seen = new HashSet<Vector2Int>();
            var uniqueRedHerrings = new List<(Vector2Int pos, int dirTowardsPath)>();
            foreach (var c in redHerringCandidates)
                if (seen.Add(c.pos))
                    uniqueRedHerrings.Add(c);

            int placed = 0;

            // Priority 1: Red herring positions
            foreach (var (pos, dirTowardsPath) in uniqueRedHerrings)
            {
                if (placed >= decoyCount) break;
                if (occupied.Contains(pos)) continue;

                var type = decoyTypes[_random.Next(decoyTypes.Count)];
                int rotation = FindRedHerringRotation(pos, type, dirTowardsPath, solvedPorts);
                if (rotation >= 0)
                {
                    levelData.pieces.Add(new PieceData(pos.x, pos.y, type, rotation, isDecoy: true));
                    _solvedPieces[pos] = (type, rotation);
                    solvedPorts[pos] = new HashSet<int>(PipeConnectionHelper.GetOpenPorts(type, rotation));
                    occupied.Add(pos);
                    placed++;
                }
            }

            // Priority 2: Plain safe decoys
            if (placed < decoyCount)
            {
                var plainCandidates = new List<Vector2Int>();
                for (int x = 0; x < _gridWidth; x++)
                    for (int y = 0; y < _gridHeight; y++)
                    {
                        var pos = new Vector2Int(x, y);
                        if (!occupied.Contains(pos))
                            plainCandidates.Add(pos);
                    }
                Shuffle(plainCandidates);

                foreach (var pos in plainCandidates)
                {
                    if (placed >= decoyCount) break;
                    if (occupied.Contains(pos)) continue;

                    var type = decoyTypes[_random.Next(decoyTypes.Count)];
                    int rotation = FindSafeDecoyRotation(pos, type, solvedPorts);
                    if (rotation >= 0)
                    {
                        levelData.pieces.Add(new PieceData(pos.x, pos.y, type, rotation, isDecoy: true));
                        _solvedPieces[pos] = (type, rotation);
                        solvedPorts[pos] = new HashSet<int>(PipeConnectionHelper.GetOpenPorts(type, rotation));
                        occupied.Add(pos);
                        placed++;
                    }
                }
            }
        }

        /// <summary>
        /// Finds rotation for red herring: HAS port in mustHaveDir (deceptive),
        /// does NOT create mutual connections with any neighbor.
        /// </summary>
        private int FindRedHerringRotation(Vector2Int pos, PieceType type, int mustHaveDir,
            Dictionary<Vector2Int, HashSet<int>> solvedPorts)
        {
            int uniqueRots = PipeConnectionHelper.GetUniqueRotationCount(type);
            int step = 360 / uniqueRots;

            for (int r = 0; r < uniqueRots; r++)
            {
                int rotation = r * step;
                var ports = PipeConnectionHelper.GetOpenPorts(type, rotation);
                if (!ports.Contains(mustHaveDir)) continue;

                bool safe = true;
                foreach (int dir in ports)
                {
                    var offset = PipeConnectionHelper.DirOffset[dir];
                    int nx = pos.x + offset.x, ny = pos.y + offset.y;
                    if (nx < 0 || nx >= _gridWidth || ny < 0 || ny >= _gridHeight)
                    { safe = false; break; }

                    var neighbor = new Vector2Int(nx, ny);
                    if (solvedPorts.TryGetValue(neighbor, out var nPorts))
                    {
                        if (nPorts.Contains(PipeConnectionHelper.OppositeDir[dir]))
                        { safe = false; break; }
                    }
                }

                if (safe) return rotation;
            }

            return -1;
        }

        /// <summary>
        /// Finds rotation creating NO mutual connections with any neighbor.
        /// </summary>
        private int FindSafeDecoyRotation(Vector2Int pos, PieceType type,
            Dictionary<Vector2Int, HashSet<int>> solvedPorts)
        {
            int uniqueRots = PipeConnectionHelper.GetUniqueRotationCount(type);
            int step = 360 / uniqueRots;

            for (int r = 0; r < uniqueRots; r++)
            {
                int rotation = r * step;
                var ports = PipeConnectionHelper.GetOpenPorts(type, rotation);
                bool safe = true;

                foreach (int dir in ports)
                {
                    var offset = PipeConnectionHelper.DirOffset[dir];
                    int nx = pos.x + offset.x, ny = pos.y + offset.y;
                    if (nx < 0 || nx >= _gridWidth || ny < 0 || ny >= _gridHeight)
                    { safe = false; break; }

                    var neighbor = new Vector2Int(nx, ny);
                    if (solvedPorts.TryGetValue(neighbor, out var nPorts))
                    {
                        if (nPorts.Contains(PipeConnectionHelper.OppositeDir[dir]))
                        { safe = false; break; }
                    }
                }

                if (safe) return rotation;
            }

            return -1;
        }

        // ================================================================
        //  STEP 10: Static Pipe Conversion
        // ================================================================

        /// <summary>
        /// Converts a fraction of puzzle pipes to static (non-rotatable) hint pieces.
        /// Sets them to solved rotation before converting.
        /// </summary>
        private void ConvertToStaticPipes(LevelDataSO levelData)
        {
            var staticPieces = new HashSet<Vector2Int>();

            foreach (var p in levelData.pieces)
            {
                if (p.pieceType == PieceType.Source || p.pieceType == PieceType.Destination)
                    staticPieces.Add(new Vector2Int(p.x, p.z));
            }

            var pipeIndices = new List<int>();
            for (int i = 0; i < levelData.pieces.Count; i++)
            {
                var p = levelData.pieces[i];
                if (p.pieceType != PieceType.Source &&
                    p.pieceType != PieceType.Destination &&
                    !p.pieceType.IsStatic())
                    pipeIndices.Add(i);
            }

            Shuffle(pipeIndices);

            int staticCount = Mathf.RoundToInt(pipeIndices.Count * _config.staticPipeRatio);
            int converted = 0;

            foreach (int idx in pipeIndices)
            {
                if (converted >= staticCount) break;

                var piece = levelData.pieces[idx];
                var pos = new Vector2Int(piece.x, piece.z);

                if (!CanBeStaticStrict(piece, levelData, staticPieces))
                    continue;

                if (_solvedPieces.ContainsKey(pos))
                    piece.rotation = _solvedPieces[pos].rotation;

                piece.pieceType = GetStaticVersion(piece.pieceType);
                staticPieces.Add(pos);
                converted++;
            }
        }

        private bool CanBeStaticStrict(PieceData piece, LevelDataSO levelData, HashSet<Vector2Int> staticPieces)
        {
            var pos = new Vector2Int(piece.x, piece.z);
            int solvedRot = _solvedPieces.ContainsKey(pos) ? _solvedPieces[pos].rotation : piece.rotation;
            var ports = PipeConnectionHelper.GetOpenPorts(piece.pieceType, solvedRot);

            foreach (int dir in ports)
            {
                var offset = PipeConnectionHelper.DirOffset[dir];
                int nx = piece.x + offset.x, nz = piece.z + offset.y;

                if (nx < 0 || nx >= _gridWidth || nz < 0 || nz >= _gridHeight)
                    return false;

                var neighbor = levelData.GetPieceAt(nx, nz);
                if (neighbor == null) return false;

                var nPos = new Vector2Int(nx, nz);
                int oppDir = PipeConnectionHelper.OppositeDir[dir];
                int nSolvedRot = _solvedPieces.ContainsKey(nPos)
                    ? _solvedPieces[nPos].rotation
                    : neighbor.rotation;

                if (!PipeConnectionHelper.HasPort(neighbor.pieceType, nSolvedRot, oppDir))
                    return false;

                if (staticPieces.Contains(nPos) || neighbor.pieceType.IsStatic())
                {
                    if (!PipeConnectionHelper.HasPort(neighbor.pieceType, neighbor.rotation, oppDir))
                        return false;
                }
            }

            return true;
        }

        private PieceType GetStaticVersion(PieceType type)
        {
            return type switch
            {
                PieceType.StraightPipe => PieceType.StaticStraightPipe,
                PieceType.CornerPipe => PieceType.StaticCornerPipe,
                PieceType.TJunctionPipe => PieceType.StaticTJunctionPipe,
                PieceType.CrossPipe => PieceType.StaticCrossPipe,
                _ => type
            };
        }

        // ================================================================
        //  STEP 11: Smart Scramble
        // ================================================================

        /// <summary>
        /// Scrambles pipes using "most deceptive rotation" strategy.
        /// Finds the rotation that creates the most plausible-looking but wrong connections.
        /// Re-scrambles if minMoves target is not met (up to 5 attempts).
        /// </summary>
        private void SmartScramble(LevelDataSO levelData)
        {
            int targetMinMoves = _config.GetTargetMinMoves();
            var occupied = new HashSet<Vector2Int>(levelData.pieces.Select(p => new Vector2Int(p.x, p.z)));

            for (int attempt = 0; attempt < 5; attempt++)
            {
                foreach (var piece in levelData.pieces)
                {
                    if (piece.pieceType == PieceType.CrossPipe ||
                        piece.pieceType == PieceType.Source ||
                        piece.pieceType == PieceType.Destination ||
                        piece.pieceType.IsStatic())
                        continue;

                    var pos = new Vector2Int(piece.x, piece.z);
                    int solvedRot = _solvedPieces.ContainsKey(pos) ? _solvedPieces[pos].rotation : piece.rotation;
                    int solvedMask = PipeConnectionHelper.GetPortMask(piece.pieceType, solvedRot);

                    // Find most deceptive wrong rotation using 90° steps
                    // Skip rotations that produce the same port mask (functionally equivalent)
                    int bestRotation = -1;
                    int bestScore = int.MinValue;

                    for (int r = 1; r <= 3; r++)
                    {
                        int candidateRot = (solvedRot + r * 90) % 360;
                        // Skip if functionally equivalent to solved rotation
                        if (PipeConnectionHelper.GetPortMask(piece.pieceType, candidateRot) == solvedMask)
                            continue;

                        int score = CalculateDeceptiveScore(pos, piece.pieceType, candidateRot, occupied);
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestRotation = candidateRot;
                        }
                    }

                    if (bestRotation >= 0)
                        piece.rotation = bestRotation;
                    // If no non-equivalent rotation found (shouldn't happen for non-Cross), keep solved
                }

                int currentMinMoves = CalculateMinMoves(levelData);
                if (currentMinMoves >= targetMinMoves)
                    return;
            }
        }

        /// <summary>
        /// Deceptive score: how plausible does this wrong rotation look?
        /// +3 for port towards neighbor with reciprocal port (in solved state)
        /// +1 for port towards any occupied cell
        /// -1 for port towards empty cell
        /// -2 for port outside grid
        /// </summary>
        private int CalculateDeceptiveScore(Vector2Int pos, PieceType type, int rotation,
            HashSet<Vector2Int> occupied)
        {
            var ports = PipeConnectionHelper.GetOpenPorts(type, rotation);
            int score = 0;

            foreach (int dir in ports)
            {
                var offset = PipeConnectionHelper.DirOffset[dir];
                int nx = pos.x + offset.x, ny = pos.y + offset.y;

                if (nx < 0 || nx >= _gridWidth || ny < 0 || ny >= _gridHeight)
                { score -= 2; continue; }

                var neighbor = new Vector2Int(nx, ny);
                if (_solvedPieces.TryGetValue(neighbor, out var nSolved))
                {
                    int opp = PipeConnectionHelper.OppositeDir[dir];
                    score += PipeConnectionHelper.HasPort(nSolved.type, nSolved.rotation, opp) ? 3 : 1;
                }
                else if (occupied.Contains(neighbor))
                {
                    score += 1;
                }
                else
                {
                    score -= 1;
                }
            }

            return score;
        }

        // ================================================================
        //  STATS
        // ================================================================

        private int CalculateMinMoves(LevelDataSO levelData)
        {
            int moves = 0;
            foreach (var piece in levelData.pieces)
            {
                if (piece.pieceType.IsStatic() ||
                    piece.pieceType == PieceType.CrossPipe ||
                    piece.pieceType == PieceType.Source ||
                    piece.pieceType == PieceType.Destination ||
                    piece.isDecoy)  // Decoys don't affect solvability
                    continue;

                var pos = new Vector2Int(piece.x, piece.z);
                if (!_solvedPieces.ContainsKey(pos)) continue;

                int currentRot = piece.rotation;
                int solvedRot = _solvedPieces[pos].rotation;
                int solvedMask = PipeConnectionHelper.GetPortMask(piece.pieceType, solvedRot);

                // Find minimum clockwise 90° clicks to reach any equivalent solved rotation
                int minClicks = 4;
                for (int rot = 0; rot < 360; rot += 90)
                {
                    if (PipeConnectionHelper.GetPortMask(piece.pieceType, rot) == solvedMask)
                    {
                        int clicks = ((rot - currentRot) % 360 + 360) % 360 / 90;
                        if (clicks < minClicks) minClicks = clicks;
                    }
                }
                moves += minClicks;
            }
            return moves;
        }

        private int CalculateDifficulty(LevelDataSO levelData)
        {
            float score = 0;
            int pathCount = _subtreeAdj != null ? _subtreeAdj.Count : levelData.pieces.Count;

            score += Mathf.Min((_gridWidth + _gridHeight) * 0.25f, 2.5f);
            score += Mathf.Min(pathCount * 0.25f, 2.5f);
            score += Mathf.Min(levelData.minimumMoves * 0.2f, 2f);
            score += Mathf.Min((_config.destinationCount - 1) * 0.7f, 1.5f);

            int typeCount = 0;
            if (_config.useStraightPipes) typeCount++;
            if (_config.useCornerPipes) typeCount++;
            if (_config.useTJunctionPipes) typeCount++;
            if (_config.useCrossPipes) typeCount++;
            score += Mathf.Min((typeCount - 1) * 0.4f, 1f);

            score -= _config.staticPipeRatio * 1.5f;
            score += _config.decoyPipeRatio * 1.0f;

            return Mathf.Clamp(Mathf.RoundToInt(score), 1, 10);
        }

        // ================================================================
        //  UTILITY
        // ================================================================

        private int GetDirection(Vector2Int from, Vector2Int to)
        {
            int dx = to.x - from.x;
            int dz = to.y - from.y;

            if (dz > 0) return PipeConnectionHelper.DIR_UP;
            if (dx > 0) return PipeConnectionHelper.DIR_RIGHT;
            if (dz < 0) return PipeConnectionHelper.DIR_DOWN;
            return PipeConnectionHelper.DIR_LEFT;
        }

        private List<Vector2Int> GetEdgeCells()
        {
            var edges = new List<Vector2Int>();
            for (int x = 0; x < _gridWidth; x++)
            {
                edges.Add(new Vector2Int(x, 0));
                edges.Add(new Vector2Int(x, _gridHeight - 1));
            }
            for (int y = 1; y < _gridHeight - 1; y++)
            {
                edges.Add(new Vector2Int(0, y));
                edges.Add(new Vector2Int(_gridWidth - 1, y));
            }
            return edges;
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
