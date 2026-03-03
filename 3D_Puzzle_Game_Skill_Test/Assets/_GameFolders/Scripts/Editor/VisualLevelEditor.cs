using UnityEngine;
using UnityEditor;
using BufoGames.Data;
using BufoGames.Generation;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BufoGames.Editor
{
    [CustomEditor(typeof(LevelDataSO))]
    public class VisualLevelEditor : UnityEditor.Editor
    {
        private LevelDataSO levelData;
        private Vector2Int selectedCell = new Vector2Int(-1, -1);
        private bool showCellDetail = false;
        private bool showGeneratorPanel = false;
        private bool showRawData = false;
        private Vector2 rawDataScroll;
        private bool isValidationRunning = false;
        
        private LevelGeneratorConfig generatorConfig = new LevelGeneratorConfig();
        private ProceduralLevelGenerator proceduralGenerator = new ProceduralLevelGenerator();
        
        private const float CELL_SIZE = 50f;
        private const float CELL_SPACING = 2f;
        
        private readonly Color emptyColor = new Color(0.85f, 0.85f, 0.85f);
        private readonly Color selectedColor = new Color(0.3f, 0.7f, 1f);
        private readonly Color sourceColor = new Color(0.2f, 0.6f, 1f);
        private readonly Color destinationColor = new Color(1f, 0.7f, 0.2f);
        private readonly Color pipeColor = new Color(0.5f, 0.8f, 0.5f);
        
        private GUIStyle centeredLabelStyle;
        private GUIStyle largeLabelStyle;
        private GUIStyle rotationLabelStyle;
        
        private void OnEnable()
        {
            levelData = (LevelDataSO)target;
            SyncConfigWithLevelData();
        }
        
        private void SyncConfigWithLevelData()
        {
            generatorConfig.gridWidth = levelData.gridWidth;
            generatorConfig.gridHeight = levelData.gridHeight;
        }
        
        private void InitStyles()
        {
            if (centeredLabelStyle == null)
            {
                centeredLabelStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    wordWrap = false,
                    clipping = TextClipping.Clip
                };
                
                largeLabelStyle = new GUIStyle(centeredLabelStyle)
                {
                    fontSize = 24
                };
                
                rotationLabelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.LowerCenter,
                    fontSize = 9
                };
            }
        }
        
        public override void OnInspectorGUI()
        {
            InitStyles();
            serializedObject.Update();
            
            DrawHeader();
            DrawBasicProperties();
            
            EditorGUILayout.Space(10);
            
            DrawGeneratorPanel();
            
            EditorGUILayout.Space(10);
            
            DrawGridEditor();
            
            EditorGUILayout.Space(10);
            
            if (showCellDetail && selectedCell.x >= 0)
            {
                DrawCellDetail();
                EditorGUILayout.Space(10);
            }
            
            DrawActionButtons();
            
            EditorGUILayout.Space(10);
            
            DrawValidationInfo();
            
            serializedObject.ApplyModifiedProperties();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(levelData);
            }
        }
        
        private new void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("VISUAL LEVEL EDITOR", largeLabelStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        private void DrawBasicProperties()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);
            
            levelData.levelIndex = EditorGUILayout.IntField("Level Index", levelData.levelIndex);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Grid Dimensions", EditorStyles.miniBoldLabel);
            
            int oldWidth = levelData.gridWidth;
            int oldHeight = levelData.gridHeight;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Width (X)", GUILayout.Width(70));
            levelData.gridWidth = EditorGUILayout.IntSlider(levelData.gridWidth, 2, 12);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Height (Z)", GUILayout.Width(70));
            levelData.gridHeight = EditorGUILayout.IntSlider(levelData.gridHeight, 2, 12);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField($"Grid: {levelData.gridWidth} × {levelData.gridHeight} = {levelData.GridArea} cells", EditorStyles.centeredGreyMiniLabel);
            
            if (oldWidth != levelData.gridWidth || oldHeight != levelData.gridHeight)
            {
                selectedCell = new Vector2Int(-1, -1);
                showCellDetail = false;
                levelData.CleanupOutOfBoundsPieces();
                levelData.isValidated = false;
                SyncConfigWithLevelData();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawGeneratorPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            showGeneratorPanel = EditorGUILayout.Foldout(showGeneratorPanel, "⚡ Auto Level Generator", true);
            
            if (showGeneratorPanel)
            {
                EditorGUILayout.Space(5);
                
                EditorGUILayout.LabelField("Difficulty", EditorStyles.miniBoldLabel);
                generatorConfig.targetDifficulty = EditorGUILayout.IntSlider(
                    "Target Difficulty", generatorConfig.targetDifficulty, 1, 10);
                EditorGUILayout.LabelField(
                    $"Expected Min Moves: ~{generatorConfig.GetTargetMinMoves()}", 
                    EditorStyles.centeredGreyMiniLabel);
                
                // Apply Difficulty Preset button
                GUI.backgroundColor = new Color(0.7f, 0.85f, 1f);
                if (GUILayout.Button($"Apply Difficulty Preset (Level {generatorConfig.targetDifficulty})", GUILayout.Height(22)))
                {
                    generatorConfig.ApplyDifficultyPreset(generatorConfig.targetDifficulty);
                    // Sync grid size back to level data
                    levelData.gridWidth = generatorConfig.gridWidth;
                    levelData.gridHeight = generatorConfig.gridHeight;
                    EditorUtility.SetDirty(levelData);
                }
                GUI.backgroundColor = Color.white;
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.LabelField("Destinations", EditorStyles.miniBoldLabel);
                generatorConfig.destinationCount = EditorGUILayout.IntSlider(
                    "Destination Count", generatorConfig.destinationCount, 1, 10);
                if (generatorConfig.destinationCount > 1)
                {
                    EditorGUILayout.HelpBox(
                        $"Level will have {generatorConfig.destinationCount} destinations. All must be connected to complete!", 
                        MessageType.Info);
                }
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.LabelField("Allowed Pipe Types", EditorStyles.miniBoldLabel);
                EditorGUILayout.BeginHorizontal();
                generatorConfig.useStraightPipes = GUILayout.Toggle(
                    generatorConfig.useStraightPipes, "Straight", "Button", GUILayout.Height(25));
                generatorConfig.useCornerPipes = GUILayout.Toggle(
                    generatorConfig.useCornerPipes, "Corner", "Button", GUILayout.Height(25));
                generatorConfig.useTJunctionPipes = GUILayout.Toggle(
                    generatorConfig.useTJunctionPipes, "T-Junction", "Button", GUILayout.Height(25));
                generatorConfig.useCrossPipes = GUILayout.Toggle(
                    generatorConfig.useCrossPipes, "Cross", "Button", GUILayout.Height(25));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.LabelField("Options", EditorStyles.miniBoldLabel);
                generatorConfig.useStaticPipes = EditorGUILayout.Toggle(
                    "Use Static Pipes", generatorConfig.useStaticPipes);
                if (generatorConfig.useStaticPipes)
                {
                    generatorConfig.staticPipeRatio = EditorGUILayout.Slider(
                        "Static Ratio", generatorConfig.staticPipeRatio, 0f, 0.5f);
                }
                
                generatorConfig.decoyPipeRatio = EditorGUILayout.Slider(
                    "Decoy Pipes", generatorConfig.decoyPipeRatio, 0f, 0.5f);
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.BeginHorizontal();
                generatorConfig.useRandomSeed = EditorGUILayout.Toggle(
                    "Random Seed", generatorConfig.useRandomSeed, GUILayout.Width(100));
                if (!generatorConfig.useRandomSeed)
                {
                    generatorConfig.seed = EditorGUILayout.IntField(generatorConfig.seed);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(10);
                
                GUI.backgroundColor = new Color(0.3f, 0.9f, 0.5f);
                if (GUILayout.Button("⚡ GENERATE LEVEL", GUILayout.Height(40)))
                {
                    GenerateLevel();
                }
                GUI.backgroundColor = Color.white;
                
                if (generatorConfig.GetAllowedPipeTypes().Count == 0)
                {
                    EditorGUILayout.HelpBox("Select at least one pipe type!", MessageType.Warning);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void GenerateLevel()
        {
            if (generatorConfig.GetAllowedPipeTypes().Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "Select at least one pipe type!", "OK");
                return;
            }
            
            generatorConfig.gridWidth = levelData.gridWidth;
            generatorConfig.gridHeight = levelData.gridHeight;
            
            bool success = proceduralGenerator.GenerateWithRetry(levelData, generatorConfig, 10);
            
            if (success)
            {
                selectedCell = new Vector2Int(-1, -1);
                showCellDetail = false;
                EditorUtility.SetDirty(levelData);
                
                EditorUtility.DisplayDialog("Success", 
                    $"Level generated!\n\n" +
                    $"Pieces: {levelData.GetTotalPieceCount()}\n" +
                    $"Min Moves: {levelData.minimumMoves}\n" +
                    $"Difficulty: {levelData.estimatedDifficulty}/10", "OK");
            }
            else
            {
                string reason = proceduralGenerator.LastFailureReason;
                EditorUtility.DisplayDialog("Failed", 
                    $"Could not generate a valid level after 10 attempts.\n\n" +
                    $"Last failure: {reason}\n\n" +
                    "Try adjusting difficulty or allowed pipe types.", "OK");
            }
        }
        
        private void DrawGridEditor()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label($"Grid Editor ({levelData.gridWidth}×{levelData.gridHeight})", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUILayout.BeginVertical();
            
            for (int z = levelData.gridHeight - 1; z >= 0; z--)
            {
                GUILayout.BeginHorizontal();
                
                // Draw cells from left to right (X increasing)
                for (int x = 0; x < levelData.gridWidth; x++)
                {
                    DrawCell(x, z);
                }
                
                GUILayout.Label($"Z={z}", GUILayout.Width(40));
                
                GUILayout.EndHorizontal();
            }
            
            // Column labels
            GUILayout.BeginHorizontal();
            for (int x = 0; x < levelData.gridWidth; x++)
            {
                GUILayout.Label($"X={x}", GUILayout.Width(CELL_SIZE + CELL_SPACING));
            }
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            DrawLegend();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawCell(int x, int z)
        {
            PieceData piece = levelData.GetPieceAt(x, z);
            bool isSelected = selectedCell.x == x && selectedCell.y == z;
            bool isStatic = piece != null && piece.pieceType.IsStatic();
            bool isDecoy = piece != null && piece.isDecoy;
            
            Color bgColor = emptyColor;
            if (isSelected) 
                bgColor = selectedColor;
            else if (piece != null)
            {
                if (isDecoy)
                {
                    // Red-tinted background for decoy pieces
                    bgColor = new Color(0.85f, 0.35f, 0.35f);
                }
                else if (isStatic)
                {
                    // Darker colors for static pieces
                    bgColor = piece.pieceType switch
                    {
                        PieceType.StaticStraightPipe or PieceType.StaticCornerPipe or 
                        PieceType.StaticTJunctionPipe or PieceType.StaticCrossPipe 
                            => new Color(0.6f, 0.6f, 0.7f),
                        _ => pipeColor
                    };
                }
                else
                {
                    bgColor = piece.pieceType switch
                    {
                        PieceType.Source => sourceColor,
                        PieceType.Destination => destinationColor,
                        _ => pipeColor
                    };
                }
            }
            
            Color originalBg = GUI.backgroundColor;
            GUI.backgroundColor = bgColor;
            
            if (piece == null)
            {
                if (GUILayout.Button("+", GUILayout.Width(CELL_SIZE), GUILayout.Height(CELL_SIZE)))
                {
                    ShowAddPieceMenu(x, z);
                }
            }
            else
            {
                GUILayout.BeginVertical(
                    GUI.skin.box, 
                    GUILayout.Width(CELL_SIZE), 
                    GUILayout.Height(CELL_SIZE),
                    GUILayout.ExpandWidth(false),
                    GUILayout.ExpandHeight(false));
                GUILayout.FlexibleSpace();
                
                // Use compact icon for grid display
                string icon = GetCompactPieceIcon(piece);
                GUILayout.Label(
                    icon, 
                    centeredLabelStyle, 
                    GUILayout.Width(CELL_SIZE - 8f), 
                    GUILayout.ExpandWidth(false));
                
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                
                Rect lastRect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
                {
                    selectedCell = new Vector2Int(x, z);
                    showCellDetail = true;
                    Event.current.Use();
                    Repaint();
                }
            }
            
            GUI.backgroundColor = originalBg;
        }
        
        private void ShowAddPieceMenu(int x, int z)
        {
            GenericMenu menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("Source"), false, () => AddPiece(x, z, PieceType.Source));
            menu.AddItem(new GUIContent("Destination"), false, () => AddPiece(x, z, PieceType.Destination));
            menu.AddSeparator("");
            
            // Rotatable pipes
            menu.AddItem(new GUIContent("Rotatable/Straight Pipe"), false, () => AddPiece(x, z, PieceType.StraightPipe));
            menu.AddItem(new GUIContent("Rotatable/Corner Pipe"), false, () => AddPiece(x, z, PieceType.CornerPipe));
            menu.AddItem(new GUIContent("Rotatable/T-Junction"), false, () => AddPiece(x, z, PieceType.TJunctionPipe));
            menu.AddItem(new GUIContent("Rotatable/Cross"), false, () => AddPiece(x, z, PieceType.CrossPipe));
            
            menu.AddSeparator("");
            
            // Static pipes
            menu.AddItem(new GUIContent("Static/Straight Pipe"), false, () => AddPiece(x, z, PieceType.StaticStraightPipe));
            menu.AddItem(new GUIContent("Static/Corner Pipe"), false, () => AddPiece(x, z, PieceType.StaticCornerPipe));
            menu.AddItem(new GUIContent("Static/T-Junction"), false, () => AddPiece(x, z, PieceType.StaticTJunctionPipe));
            menu.AddItem(new GUIContent("Static/Cross"), false, () => AddPiece(x, z, PieceType.StaticCrossPipe));
            
            menu.ShowAsContext();
        }
        
        private void AddPiece(int x, int z, PieceType type)
        {
            levelData.SetPieceAt(x, z, type, 0);
            selectedCell = new Vector2Int(x, z);
            showCellDetail = true;
            levelData.isValidated = false;
            EditorUtility.SetDirty(levelData);
        }
        
        /// <summary>
        /// Get visual representation of piece with directional arrows
        /// Uses Unicode arrows to show port directions
        /// </summary>
        private string GetPieceIcon(PieceData piece)
        {
            bool isStatic = piece.pieceType.IsStatic();
            
            switch (piece.pieceType)
            {
                // Source: Single output direction (arrow pointing out)
                case PieceType.Source:
                    return piece.rotation switch
                    {
                        0 => "S↑",      // Up
                        90 => "S→",     // Right
                        180 => "S↓",    // Down
                        270 => "S←",    // Left
                        _ => "S↑"
                    };
                
                // Destination: Single input direction (arrow pointing in)
                case PieceType.Destination:
                    return piece.rotation switch
                    {
                        0 => "D↑",      // Expects from Up
                        90 => "D→",     // Expects from Right
                        180 => "D↓",    // Expects from Down
                        270 => "D←",    // Expects from Left
                        _ => "D↑"
                    };
                
                // Straight Pipe: Two opposite directions
                case PieceType.StraightPipe:
                case PieceType.StaticStraightPipe:
                    string straightIcon = (piece.rotation == 0 || piece.rotation == 180) ? "↕" : "↔";
                    return isStatic ? $"🔒{straightIcon}" : straightIcon;
                
                // Corner Pipe: Two adjacent directions (L-shape)
                case PieceType.CornerPipe:
                case PieceType.StaticCornerPipe:
                    string cornerIcon = piece.rotation switch
                    {
                        0 => "↑→",      // Up-Right └
                        90 => "→↓",     // Right-Down ┌
                        180 => "↓←",    // Down-Left ┐
                        270 => "←↑",    // Left-Up ┘
                        _ => "↑→"
                    };
                    return isStatic ? $"🔒{cornerIcon}" : cornerIcon;
                
                // T-Junction: Three directions
                case PieceType.TJunctionPipe:
                case PieceType.StaticTJunctionPipe:
                    // Match PipeConnectionHelper: 0°=Up,Right,Down 90°=Right,Down,Left 180°=Down,Left,Up 270°=Left,Up,Right
                    string tIcon = piece.rotation switch
                    {
                        0 => "↑→↓",     // Up-Right-Down (Left missing)
                        90 => "→↓←",    // Right-Down-Left (Up missing)
                        180 => "↓←↑",   // Down-Left-Up (Right missing)
                        270 => "←↑→",   // Left-Up-Right (Down missing)
                        _ => "↑→↓"
                    };
                    return isStatic ? $"🔒{tIcon}" : tIcon;
                
                // Cross Pipe: All four directions
                case PieceType.CrossPipe: 
                    return "✚";
                case PieceType.StaticCrossPipe: 
                    return "🔒✚";
            }
            return "?";
        }
        
        /// <summary>
        /// Get a compact single-character representation for small cells
        /// </summary>
        private string GetCompactPieceIcon(PieceData piece)
        {
            bool isStatic = piece.pieceType.IsStatic();
            string decoyPrefix = piece.isDecoy ? "D" : "";
            string lockPrefix = isStatic ? "⬛" : decoyPrefix;
            
            switch (piece.pieceType)
            {
                case PieceType.Source:
                    return piece.rotation switch { 0 => "⬆", 90 => "➡", 180 => "⬇", 270 => "⬅", _ => "⬆" };
                case PieceType.Destination:
                    return piece.rotation switch { 0 => "🎯↑", 90 => "🎯→", 180 => "🎯↓", 270 => "🎯←", _ => "🎯↑" };
                case PieceType.StraightPipe:
                case PieceType.StaticStraightPipe:
                    return lockPrefix + ((piece.rotation == 0 || piece.rotation == 180) ? "║" : "═");
                case PieceType.CornerPipe:
                case PieceType.StaticCornerPipe:
                    return lockPrefix + piece.rotation switch { 0 => "╚", 90 => "╔", 180 => "╗", 270 => "╝", _ => "╚" };
                case PieceType.TJunctionPipe:
                case PieceType.StaticTJunctionPipe:
                    return lockPrefix + piece.rotation switch { 0 => "╠", 90 => "╩", 180 => "╣", 270 => "╦", _ => "╠" };
                case PieceType.CrossPipe:
                    return "╬";
                case PieceType.StaticCrossPipe:
                    return "⬛╬";
            }
            return "?";
        }
        
        /// <summary>
        /// Draw a visual representation of port directions
        /// </summary>
        private void DrawPortVisualization(PieceData piece)
        {
            // Use PipeConnectionHelper for consistent port calculation
            var ports = PipeConnectionHelper.GetOpenPorts(piece.pieceType, piece.rotation);
            bool hasUp = ports.Contains(PipeConnectionHelper.DIR_UP);
            bool hasRight = ports.Contains(PipeConnectionHelper.DIR_RIGHT);
            bool hasDown = ports.Contains(PipeConnectionHelper.DIR_DOWN);
            bool hasLeft = ports.Contains(PipeConnectionHelper.DIR_LEFT);
            
            // Draw 3x3 grid visualization
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Port Directions:", EditorStyles.miniLabel);
            
            GUIStyle portStyle = new GUIStyle(EditorStyles.label) 
            { 
                alignment = TextAnchor.MiddleCenter, 
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
            
            float boxSize = 25f;
            
            // Row 1: Up
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("", GUILayout.Width(boxSize));
            GUI.backgroundColor = hasUp ? Color.green : Color.gray;
            GUILayout.Box(hasUp ? "▲" : "·", portStyle, GUILayout.Width(boxSize), GUILayout.Height(boxSize));
            GUI.backgroundColor = Color.white;
            GUILayout.Label("", GUILayout.Width(boxSize));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            // Row 2: Left - Center - Right
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = hasLeft ? Color.green : Color.gray;
            GUILayout.Box(hasLeft ? "◄" : "·", portStyle, GUILayout.Width(boxSize), GUILayout.Height(boxSize));
            GUI.backgroundColor = piece.pieceType.IsStatic() ? new Color(0.5f, 0.5f, 0.6f) : new Color(0.8f, 0.8f, 0.9f);
            GUILayout.Box("●", portStyle, GUILayout.Width(boxSize), GUILayout.Height(boxSize));
            GUI.backgroundColor = hasRight ? Color.green : Color.gray;
            GUILayout.Box(hasRight ? "►" : "·", portStyle, GUILayout.Width(boxSize), GUILayout.Height(boxSize));
            GUI.backgroundColor = Color.white;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            // Row 3: Down
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("", GUILayout.Width(boxSize));
            GUI.backgroundColor = hasDown ? Color.green : Color.gray;
            GUILayout.Box(hasDown ? "▼" : "·", portStyle, GUILayout.Width(boxSize), GUILayout.Height(boxSize));
            GUI.backgroundColor = Color.white;
            GUILayout.Label("", GUILayout.Width(boxSize));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawCellDetail()
        {
            PieceData piece = levelData.GetPieceAt(selectedCell.x, selectedCell.y);
            if (piece == null) return;
            
            bool isStatic = piece.pieceType.IsStatic();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Selected Cell Detail", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            // Type and position info
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField($"Type: {piece.pieceType}");
            EditorGUILayout.LabelField($"Position: ({piece.x}, {piece.z})");
            EditorGUILayout.LabelField($"Rotation: {piece.rotation}°");
            if (isStatic)
            {
                EditorGUILayout.HelpBox("🔒 STATIC", MessageType.None);
            }
            EditorGUILayout.EndVertical();
            
            // Port visualization
            DrawPortVisualization(piece);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            // All pieces can be rotated in editor (including Source/Destination)
            // Only static pipes have a different button style
            bool isSourceOrDest = piece.pieceType == PieceType.Source || piece.pieceType == PieceType.Destination;
            
            if (isStatic)
            {
                // Static pipes - gray button
                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.8f);
                if (GUILayout.Button("⟳ Set Initial Rotation", GUILayout.Height(35)))
                {
                    levelData.RotatePieceAt(selectedCell.x, selectedCell.y);
                    levelData.isValidated = false;
                    EditorUtility.SetDirty(levelData);
                }
                GUI.backgroundColor = Color.white;
            }
            else if (isSourceOrDest)
            {
                // Source/Destination - special color (orange/blue)
                GUI.backgroundColor = piece.pieceType == PieceType.Source ? 
                    new Color(0.3f, 0.7f, 1f) : new Color(1f, 0.7f, 0.3f);
                if (GUILayout.Button("⟳ Rotate Direction", GUILayout.Height(35)))
                {
                    levelData.RotatePieceAt(selectedCell.x, selectedCell.y);
                    levelData.isValidated = false;
                    EditorUtility.SetDirty(levelData);
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                // Normal rotatable pipes - cyan button
                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button("⟳ Rotate 90°", GUILayout.Height(35)))
                {
                    levelData.RotatePieceAt(selectedCell.x, selectedCell.y);
                    levelData.isValidated = false;
                    EditorUtility.SetDirty(levelData);
                }
                GUI.backgroundColor = Color.white;
            }
            
            GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);
            if (GUILayout.Button("Delete", GUILayout.Height(35), GUILayout.Width(100)))
            {
                levelData.RemovePieceAt(selectedCell.x, selectedCell.y);
                selectedCell = new Vector2Int(-1, -1);
                showCellDetail = false;
                levelData.isValidated = false;
                EditorUtility.SetDirty(levelData);
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawLegend()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Legend:", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);
            
            // Pipe shapes
            EditorGUILayout.LabelField("Pipe Shapes:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  ║ ═  Straight (vertical/horizontal)", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  ╚ ╔ ╗ ╝  Corner (L-shape rotations)", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  ╠ ╩ ╣ ╦  T-Junction (rotations)", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  ╬  Cross (all directions)", EditorStyles.miniLabel);
            
            EditorGUILayout.Space(3);
            
            // Special pieces
            EditorGUILayout.LabelField("Special:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  ⬆ ➡ ⬇ ⬅  Source (output direction)", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  🎯↑→↓←  Destination (input direction)", EditorStyles.miniLabel);
            
            EditorGUILayout.Space(3);
            
            // Static indicator
            EditorGUILayout.LabelField("Modifiers:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  ⬛  Static/Locked (cannot rotate)", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  \u25A0 D  Decoy (red bg, not on solution path)", EditorStyles.miniLabel);
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawActionButtons()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = new Color(0.3f, 0.9f, 0.3f);
            if (GUILayout.Button("Validate Level", GUILayout.Height(40)))
            {
                ValidateLevel();
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = new Color(1f, 0.9f, 0.3f);
            if (GUILayout.Button("Clear All", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Clear All Pieces", 
                    "Are you sure you want to clear all pieces from the grid?", 
                    "Yes, Clear", "Cancel"))
                {
                    levelData.ClearAllPieces();
                    selectedCell = new Vector2Int(-1, -1);
                    showCellDetail = false;
                    EditorUtility.SetDirty(levelData);
                }
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawValidationInfo()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Level Information", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField($"Grid: {levelData.gridWidth} × {levelData.gridHeight} ({levelData.GridArea} cells)");
            EditorGUILayout.LabelField($"Total Pieces: {levelData.GetTotalPieceCount()}");
            
            var source = levelData.GetSource();
            var destinations = levelData.GetDestinations();
            
            if (source != null)
                EditorGUILayout.LabelField($"Source: ({source.x}, {source.z}) Rotation: {source.rotation}°");
            else
                EditorGUILayout.LabelField("Source: NOT SET", EditorStyles.boldLabel);
            
            if (destinations.Count > 0)
            {
                EditorGUILayout.LabelField($"Destinations: {destinations.Count}");
                foreach (var dest in destinations)
                {
                    EditorGUILayout.LabelField($"  🎯 ({dest.x}, {dest.z}) Rotation: {dest.rotation}°", EditorStyles.miniLabel);
                }
            }
            else
            {
                EditorGUILayout.LabelField("Destination: NOT SET", EditorStyles.boldLabel);
            }
            
            EditorGUILayout.Space(5);
            
            if (levelData.isValidated)
            {
                MessageType msgType = levelData.validationMessage.Contains("✅") ? 
                    MessageType.Info : MessageType.Warning;
                
                EditorGUILayout.HelpBox(levelData.validationMessage, msgType);
                
                if (levelData.minimumMoves >= 0)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.BeginHorizontal();
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("Min Moves", EditorStyles.centeredGreyMiniLabel);
                    EditorGUILayout.LabelField(levelData.minimumMoves.ToString(), 
                        new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 18 });
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("Difficulty", EditorStyles.centeredGreyMiniLabel);
                    EditorGUILayout.LabelField($"{levelData.estimatedDifficulty}/10", 
                        new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 18 });
                    EditorGUILayout.EndVertical();
                    
                    if (destinations.Count > 1)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField("Destinations", EditorStyles.centeredGreyMiniLabel);
                        EditorGUILayout.LabelField($"{destinations.Count}", 
                            new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 18 });
                        EditorGUILayout.EndVertical();
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Level not validated. Click 'Validate Level' to check.", MessageType.None);
            }
            
            EditorGUILayout.EndVertical();
            
            DrawRawDataPanel();
        }
        
        private void DrawRawDataPanel()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            showRawData = EditorGUILayout.Foldout(showRawData, "📋 Raw Level Data (JSON)", true);
            
            if (showRawData)
            {
                EditorGUILayout.Space(5);
                
                string jsonData = GenerateJsonData();
                
                rawDataScroll = EditorGUILayout.BeginScrollView(rawDataScroll, GUILayout.MaxHeight(300));
                
                GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea)
                {
                    wordWrap = true,
                    fontSize = 11,
                    richText = false
                };
                
                EditorGUILayout.TextArea(jsonData, textAreaStyle, GUILayout.ExpandHeight(true));
                
                EditorGUILayout.EndScrollView();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy to Clipboard", GUILayout.Height(25)))
                {
                    GUIUtility.systemCopyBuffer = jsonData;
                    EditorUtility.DisplayDialog("Copied", "JSON data copied to clipboard!", "OK");
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private string GenerateJsonData()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"levelIndex\": {levelData.levelIndex},");
            sb.AppendLine($"  \"gridWidth\": {levelData.gridWidth},");
            sb.AppendLine($"  \"gridHeight\": {levelData.gridHeight},");
            sb.AppendLine($"  \"isValidated\": {levelData.isValidated.ToString().ToLower()},");
            sb.AppendLine($"  \"minimumMoves\": {levelData.minimumMoves},");
            sb.AppendLine($"  \"estimatedDifficulty\": {levelData.estimatedDifficulty},");
            sb.AppendLine("  \"pieces\": [");
            
            var sortedPieces = levelData.pieces.OrderBy(p => p.z).ThenBy(p => p.x).ToList();
            
            for (int i = 0; i < sortedPieces.Count; i++)
            {
                var piece = sortedPieces[i];
                string ports = GetPortsString(piece);
                string comma = (i < sortedPieces.Count - 1) ? "," : "";
                
                sb.AppendLine($"    {{");
                sb.AppendLine($"      \"position\": \"({piece.x},{piece.z})\",");
                sb.AppendLine($"      \"type\": \"{piece.pieceType}\",");
                sb.AppendLine($"      \"rotation\": {piece.rotation},");
                sb.AppendLine($"      \"ports\": \"{ports}\",");
                sb.AppendLine($"      \"isStatic\": {piece.pieceType.IsStatic().ToString().ToLower()}");
                sb.AppendLine($"    }}{comma}");
            }
            
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            
            return sb.ToString();
        }
        
        private string GetPortsString(PieceData piece)
        {
            // Use PipeConnectionHelper for consistent port calculation
            var portList = PipeConnectionHelper.GetOpenPorts(piece.pieceType, piece.rotation);
            var ports = new List<string>();
            
            foreach (int dir in portList)
            {
                switch (dir)
                {
                    case PipeConnectionHelper.DIR_UP: ports.Add("Up"); break;
                    case PipeConnectionHelper.DIR_RIGHT: ports.Add("Right"); break;
                    case PipeConnectionHelper.DIR_DOWN: ports.Add("Down"); break;
                    case PipeConnectionHelper.DIR_LEFT: ports.Add("Left"); break;
                }
            }
            
            return string.Join(",", ports);
        }
        
        private async void ValidateLevel()
        {
            if (isValidationRunning)
                return;

            isValidationRunning = true;
            EditorUtility.DisplayProgressBar("Validating Level", "Checking solvability...", 0.5f);

            try
            {
                LevelValidator validator = new LevelValidator(levelData);
                // Run async validation
                bool isValid = await validator.ValidateAsync((p) => {
                    EditorApplication.delayCall += () => {
                        if (!isValidationRunning) return;
                        EditorUtility.DisplayProgressBar("Validating Level", $"Checking solvability... {p:P0}", p);
                    };
                });

                levelData.isValidated = true;
                levelData.validationMessage = validator.GetValidationMessage();
                levelData.estimatedDifficulty = validator.GetEstimatedDifficulty();
                levelData.minimumMoves = validator.GetMinimumMoves();

                EditorUtility.SetDirty(levelData);

                if (isValid)
                {
                    string details = $"Level is valid and solvable!\n\n" +
                        $"Minimum Moves: {levelData.minimumMoves}\n" +
                        $"Difficulty: {levelData.estimatedDifficulty}/10";

                    EditorUtility.DisplayDialog("Validation Success", details, "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Validation Failed",
                        validator.GetValidationMessage(), "OK");
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Validation error: {ex}");
                EditorUtility.DisplayDialog("Error", $"Validation error: {ex.Message}", "OK");
            }
            finally
            {
                isValidationRunning = false;
                EditorUtility.ClearProgressBar();
            }
        }
    }
}

