# Scripts Architecture Map

## Scope

This document maps all C# scripts under:

`Assets/_GameFolders/Scripts`

It is a structural and flow guide:
- where each script lives
- what each script is responsible for
- how runtime and editor flows work end-to-end

No risk analysis or refactor proposal is included.

---

## Quick Snapshot

- Total script count: `75`
- Main modules:
  - `_Concretes`: gameplay/runtime domain code
  - `_Abstracts`: shared contracts/base behaviors
  - `BatuhanSevinc`: infrastructure/utilities (singleton, event, save/load)
  - `Editor`: custom inspector tooling and visual level authoring

Directory distribution:
- `Scripts/_Concretes`: 40 files
- `Scripts/_Abstracts`: 7 files
- `Scripts/BatuhanSevinc`: 24 files
- `Scripts/Editor`: 4 files

---

## Core Runtime Flow

### 1) Boot and Level Load

1. `LevelManager` initializes singleton, ensures `InputManager` exists, creates a `LevelGenerator` object.
2. `LevelManager` reads current level index (test mode or `PlayerPrefs`).
3. `LevelManager.LoadLevel(levelIndex)` pulls `LevelDataSO` from `LevelDatabaseSO`.
4. `LevelGenerator.GenerateLevel(levelData)` creates:
   - level root object
   - grid tiles
   - all piece instances (source/destination/pipes)
   - `LevelController` with references to spawned pieces
5. `LevelManager` injects runtime game events into `LevelController` using `SetGameEvents(...)`.

### 2) Input and Piece Rotation

1. `InputManager` listens for mouse click.
2. Raycast hit is resolved to `PipeController`, `SourceController`, or `DestinationController`.
3. The selected piece runs `Rotate()` (DOTween animation).
4. On animation completion, piece emits `OnRotationCompleted`.

### 3) Connection Check and Win

1. `LevelController` subscribes to all piece rotation completed events.
2. Every rotation completion triggers `LevelController.CheckLevelCompletion()`.
3. `ConnectionValidator.ValidateAllConnections(...)` runs BFS from source over valid reciprocal ports.
4. If all destinations are connected:
   - level marked complete
   - completion coroutine starts
   - game events are fired in sequence:
     - start/end game animations event
     - fireworks event
     - level completed event

### 4) UI and Progress

- UI button scripts fire `GameEvent` assets through `BaseButtonWithGameEvents`.
- Settings toggles (`SoundSettingsButton`, `VibrationSettingsButton`) persist values via `SaveLoadManager` -> `PlayerPrefsDataSaveLoadDal`.
- Scene-level progression is handled by `GameManager` (scene build index based).
- Data-level progression inside level database is handled by `LevelManager`.

---

## Editor / Authoring Flow

### Visual Level Authoring

`VisualLevelEditor` (custom inspector for `LevelDataSO`) provides:
- manual grid editing (add/remove/rotate pieces)
- procedural generation panel (`LevelGeneratorConfig` + `ProceduralLevelGenerator`)
- async validation (`LevelValidator.ValidateAsync`)
- JSON raw data preview and clipboard copy

### Validation Helpers

- `LevelManagerValidator`: checks setup in inspector (database, theme, camera targets, events), can auto-create camera targets.
- `LevelControllerValidator`: validates event fields and clarifies runtime injection behavior.
- `BatchLevelPrefabFixer`: deprecated info window.

---

## Data and Generation Pipeline

### Data Assets

- `LevelDatabaseSO`: list and retrieval of all levels.
- `LevelDataSO`: one level definition:
  - grid width/height
  - piece list (`PieceData`)
  - validation metadata (`isValidated`, `minimumMoves`, `estimatedDifficulty`, message)
- `ThemeDataSO`: prefab/material mapping for tiles and piece types.

### Procedural Generation (`ProceduralLevelGenerator`)

High-level algorithm:
1. Place source on edge cell.
2. Place one or more destinations.
3. Build paths using A* (main path + branches).
4. Convert required connection graph to concrete piece types/rotations.
5. Validate generated solved topology.
6. Optionally add decoy pipes.
7. Optionally convert subset to static pipe variants.
8. Scramble rotatable pieces for gameplay start.
9. Compute and write generated level metadata.

