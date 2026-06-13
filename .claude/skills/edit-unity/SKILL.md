**[INTERNAL — reached only via `/code` or `/fix-bug`. Do not invoke directly.]**

Atomic skill — single purpose: apply ONE Unity change safely. Wraps the fixed pre/post checklist — snapshot read, change, compile check, play-test, save, snapshot update — around the change. Has no design opinion: it does not read or write GDDs. Called by `/code` and `/fix-bug`; the gate that ensures the GDD was read first lives in `/code`, not here.

---

## Agent

`gameplay-programmer`

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/project-snapshot-index.md` | Read + Write | Locate affected area before change; refresh after change |
| `.claude/rules-for-skill/rule-read-write-unity.md` | Read | Full procedural reference for Unity editor workflow |

---

## Steps

**Step 1 — Read snapshot**

Read `project-snapshot-index.md` to locate the scene, GameObject, script, or prefab affected by the task.

If the file does not exist: run `GenerateProjectSnapshot.Execute()` via `execute_script` to generate it, then read it.

---

**Step 2 — Execute the task**

Perform the Unity work the user described. Use coplay MCP tools appropriate to the task:
- Scene/GameObject changes: `get_unity_editor_state`, `create_game_object`, `set_property`, `set_transform`, `parent_game_object`
- Script changes: write or edit the `.cs` file directly
- Prefab changes: `place_asset_in_scene`, `set_property`

Stay within the scope the user specified — do not make unrequested changes.

---

**Step 3 — Compile check**

Call `check_compile_errors`. Fix all errors before continuing. Do not proceed to Step 4 with compile errors outstanding.

---

**Step 4 — Test**

Call `play_game`. Observe the specific behaviour the task was meant to produce. Call `stop_game`. If the behaviour is wrong, fix it and repeat from Step 3.

---

**Step 5 — Save**

Call `save_scene`.

---

**Step 6 — Update snapshot**

Run `GenerateProjectSnapshot.Execute()` via `execute_script` to refresh `project-snapshot-index.md`.

---

## Exit Condition

Change is in the scene, compile errors are zero, play-test passed, scene saved, snapshot updated.

---

## Constraints

- Never skip Step 1 — read the snapshot before touching anything
- Never proceed past Step 3 with compile errors
- Never mark done before Step 6 — snapshot must reflect the new state
- Stay within the user's stated scope — no unrequested changes
