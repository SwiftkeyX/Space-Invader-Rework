**[INTERNAL — reached only via `/debug`. Do not invoke directly.]**

Atomic skill — single purpose: manage the repair of ONE known bug. Diagnoses and reports the suspected root cause for approval, delegates the actual change to `/edit-unity`, then archives the bug card. Does NOT re-implement the generic write checklist (that is `/edit-unity`'s job).

---

## Agent

`gameplay-programmer`

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/project-snapshot-index.md` | Read | **Mandatory first read** — current scene hierarchy, scripts, prefabs; locate the affected object/component |
| `.claude/docs/beta/known-issues.md` | Write | Remove the bug card from Open after the fix is confirmed |
| `.claude/docs/beta/known-issues-archive.md` | Write | Append full card + fix note after the fix is confirmed |
| `.claude/docs/production/gdd/<system>.md` | Read | Per-system constraints — ensure the fix respects system boundaries |
| `.claude/docs/preproduction/best-practices.md` | Read | Project-critical patterns that override all other decisions |
| `.claude/rules-for-skill/rule-what-to-do-get-block-by-previous-step.md` | Read | When to call /regress instead of patching |

---

## Entry Condition

`/debug` routed here with a specific bug to fix. `project-snapshot-index.md` must exist.

---

## Steps

1. **Read the design docs (`/read-gdd`)** — the same gate `/code` uses. Run `/read-gdd` for the affected system (GDD, plus `design-decisions.md` / `game-vision.md` if the bug touches a mechanic or pillar). If the bug reveals a doc itself is wrong, run `/write-gdd` to correct it before diagnosing — with a good-enough reason and user approval, never a casual edit (bug fixes are where GDDs silently drift).
2. **Locate** — read `project-snapshot-index.md` to find the affected GameObject, script, or asset.
3. **Inspect live state** — call `get_unity_editor_state` and `get_game_object_info` on the affected object; verify current values match the snapshot.
4. **Diagnose and report** — before touching anything, tell the user:
   - The suspected root cause, with file + line reference.
   - What the fix will change and why.
   - What could go wrong or which edge cases it might miss.
   End with "**Apply the fix?**" — wait for explicit approval.
5. **Apply via `/edit-unity`** — hand the approved change to `/edit-unity`, which performs the snapshot → change → compile → play-test → save → snapshot checklist. One logical change only.
6. **Confirm with user** — ask "Did it work? (yes / no / something else happened)".
   - **yes** → proceed to Step 7.
   - **no / something else** → diagnose further; do not archive yet.
7. **Archive the bug** — if this bug came from `known-issues.md`: remove its card from the Open section, then append the full card (all original fields + "Fixed in: YYYY-MM-DD — <one-line fix description>") to `known-issues-archive.md` (summary-table row + full `### #N` card below).

---

## Exit Condition

User confirms the fix works, the change is applied and saved (via `/edit-unity`), and the bug card is archived.

---

## Constraints

- Never apply a fix before inspecting current state (Steps 2–3)
- Never apply a fix before reporting diagnosis and receiving approval (Step 4)
- Never archive the bug until the user explicitly confirms it worked (Step 6)
- Never re-implement the compile/play-test/save/snapshot checklist here — delegate to `/edit-unity`