### Validation (`LevelValidator`)

High-level algorithm:
1. Basic requirement checks (source, destinations, pipe presence, adjacency).
2. Static/source/destination boundary and connectivity consistency checks.
3. Constraint propagation over rotatable pieces.
4. Guided backtracking search over rotations.
5. Solved-state reachability check from source to all destinations.
6. Minimum move and difficulty estimation output.

---

## Grid and Connectivity Model

- `PieceType` defines all piece families:
  - special: source, destination
  - rotatable pipes
  - static pipes
- `PipeConnectionHelper` is the central runtime/editor connection truth:
  - direction constants and offsets
  - port masks by type and rotation
  - opposite direction mapping
  - helper methods for rotation/port validity
- `PipePortData` and `DirectionHelper` provide additional direction/port utilities.
- `GridManager` holds O(1) `(x,z) -> PieceBase` lookup for runtime checks.

---

## Full Script Map (All 75 Files)

### `_Abstracts` (7)

- `Assets/_GameFolders/Scripts/_Abstracts/BaseSettingsSwitchButton.cs`
  - Base class for toggle-based settings buttons with save/load helpers.
- `Assets/_GameFolders/Scripts/_Abstracts/Controllers/ILevelController.cs`
  - Interface for level controller contract.
- `Assets/_GameFolders/Scripts/_Abstracts/Initializable/IInitializable.cs`
  - Generic initialize/object-type/pipe-type contract.
- `Assets/_GameFolders/Scripts/_Abstracts/Mover/Animate/AnimationSetter.cs`
  - Bounce animation wrapper using DOTween.
- `Assets/_GameFolders/Scripts/_Abstracts/Mover/Animate/IAnimatable.cs`
  - Animation contract interface.
- `Assets/_GameFolders/Scripts/_Abstracts/Mover/Rotate/IRotatable.cs`
  - Rotation contract interface.
- `Assets/_GameFolders/Scripts/_Abstracts/Mover/Rotate/Rotator.cs`
  - Rotation tween implementation.

### `_Concretes/Constants` (1)

- `Assets/_GameFolders/Scripts/_Concretes/Constants/LevelConstants.cs`
  - Shared constants for intervals, timings, tags, and layer names.

### `_Concretes/Controllers` (4)

- `Assets/_GameFolders/Scripts/_Concretes/Controllers/ClickableObject.cs`
  - Click event relay with cooldown.
- `Assets/_GameFolders/Scripts/_Concretes/Controllers/LevelController.cs`
  - Runtime level orchestration and completion checks.
- `Assets/_GameFolders/Scripts/_Concretes/Controllers/ParentRemover.cs`
  - Transform/parent normalization helper.
- `Assets/_GameFolders/Scripts/_Concretes/Controllers/SpawnedObjectAnimationController.cs`
  - Animator trigger control for spawned objects.

### `_Concretes/Controllers/UI` (8)

- `Assets/_GameFolders/Scripts/_Concretes/Controllers/UI/CanvasGroupController.cs`
  - CanvasGroup visibility and interaction toggle.
- `Assets/_GameFolders/Scripts/_Concretes/Controllers/UI/CloseSettingsButton.cs`
  - Close settings via game event.
- `Assets/_GameFolders/Scripts/_Concretes/Controllers/UI/ReTryButton.cs`
  - Retry button with sound + event invoke.
- `Assets/_GameFolders/Scripts/_Concretes/Controllers/UI/SettingsButton.cs`
  - Open settings via event invoke.
- `Assets/_GameFolders/Scripts/_Concretes/Controllers/UI/SoundSettingsButton.cs`
  - Sound toggle with AudioMixer and persistent save.
- `Assets/_GameFolders/Scripts/_Concretes/Controllers/UI/SplashScreenController.cs`
  - Loading slider animation then `GameManager.LoadNextLevel()`.
- `Assets/_GameFolders/Scripts/_Concretes/Controllers/UI/UIController.cs`
  - UI text updates and particle/confetti control.
- `Assets/_GameFolders/Scripts/_Concretes/Controllers/UI/VibrationSettingsButton.cs`
  - Vibration toggle with persistent save.

### `_Concretes/Data` (7)

