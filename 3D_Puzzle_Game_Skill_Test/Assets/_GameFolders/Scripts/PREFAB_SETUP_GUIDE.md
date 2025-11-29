# 🎮 Puzzle Game Prefab Setup Guide

## Overview
This document explains how to set up prefabs for the new refactored architecture.

---

## 📦 PREFAB STRUCTURE

### 1️⃣ Tile Prefabs (tileAPrefab, tileBPrefab)
**Location:** Theme ScriptableObject → tileAPrefab / tileBPrefab

```
TilePrefab (GameObject)
├── MeshFilter (tile mesh)
├── MeshRenderer (tile material)
└── NO SCRIPTS - Visual only!
```

**Requirements:**
- ❌ No Collider
- ❌ No Rigidbody
- ❌ No Scripts
- ✅ Only visual mesh

---

### 2️⃣ Pipe Prefabs (StraightPipe, CornerPipe, TJunction, Cross)
**Location:** Theme ScriptableObject → straightPipePrefab, cornerPipePrefab, etc.

```
PipePrefab (GameObject)
├── [Visual] (Child GameObject - for rotation)
│   ├── MeshFilter (pipe mesh)
│   └── MeshRenderer (pipe material)
├── PipeController (Script) ← REQUIRED
├── ClickableObject (Script) ← OPTIONAL (added at runtime if missing)
└── BoxCollider ← OPTIONAL (added at runtime if missing)
```

**PipeController Settings:**
- `pieceType`: Set to correct type (StraightPipe, CornerPipe, etc.)
- `visualTransform`: Assign the [Visual] child transform
- `rotationDuration`: 0.25 (default)
- `rotationEase`: OutBack (default)

**Collider Settings (if pre-added):**
- Type: BoxCollider
- Size: (0.6, 0.5, 0.6)
- Center: (0, 0.25, 0)
- Is Trigger: ❌ FALSE (solid for raycast)

---

### 3️⃣ Source Prefab
**Location:** Theme ScriptableObject → sourcePrefab

```
SourcePrefab (GameObject)
├── [Visual] (Child GameObject)
│   ├── MeshFilter
│   └── MeshRenderer
├── SourceController (Script) ← REQUIRED
└── BoxCollider ← OPTIONAL
```

**Notes:**
- Source is always connected (starting point)
- Source does not rotate
- Connects in all 4 directions

---

### 4️⃣ Destination Prefab
**Location:** Theme ScriptableObject → destinationPrefab

```
DestinationPrefab (GameObject)
├── [Visual] (Child GameObject)
│   ├── MeshFilter
│   └── MeshRenderer
├── DestinationController (Script) ← REQUIRED
└── BoxCollider ← OPTIONAL
```

**Notes:**
- Level completes when Destination becomes connected
- Does not rotate
- Connects in all 4 directions

---

## 🔧 SETUP STEPS

### Step 1: Update Theme ScriptableObject
1. Open your ThemeDataSO asset
2. Assign prefabs:
   - `tileAPrefab` → Light tile prefab
   - `tileBPrefab` → Dark tile prefab
   - `sourcePrefab` → Source prefab
   - `destinationPrefab` → Destination prefab
   - `straightPipePrefab` → Straight pipe prefab
   - `cornerPipePrefab` → Corner pipe prefab
   - `tJunctionPipePrefab` → T-junction pipe prefab
   - `crossPipePrefab` → Cross pipe prefab

### Step 2: Create/Update Pipe Prefabs
For each pipe prefab:

1. **Create hierarchy:**
   ```
   StraightPipe
   └── Visual (child with mesh)
   ```

2. **Add PipeController component:**
   - Component → Add → BufoGames.Pieces → PipeController
   - Set `Piece Type` to match (StraightPipe, CornerPipe, etc.)
   - Drag `Visual` child to `Visual Transform` field

3. **Optionally add BoxCollider:**
   - Size: 0.6, 0.5, 0.6
   - Is Trigger: FALSE

