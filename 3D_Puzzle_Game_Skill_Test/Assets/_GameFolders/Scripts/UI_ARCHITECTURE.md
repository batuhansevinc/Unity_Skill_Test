# UI Architecture (Hierarchical Init/Deinit)

## Scope
Bu dokuman `GameScene + UI + Splash UI` runtime akisinda kullanilan yeni hiyerarsik yapinin referansidir.

## Runtime Ownership
- `GameSceneManager` akisin sahibidir.
- `LevelManager` sadece level yukleme/ilerleme/input durumunu yonetir.
- `UIManager` sadece alt UI controller'larini initialize/deinitialize eder.
- `LevelController` sadece typed completion event'leri emit eder.

## GameScene Hierarchy
- `GameSceneManager`
- `LevelManager`
- `UIManager`
- `UIManager` children:
  - `HudController`
  - `SettingsPopupController`
  - `EndGamePopupController`

## Splash Hierarchy
- `SplashSceneManager`
- `SplashScreenController`

## Init Order (GameScene)
1. `GameSceneManager.Initialize()`
2. `LevelManager.Initialize(inputManager)`
3. `UIManager.Initialize(gameSceneManager)`
4. `LevelManager` ilk leveli yukler ve `LevelLoaded(LevelController, int)` emit eder.
5. `GameSceneManager` yeni `LevelController` event'lerine subscribe olur.

## Completion Flow
1. `LevelCompletionAnimationStarted` -> `UIManager.ShowEndGame(true)`
2. `FireworksTriggered` -> `UIManager.PlayEndGameVfx()`
3. `LevelCompleted` -> HUD level text refresh

## Button Mapping
- `NextLevelButton` -> `GameSceneManager.RequestMoveToNextLevel()` -> `LevelManager.MoveToNextLevel()`
- `ReTryButton` -> `GameSceneManager.RequestReloadCurrentLevel()` -> `LevelManager.ReloadCurrentLevel()`
- `SettingsButton` / `CloseSettingsButton` sadece `SettingsPopupController` icinde panel gorunurlugunu yonetir.

## Lifecycle Rules
- UI/scene flow scriptlerinde `Awake/Start/OnEnable` yoktur.
- Sadece ust seviye scene manager'lar (`GameSceneManager`, `SplashSceneManager`) Unity lifecycle entry point kullanir.
- Diger scriptler init/deinit ile calisir.

## Reference Rules
- UI tarafinda `FindObjectOfType`, `GetComponent`, `GetComponentsInChildren` ve fallback auto-reference yoktur.
- Tum baglantilar inspector/serialized referansla acik ve gorunur sekilde tutulur.

## Removed Event System
- ScriptableObject tabanli `GameEvent` altyapisi ve listener'lari runtime akistan kaldirilmistir.
- `BaseButtonWithGameEvents` kullanimi kaldirilmistir.
- Button akisleri callback injection modeli ile calisir (`BaseButton.Initialize(Action)`).