- `Assets/_GameFolders/Scripts/_Concretes/Data/LevelDatabaseSO.cs`
  - Level list repository ScriptableObject.
- `Assets/_GameFolders/Scripts/_Concretes/Data/LevelDataSO.cs`
  - Single level definition and piece list operations.
- `Assets/_GameFolders/Scripts/_Concretes/Data/LevelGridData.cs`
  - Grid geometry and camera target coordinate calculations.
- `Assets/_GameFolders/Scripts/_Concretes/Data/LevelValidator.cs`
  - Solvability/complexity validator with async option.
- `Assets/_GameFolders/Scripts/_Concretes/Data/PieceType.cs`
  - Piece enum + type extension helpers.
- `Assets/_GameFolders/Scripts/_Concretes/Data/PipeConnectionHelper.cs`
  - Central port/direction/connection utility model.
- `Assets/_GameFolders/Scripts/_Concretes/Data/ThemeDataSO.cs`
  - Theme prefab/material references and type-to-prefab mapping.

### `_Concretes/Generation` (3)

- `Assets/_GameFolders/Scripts/_Concretes/Generation/LevelGenerator.cs`
  - Spawns tiles and pieces from `LevelDataSO`, wires `LevelController`.
- `Assets/_GameFolders/Scripts/_Concretes/Generation/LevelGeneratorConfig.cs`
  - Procedural generation configuration payload.
- `Assets/_GameFolders/Scripts/_Concretes/Generation/ProceduralLevelGenerator.cs`
  - Procedural puzzle topology generation and scrambling.

### `_Concretes/Grid` (4)

- `Assets/_GameFolders/Scripts/_Concretes/Grid/Direction.cs`
  - Cardinal direction enum.
- `Assets/_GameFolders/Scripts/_Concretes/Grid/DirectionHelper.cs`
  - Direction arithmetic and neighbor helpers.
- `Assets/_GameFolders/Scripts/_Concretes/Grid/GridManager.cs`
  - Piece lookup map and neighbor retrieval.
- `Assets/_GameFolders/Scripts/_Concretes/Grid/PipePortData.cs`
  - Port data tables and fast port lookup methods.

### `_Concretes/Managers` (3)

- `Assets/_GameFolders/Scripts/_Concretes/Managers/GameManager.cs`
  - Scene-level progression, restart, pause state.
- `Assets/_GameFolders/Scripts/_Concretes/Managers/InputManager.cs`
  - Click/raycast input and piece rotation trigger.
- `Assets/_GameFolders/Scripts/_Concretes/Managers/LevelManager.cs`
  - Runtime level lifecycle over `LevelDatabaseSO`.

### `_Concretes/Pieces` (4)

- `Assets/_GameFolders/Scripts/_Concretes/Pieces/DestinationController.cs`
  - Destination behavior, rotation, and connection events.
- `Assets/_GameFolders/Scripts/_Concretes/Pieces/PieceBase.cs`
  - Shared piece API for position, ports, connectivity state.
- `Assets/_GameFolders/Scripts/_Concretes/Pieces/PipeController.cs`
  - Pipe behavior including static/non-static rotate policy.
- `Assets/_GameFolders/Scripts/_Concretes/Pieces/SourceController.cs`
  - Source behavior and rotation event.

### `_Concretes/ScriptableObjects/Levels/References` (1)

- `Assets/_GameFolders/Scripts/_Concretes/ScriptableObjects/Levels/References/LevelReferencesSO.cs`
  - Stores camera target transform references.

### `_Concretes/ScriptableObjects/Puzzle` (3)

- `Assets/_GameFolders/Scripts/_Concretes/ScriptableObjects/Puzzle/GroundObjectSO.cs`
  - Ground object and pipe category references.
- `Assets/_GameFolders/Scripts/_Concretes/ScriptableObjects/Puzzle/PipeCategorySO.cs`
  - Group of pipe type assets.
- `Assets/_GameFolders/Scripts/_Concretes/ScriptableObjects/Puzzle/PipeTypeSO.cs`
  - Pipe name + prefab entry.

### `_Concretes/Tiles` (1)

- `Assets/_GameFolders/Scripts/_Concretes/Tiles/TileController.cs`
  - Tile bounce animation handling.

### `_Concretes/Validators` (1)

