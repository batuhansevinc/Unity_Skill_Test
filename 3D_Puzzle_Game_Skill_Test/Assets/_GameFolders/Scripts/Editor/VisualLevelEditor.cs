using UnityEngine;
using UnityEditor;
using BufoGames.Data;

namespace BufoGames.Editor
{
    [CustomEditor(typeof(LevelDataSO))]
    public class VisualLevelEditor : UnityEditor.Editor
    {
        private LevelDataSO levelData;
        private Vector2Int selectedCell = new Vector2Int(-1, -1);
        private bool showCellDetail = false;
        
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
        }
        
        private void InitStyles()
        {
            if (centeredLabelStyle == null)
            {
                centeredLabelStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 20,
                    fontStyle = FontStyle.Bold
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
            
            int oldGridSize = levelData.gridSize;
            levelData.gridSize = EditorGUILayout.IntSlider("Grid Size", levelData.gridSize, 2, 10);
            
            if (oldGridSize != levelData.gridSize)
            {
                selectedCell = new Vector2Int(-1, -1);
                showCellDetail = false;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawGridEditor()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Grid Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUILayout.BeginVertical();
            
            for (int z = levelData.gridSize - 1; z >= 0; z--)
            {
                GUILayout.BeginHorizontal();
                
                for (int x = 0; x < levelData.gridSize; x++)
                {
                    DrawCell(x, z);
                }
                
                GUILayout.Label($"Row {z}", GUILayout.Width(50));
                
                GUILayout.EndHorizontal();
            }
            
            GUILayout.BeginHorizontal();
            for (int x = 0; x < levelData.gridSize; x++)
            {
                GUILayout.Label($"Col {x}", GUILayout.Width(CELL_SIZE + CELL_SPACING));
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
            
            Color bgColor = emptyColor;
            if (isSelected) 
                bgColor = selectedColor;
            else if (piece != null)
            {
                switch (piece.pieceType)
                {
                    case PieceType.Source: bgColor = sourceColor; break;
                    case PieceType.Destination: bgColor = destinationColor; break;
                    default: bgColor = pipeColor; break;
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
                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(CELL_SIZE), GUILayout.Height(CELL_SIZE));
                GUILayout.FlexibleSpace();
                
                string icon = GetPieceIcon(piece);
                GUILayout.Label(icon, centeredLabelStyle);
                
                if (piece.pieceType != PieceType.Source && piece.pieceType != PieceType.Destination)
                {
                    GUILayout.Label($"{piece.rotation}°", rotationLabelStyle);
                }
                
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
            menu.AddItem(new GUIContent("Straight Pipe"), false, () => AddPiece(x, z, PieceType.StraightPipe));
            menu.AddItem(new GUIContent("Corner Pipe"), false, () => AddPiece(x, z, PieceType.CornerPipe));
            menu.AddItem(new GUIContent("T-Junction"), false, () => AddPiece(x, z, PieceType.TJunctionPipe));
            menu.AddItem(new GUIContent("Cross"), false, () => AddPiece(x, z, PieceType.CrossPipe));
            
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
        
        private string GetPieceIcon(PieceData piece)
        {
            switch (piece.pieceType)
            {
                case PieceType.Source: return "S";
                case PieceType.Destination: return "D";
                case PieceType.StraightPipe:
                    return (piece.rotation == 0 || piece.rotation == 180) ? "|" : "-";
                case PieceType.CornerPipe:
                    switch (piece.rotation)
                    {
                        case 0: return "L";
                        case 90: return "r";
                        case 180: return "7";
                        case 270: return "J";
                    }
                    break;
                case PieceType.TJunctionPipe:
                    return "T";
                case PieceType.CrossPipe: return "+";
            }
            return "?";
        }
        
        private void DrawCellDetail()
        {
            PieceData piece = levelData.GetPieceAt(selectedCell.x, selectedCell.y);
            if (piece == null) return;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Selected Cell Detail", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField($"Type: {piece.pieceType}", EditorStyles.largeLabel);
            EditorGUILayout.LabelField($"Position: ({piece.x}, {piece.z})");
            EditorGUILayout.LabelField($"Current Rotation: {piece.rotation}°");
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            if (piece.pieceType != PieceType.Source && piece.pieceType != PieceType.Destination)
            {
                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button($"Rotate: {piece.rotation}°", GUILayout.Height(35)))
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
            EditorGUILayout.LabelField("Legend:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("S=Source  D=Destination  +=Empty", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("|=Straight  L=Corner  T=T-Junction  +=Cross", EditorStyles.miniLabel);
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
            
            EditorGUILayout.LabelField($"Total Pieces: {levelData.GetTotalPieceCount()}");
            
            var source = levelData.GetSource();
            var dest = levelData.GetDestination();
            
            if (source != null)
                EditorGUILayout.LabelField($"Source: ({source.x}, {source.z})");
            else
                EditorGUILayout.LabelField("Source: NOT SET", EditorStyles.boldLabel);
            
            if (dest != null)
                EditorGUILayout.LabelField($"Destination: ({dest.x}, {dest.z})");
            else
                EditorGUILayout.LabelField("Destination: NOT SET", EditorStyles.boldLabel);
            
            EditorGUILayout.Space(5);
            
            if (levelData.isValidated)
            {
                MessageType msgType = levelData.validationMessage.Contains("valid") ? 
                    MessageType.Info : MessageType.Warning;
                
                EditorGUILayout.HelpBox(levelData.validationMessage, msgType);
            }
            else
            {
                EditorGUILayout.HelpBox("Level not validated. Click 'Validate Level' to check.", MessageType.None);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void ValidateLevel()
        {
            LevelValidator validator = new LevelValidator(levelData);
            bool isValid = validator.Validate();
            
            levelData.isValidated = true;
            levelData.validationMessage = validator.GetValidationMessage();
            levelData.estimatedDifficulty = validator.GetEstimatedDifficulty();
            
            EditorUtility.SetDirty(levelData);
            
            if (isValid)
            {
                EditorUtility.DisplayDialog("Validation Success", 
                    "Level is valid and solvable!\n\n" + validator.GetValidationMessage(), "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Failed", 
                    validator.GetValidationMessage(), "OK");
            }
        }
    }
}