### Step 3: Create Source Prefab
1. Create hierarchy with Visual child
2. Add SourceController component
3. Optionally add BoxCollider

### Step 4: Create Destination Prefab
1. Create hierarchy with Visual child
2. Add DestinationController component
3. Optionally add BoxCollider

### Step 5: Remove Old Scripts from Prefabs
Remove these deprecated components if present:
- ❌ GroundObject
- ❌ SpawnedObjectController
- ❌ Any script from _Deprecated folder

---

## 📐 PIPE CONNECTION DIRECTIONS

Each pipe type has specific connection ports:

| Pipe Type | Base Ports (0° rotation) | Visual |
|-----------|-------------------------|--------|
| StraightPipe | Up, Down | │ |
| CornerPipe | Up, Right | └ |
| TJunctionPipe | Up, Down, Right | ├ |
| CrossPipe | Up, Down, Left, Right | ┼ |
| Source | All directions | ● |
| Destination | All directions | ■ |

**Rotation affects ports:**
- 90° clockwise: Up→Right, Right→Down, Down→Left, Left→Up
- Example: CornerPipe at 90° has ports: Right, Down

---

## 🎯 RUNTIME FLOW

```
LevelManager.LoadLevel()
    ↓
LevelGenerator.GenerateLevel()
    ├── GenerateTiles() → Visual only, no scripts
    └── SpawnPieces() → Pieces with controllers
        ├── For Source: Add SourceController
        ├── For Destination: Add DestinationController
        └── For Pipes: Add PipeController + ClickableObject
    ↓
LevelController.Initialize()
    ├── Build GridManager (position lookup)
    ├── Create ConnectionValidator
    └── Subscribe to pipe rotation events
    ↓
User clicks pipe
    ↓
InputManager.Raycast → ClickableObject.OnClick
    ↓
PipeController.Rotate() → DOTween animation
    ↓
OnRotationCompleted event
    ↓
LevelController.CheckLevelCompletion()
    ↓
ConnectionValidator.ValidateAllConnections()
    ├── BFS from Source
    ├── Check port alignment (mathematical)
    └── Return true if all connected
    ↓
If complete → Trigger win events
```

---

## ✅ CHECKLIST

- [ ] ThemeDataSO has all prefabs assigned
- [ ] Pipe prefabs have PipeController with correct PieceType
- [ ] Pipe prefabs have Visual child for rotation
- [ ] Source prefab has SourceController
- [ ] Destination prefab has DestinationController
- [ ] No deprecated scripts on any prefab
- [ ] LevelManager has ThemeDataSO assigned
- [ ] LevelManager has LevelDatabaseSO assigned
- [ ] Scene has InputManager (or will be created at runtime)

---

## 🐛 TROUBLESHOOTING

**Issue: Pipes don't rotate**
- Check ClickableObject component exists
- Check BoxCollider exists (for raycast)
- Check PipeController.visualTransform is assigned

**Issue: Connections not working**
- Check pieces have correct PieceType set
- Check LevelDataSO has correct rotations
- Enable debug logs in ConnectionValidator

**Issue: Level never completes**
- Check Source and Destination are spawning
- Check all pieces have PieceBase derived component
- Check GridManager.BuildMap() is called

---

## 📁 FILE LOCATIONS

```
Assets/_GameFolders/Scripts/
├── _Concretes/
│   ├── Grid/
│   │   ├── Direction.cs
│   │   ├── DirectionHelper.cs
│   │   ├── GridManager.cs
│   │   └── PipePortData.cs
│   ├── Pieces/
│   │   ├── PieceBase.cs
│   │   ├── PipeController.cs
│   │   ├── SourceController.cs
│   │   └── DestinationController.cs
│   ├── Validators/
│   │   └── ConnectionValidator.cs
│   ├── Generation/
│   │   └── LevelGenerator.cs
│   ├── Controllers/
│   │   └── LevelController.cs
│   └── Managers/
│       └── LevelManager.cs
└── _Deprecated/ (old files - can be deleted)
```