- `Assets/_GameFolders/Scripts/_Concretes/Validators/ConnectionValidator.cs`
  - Runtime BFS validation for connected destinations.

### `BatuhanSevinc/Abstracts` (9)

- `Assets/_GameFolders/Scripts/BatuhanSevinc/Abstracts/DataAccessLayers/IDataSaveLoadDal.cs`
  - Save/load DAL interface.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Abstracts/GameEventHandlers/BaseGameEventListener.cs`
  - Base class for ScriptableObject event listeners.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Abstracts/Patterns/GenericPoolManager.cs`
  - Generic pooling base behavior.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Abstracts/Patterns/SingletonMonoAndDontDestroy.cs`
  - Persistent singleton base.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Abstracts/Patterns/SingletonMonoDestroy.cs`
  - Non-persistent singleton base.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Abstracts/Patterns/SingletonMonoObject.cs`
  - Root singleton abstraction.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Abstracts/Uis/BaseButton.cs`
  - UI button base listener pattern.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Abstracts/Uis/BaseButtonWithGameEvents.cs`
  - Button base that fires `GameEvent`.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Abstracts/Uis/NextLevelButton.cs`
  - Next-level button specialization.

### `BatuhanSevinc/Concretes` (15)

- `Assets/_GameFolders/Scripts/BatuhanSevinc/Concretes/DataAccessLayers/PlayerPrefsDataSaveLoadDal.cs`
  - Concrete DAL using PlayerPrefs + JSON + encryption helper.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Concretes/Enums/ObjectType.cs`
  - Generic object type enum.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Concretes/Enums/SaveLoadType.cs`
  - Save system backend enum.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Concretes/Helpers/EncryptHelper.cs`
  - Encrypt/decrypt helper for persisted strings.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Concretes/Helpers/FpsCounter.cs`
  - Runtime FPS overlay utility.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Concretes/Helpers/IdGeneratorHelper.cs`
  - Random id generation helper.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Concretes/Helpers/MonoExtensionMethods.cs`
  - MonoBehaviour extension helpers.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Concretes/Helpers/SaveLoadDalFactory.cs`
  - DAL factory.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Concretes/Helpers/Vector2Helper.cs`
  - Static Vector2 aliases.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Concretes/Helpers/Vector3Helper.cs`
  - Static Vector3 aliases.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Concretes/Managers/SaveLoadManager.cs`
  - Save/load facade for app usage.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Concretes/Managers/SoundManager.cs`
  - Global sound singleton manager.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Concretes/ScriptableObjects/GameEvent.cs`
  - ScriptableObject event publisher.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Concretes/ScriptableObjects/GameEventListeners/NormalGameEventListener.cs`
  - C# action based event listener.
- `Assets/_GameFolders/Scripts/BatuhanSevinc/Concretes/ScriptableObjects/GameEventListeners/UnityGameEventListener.cs`
  - UnityEvent based event listener.

### `Editor` (4)

- `Assets/_GameFolders/Scripts/Editor/BatchLevelPrefabFixer.cs`
  - Deprecated editor utility notice window.
- `Assets/_GameFolders/Scripts/Editor/LevelControllerValidator.cs`
  - Inspector validator for `LevelController`.
- `Assets/_GameFolders/Scripts/Editor/LevelManagerValidator.cs`
  - Inspector validator and camera target helper for `LevelManager`.
- `Assets/_GameFolders/Scripts/Editor/VisualLevelEditor.cs`
  - Full visual level authoring, generation, validation tool.

---

## External Dependencies Seen in Scripts

- DOTween (`DG.Tweening`)
- TextMeshPro (`TMPro`)
- Newtonsoft Json (`Newtonsoft.Json`)
- Unity AudioMixer (`UnityEngine.Audio`)
- Scene management (`UnityEngine.SceneManagement`)

---

## Practical Navigation Pointers

If you are new to this project, read in this order:
1. `LevelManager` -> runtime bootstrap
2. `LevelGenerator` -> scene build from data
3. `LevelController` + `ConnectionValidator` -> completion logic
4. `PieceBase` + piece controllers -> interaction model
5. `PipeConnectionHelper` -> connection rules source of truth
6. `VisualLevelEditor` + `LevelValidator` -> content creation and solvability checks

