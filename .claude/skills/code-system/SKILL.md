Step-skill: implements one system in Unity from its approved GDD. Run by /production-task during Sub-phase B after all GDDs are approved.

---

## Agent

`gameplay-programmer`

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/production/gdd/<SystemName>.md` | Read | Approved GDD — implementation source of truth |
| `.claude/docs/preproduction/systems-design.md` | Read | Tier membership and inter-system dependencies — used by Step 0 to identify parallel candidates (frozen artifact, valid for tier info) |
| `.claude/docs/production/gdd/<SystemName>.md` | Read | Communication patterns, SRP, and allowed references — primary implementation spec |
| `.claude/docs/preproduction/best-practices.md` | Read | Project-critical patterns — override everything |
| `.claude/docs/preproduction/technical-preferences.md` | Read | Performance budgets to respect while coding |
| `.claude/docs/project-snapshot-index.md` | Read (if exists) + Write | Current scene state; update after changes |
| `Assets/Scripts/<SystemName>.cs` | Write | Primary output script |
| `.claude/docs/PIPELINE.md` | Read + Write | Tick system item on successful test |
| `.claude/rules-for-skill/rule-read-write-unity.md` | Read | Compile check, play/stop, save, snapshot — Unity editor workflow |
| `.claude/rules-for-skill/rule-what-to-do-get-block-by-previous-step.md` | Read | When to call /regress instead of patching |
| `.claude/rules-for-skill/rule-pipeline-progression-update.md` | Read | When and how to tick PIPELINE.md |

---

## Project Snapshot Index

See `rule-read-write-unity.md` — full instructions (path, fallback generation, manual template, and post-change refresh) live there so all Unity skills share them.

---

## Entry Condition

GDD at `.claude/docs/production/gdd/<SystemName>.md` must exist and be approved. If missing, call `/regress "Design <SystemName> GDD" "GDD required before coding"`.

---

## Steps

**Step 0 — Parallel spawn for independent co-tier systems (lead system only)**

1. Read `preproduction/systems-design.md` to find which tier this system belongs to and its full dependency list.
2. Read `PIPELINE.md` to get all unchecked systems in the same tier.
3. For each unchecked co-tier system, check if it and the current system are mutually independent (neither lists the other as a dependency in `systems-design.md`).
4. For each independent co-tier system whose GDD exists at `.claude/docs/production/gdd/<SystemName>.md`:
   - Spawn a parallel `gameplay-programmer` agent with the full Steps 1–10 instructions for that system.
   - Include this instruction in the agent prompt: **"Do NOT run Step 0 — you are a parallel child agent."**
5. Wait for all parallel agents to complete before continuing to Step 1 for the current (lead) system.

If no independent co-tier systems are pending, skip directly to Step 1.

---

**Steps 1–10 — Implementation (run by lead and parallel child agents)**

> The pre-code GDD gate applies here too: run `/read-gdd` first. During production the GDD was just approved in Sub-phase A, so this is usually a quick confirm — but if implementing reveals the GDD is wrong, stop and run `/write-gdd` to correct it before coding, then continue.

1. Read all input docs in the order listed above
2. **Verify editor state** — call `get_unity_editor_state` and `list_game_objects_in_hierarchy`
3. **Check existing scripts** — call `list_files` on `Assets/Scripts/`
4. **Write script(s)** — write `Assets/Scripts/<SystemName>.cs` per the GDD
   - One script per responsibility — never merge two responsibilities into one file
   - Apply every rule from `best-practices.md` exactly
   - Respect all performance budgets from `technical-preferences.md`
5. **Compile check** — call `check_compile_errors`; fix all errors before continuing
6. **Wire scene** — place GameObjects, assign component references, set transforms per the GDD
7. **Save** — call `save_scene`
8. **Test** — call `play_game`, observe console, call `stop_game`; fix any issues and re-test until clean
9. **Update snapshot** — run `GenerateProjectSnapshot.Execute()` via `execute_script`
10. **Tick PIPELINE.md** — change `- [ ] <SystemName>` to `- [x] <SystemName>`; if all systems in a tier are now checked, tick that tier's milestone line too

---

## Exit Condition

`play_game` test passes with no errors. PIPELINE.md item ticked. Snapshot updated.

---

## Constraints

- Never start coding without reading the GDD first
- Never tick PIPELINE.md before `play_game` passes
- Spawned parallel child agents must skip Step 0 — Step 0 is the lead system's responsibility only (prevents infinite recursion)
