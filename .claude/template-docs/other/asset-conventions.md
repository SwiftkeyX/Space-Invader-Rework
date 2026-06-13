# Asset Conventions

> Naming rules, folder layout, and import settings. Claude follows these when placing or referencing assets.

## Folder Structure

```
Assets/
  Scripts/        — all C# scripts, one script per responsibility
  Prefabs/        — prefab assets
  Scenes/         — Unity scene files
  Art/
    Sprites/      — 2D sprites and spritesheets
    Models/       — 3D models (.fbx)
    Textures/     — standalone textures
    Materials/    — Unity materials
  Audio/
    Music/
    SFX/
  ScriptableObjects/  — data assets
  UI/             — UI prefabs and panel settings
```

## Naming Rules

| Asset type | Convention | Example |
|---|---|---|
| Scripts | PascalCase | `PlayerController.cs` |
| Prefabs | PascalCase | `EnemyBasic.prefab` |
| Scenes | PascalCase | `MainMenu.unity` |
| Sprites | snake_case | `player_idle.png` |
| Audio | snake_case | `sfx_jump.wav` |
| ScriptableObjects | PascalCase + type suffix | `WaveData_01.asset` |

## Import Settings

| Asset type | Setting |
|---|---|
| Sprites | Pixels Per Unit: *(define)*, Filter: Point (no anti-alias) for pixel art |
| Audio (SFX) | Load Type: Decompress On Load, Compression: Vorbis |
| Audio (Music) | Load Type: Streaming, Compression: Vorbis |
