# Gaze — VR Cervical Rehabilitation

A Unity VR prototype for cervical rehabilitation on Meta Quest. Everything runs through head and neck movement: where you look, how you tilt, how you turn. Easy mode uses a controller to move forward; Medium and Hard use nothing but your head.

The point is to wrap therapeutic exercises (yaw, pitch, roll) inside an actual game so patients bother doing them.

---

## How it works

All interaction goes through gaze. A raycast fires from the camera's forward direction and hits objects that implement `IGazeable`. To select something, you hold your gaze on it for a few seconds, a visual progress indicator fills up, and the action fires. No physical inputs required during gameplay.

Controllers are still supported for menus, but the game itself is entirely head-driven.

---

## Scenes and game modes

**Main Menu** - Gaze-driven. Select a difficulty, confirm, scene loads.

**Easy - Forest walk**
Walk through a forest using a controller to move forward. Apples appear on both sides and you collect them by looking at them. Obstacles come from ahead; tilt your head to dodge. Session ends when you collect all 20 apples. Score shows up in a results screen that appears wherever you're looking when the game ends.

**Medium - Worm tunnel**
Guide a worm through a procedurally generated tunnel using gaze. The worm follows your head direction. Session runs on a timer (default 60 seconds). At the end you get your score and gaze accuracy, how long you actually kept your gaze on target vs. total session time.

**Hard - Maze**
A flat maze is projected in front of you. A laser pointer follows your head movement and you drag it through the maze to find the exit. You pick the session duration before starting and the game cycles through multiple maze layouts. It tracks how many mazes you finish and how long you took.

---

## Project structure

```
Assets/
  Scripts/
    Gaze/          - GazeSelector, GazeButton, GazeTimerButton, progress indicator
    Movement/      - PlayerMovement (Easy forward walk)
    Collectibles/  - Apple, CoinCollectible, spawners
    Obstacles/     - Obstacle, ObstacleSpawner (Easy mode)
    Worm/          - WormMover, WormHead, TunnelBuilder, TunnelCameraFollow (Medium)
    Maze/          - MazeGameManager, LaserPointer (Hard)
    Difficulty/    - DifficultySettings ScriptableObject, DifficultyConfig, DifficultyManager
    XR/            - CameraHeightInitializer
  Editor/
    MazeTextureImporter.cs   - auto-enables Read/Write on maze textures
    PlayFromMainMenu.cs      - always starts Play from MainMenu in editor
  Plugins/Android/
    AndroidManifest.xml
  XR/Settings/
    OpenXRPackageSettings.asset
```

---

## Platform and XR setup

- Target: Meta Quest 2 / 3 / 3S (Android, OpenXR, OculusLoader)
- XR rig: XR Origin -> Camera Offset -> Main Camera
- Main Camera has `GazeSelector` and `TrackedPoseDriver` (centerEye)
- Movement scripts live on XR Origin, not on Main Camera
- `MouseGazeSimulator` works in editor and disables itself automatically on device
- XR Interaction Simulator prefab is included in each scene for editor testing without a headset

Input System is set to New Input System only. `UnityEngine.Input` is not used anywhere.

---

## Running in editor

1. Open `MainMenu` scene
2. Enter Play mode, the `PlayFromMainMenu` editor script handles scene selection automatically
3. Use mouse to simulate gaze in editor (disabled automatically on Quest)

To build for Quest:
- Build target: Android
- XR Plugin: OpenXR with OculusLoader
- Run `adb logcat` if the app crashes on device, Unity errors appear under `AndroidRuntime`

---

## Difficulty settings

Each difficulty (Easy / Medium / Hard) has a `DifficultySettings` ScriptableObject with:

- `moveSpeed` - forward movement speed (Easy)
- `obstacleSpawnInterval` - how often obstacles appear
- `obstacleSpeed` - how fast they come at you
- `gazeSelectTime` - how long you need to hold gaze to activate a button

These live in `Assets/Scripts/Difficulty/` and are assigned per-scene in the Inspector.

---

## Known things to watch out for

- Maze textures need Read/Write enabled, `MazeTextureImporter.cs` handles this automatically on import
- The AndroidManifest must include `Theme.AppCompat` on the activity declaration or the app crashes before Unity starts
- Never add hand tracking metadata to the manifest unless the app actually uses the Hand Tracking SDK
- `Mouse.current` is null on Android, always null-guard before accessing it
- `GameObject.Find()` does not find inactive GameObjects, use `FindObjectsByType` with `FindObjectsInactive.Include`
