Step-skill: executes the release checklist in build-notes.md and ships the final build. Last step in Phase 3 beta.

---

## Agent

| Step | Agent |
|---|---|
| Smoke test | `qa-lead` |
| Build fixes (if needed) | `gameplay-programmer` |

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/project-snapshot-index.md` | Read | Current scene hierarchy — verify scene is complete before smoke test |
| `.claude/docs/beta/build-notes.md` | Read + Write | Release checklist and build steps; tick each item as completed |
| `.claude/docs/PIPELINE.md` | Read + Write | Confirm all prior Phase 3 items are checked; tick Ship |
| `.claude/rules-for-skill/rule-read-write-unity.md` | Read | Compile check, play/stop, save, snapshot — Unity editor workflow |
| `.claude/rules-for-skill/rule-what-to-do-get-block-by-previous-step.md` | Read | When to call /regress instead of patching |
| `.claude/rules-for-skill/rule-pipeline-progression-update.md` | Read | When and how to tick PIPELINE.md |

---

## Entry Condition

Two conditions must both be true:
1. All other Phase 3 PIPELINE.md items are `[x]` (Juice pass, Feel tuning, Difficulty tuning, Bug pass, Performance pass)
2. `build-notes.md` has no placeholder values remaining

If either condition fails, stop and state exactly what remains.

---

## Steps

1. Read `build-notes.md` — if any placeholder values exist, stop and ask the user to fill them before proceeding
2. Read PIPELINE.md — confirm all Phase 3 items except Ship are `[x]`; if any are unchecked, list them and stop
3. Follow the `build-notes.md` release checklist line by line:
   - Run final smoke test via `play_game` — confirm the game is playable end-to-end; call `stop_game`
   - Execute each build step in `build-notes.md` in order
   - Tick each checklist item in `build-notes.md` as it is completed
4. After all checklist items are ticked: tick PIPELINE.md `- [x] Ship`
5. Report: "Shipped. Build complete."

---

## Exit Condition / Gate

All `build-notes.md` checklist items ticked. PIPELINE.md Ship item checked. All Phase 3 items are `[x]`.

---

## Constraints

- Never attempt the build if any prior Phase 3 item is unchecked
- Never skip the smoke test
