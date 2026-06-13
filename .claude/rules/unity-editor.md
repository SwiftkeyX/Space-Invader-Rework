# Working with the Unity Editor

This project is developed through the **coplay MCP tools** — use them to create/modify GameObjects, scripts, materials, and scenes rather than writing raw `.unity` YAML.

Key coplay workflow:
- `get_unity_editor_state` / `list_game_objects_in_hierarchy` — inspect current scene state
- `set_unity_project_root` — required first call if multiple Unity instances are open
- `create_game_object` / `set_property` / `set_transform` — build scene objects
- `execute_script` — run one-shot editor scripts for bulk operations
- `play_game` / `stop_game` — test in Play Mode
- `check_compile_errors` — verify scripts compile before testing
- `save_scene` — always save after changes

## Entry point (the gate)

**Never write to Unity without reading the GDD first.** All Unity changes go through an entry skill, never by prompting Claude directly:

- **Building or changing something → `/code`.** It runs `/read-gdd` (the mandatory spec gate), then `/write-gdd` if the spec is wrong, then `/edit-unity`.
- **Fixing a known bug → `/debug`** → `/fix-bug`, which also reads the GDD before touching anything.

`/edit-unity` is the atomic executor — it enforces the snapshot read, compile check, play-test, save, and snapshot update in order, but has no design opinion. The "read the GDD first" gate is enforced by `/code` and `/fix-bug`, not by `/edit-unity`. Do not call `/edit-unity` directly to bypass the gate.

## Scene management

- **Never** call `DontDestroyOnLoad()` — put persistent objects in `Bootstrap.unity` instead.
- **Never** load a scene with `LoadSceneMode.Single` or call `SceneManager.LoadScene` directly — use `SceneLoader`.
- **Never** place all GameObjects in one scene — follow the Bootstrap / MainMenu / GameLogic / HUD split.
- **Always** create new scenes at `Assets/Scenes/<SceneName>.unity` — never at the `Assets/` root.
- See `best-practices.md` for the full rules.
