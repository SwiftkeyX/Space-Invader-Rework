Phase 1 pre-production orchestrator: checks entry condition, then auto-runs all /write-* step-skills in sequence until Milestone 0 is complete.

---

## Agent

Orchestrator — routes to sub-skills. See sub-skill agent assignments.

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/PIPELINE.md` | Read | Find first unchecked Phase 1 item; verify Milestone 0 on exit |
| `.claude/rules-for-skill/rule-what-to-do-get-block-by-previous-step.md` | Read | When to call /regress instead of patching |

---

## Entry Condition

Always open — Phase 1 has no prerequisite.

---

## Steps

**Step 1 — Entry check**
Read `.claude/docs/PIPELINE.md`. If all Phase 1 items are `[x]`, report "Phase 1 complete — Milestone 0 achieved." and stop.

**Step 2 — Find first unchecked item and auto-run step-skills in sequence**

| PIPELINE.md item | Step-skill |
|---|---|
| Fill out `game-vision.md` | `/write-game-vision` |
| Fill out `design-decisions.md` | `/write-design-decisions` |
| Fill out `technical-preferences.md` | `/write-technical-preferences` |
| Fill out `systems-design.md` | `/write-systems-design` |
| Fill out `architecture.md` | `/write-architecture` |
| Fill out `best-practices.md` | `/write-best-practices` |

Start at the first unchecked item. Run its step-skill. After it completes and ticks its PIPELINE.md item, continue automatically to the next unchecked item. Do not pause between skills unless the skill itself requires user input.

**Step 3 — Exit check**
After all 6 docs are checked, confirm Milestone 0 is ticked. Report "Phase 1 complete."

---

## Exit Condition

Milestone 0 = [x] in PIPELINE.md (all 6 docs filled).

---

## Constraints

- Never skip a step-skill — run every unchecked item in order
- Never modify `template-docs/` files
