# Rule: Unity Editor Workflow

> **Procedural reference for skills.** The `/edit-unity` skill enforces these steps automatically; it is reached via `/code` (build/change) or `/fix-bug` (bug repair). This file is read by skills that need the full procedure.

These rules apply to any skill that reads from or writes to the Unity Editor.

## Before Any Change
Read `project-snapshot-index.md` to locate the affected GameObject, script, or asset.
If any expected section (scenes, scripts, prefabs, audio, UI assets, levels, materials, particles) is absent or incomplete, call `GenerateProjectSnapshot.Execute()` via `execute_script` immediately, tell the user what was missing, and confirm the snapshot was refreshed before continuing.

## After Writing or Editing Code
Call `check_compile_errors` — fix all errors before continuing.

## Testing
Call `play_game`, observe the specific behaviour, then call `stop_game` before marking done.

## After Scene Changes
Call `save_scene`.

## After Adding or Removing GameObjects, Scripts, or Prefabs
Run `GenerateProjectSnapshot.Execute()` via `execute_script` to update the snapshot.

## Project Snapshot Index

**Canonical path:** `.claude/docs/project-snapshot-index.md`
(root of the `.claude/docs/` folder — same level as `PIPELINE.md`)

A living reference that maps every scene, GameObject, script, prefab, material, and audio asset in the project. Read it before any change and update it after every change.

### If the file does not exist
Run `GenerateProjectSnapshot.Execute()` via `execute_script` to generate it.
If the generator script is also absent, create the file manually using this structure:

```markdown
# Project Snapshot Index
Last updated: <YYYY-MM-DD>

## Scenes
| Scene | Path | Root GameObjects |
|---|---|---|
| Bootstrap | Assets/Scenes/Bootstrap.unity | GameManager, AudioManager, SceneLoader, BulletPool, PowerUpSystem, ScoreSystem |

## Scripts
| Script | Path | Attached to |
|---|---|---|
| GameManager | Assets/Scripts/GameManager.cs | GameManager (Bootstrap) |

## Prefabs
| Prefab | Path |
|---|---|

## Audio Clips
| Clip | Path |
|---|---|

## UI Assets
| Canvas / Panel | Scene | Hierarchy path |
|---|---|---|
```

### After any change
Run `GenerateProjectSnapshot.Execute()` via `execute_script` to refresh the file.
If the script is absent, update the relevant table rows manually before saving.
