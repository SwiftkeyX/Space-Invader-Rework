Phase 3 beta orchestrator: enforces Phase 2 completion, then auto-runs beta step-skills in sequence until ship.

---

## Agent

Orchestrator — routes to sub-skills. See sub-skill agent assignments.

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/PIPELINE.md` | Read | Verify entry condition; find first unchecked Phase 3 item |
| `.claude/rules-for-skill/rule-what-to-do-get-block-by-previous-step.md` | Read | When to call /regress instead of patching |

---

## Entry Condition

Milestone 3 = [x] **and** Architecture pass = [x] in PIPELINE.md. If either is unmet, stop: "Phase 3 is locked — complete all Phase 2 systems (Milestone 3) and the architecture pass (`/architecture-pass`) first."

---

## Steps

**Step 1 — Entry check**
Read PIPELINE.md. Confirm Milestone 3 = [x] **and** Architecture pass = [x]. If either is unmet, stop with the message above.

**Step 2 — Find first unchecked Phase 3 item**
If all Phase 3 items are `[x]`, report "Phase 3 complete." and stop.

**Step 3 — Route to the correct step-skill**

| PIPELINE.md item | Step-skill |
|---|---|
| Juice pass | `/juice-pass` |
| Feel tuning | `/tune-pass` |
| Difficulty tuning | `/tune-pass` |
| Bug pass | `/fix-all-bugs` |
| Performance pass | `/performance-pass` |
| Ship | `/release-pass` |

Run the step-skill for the first unchecked item. After it completes and ticks its PIPELINE.md item, continue automatically to the next unchecked item.

**Note on `/tune-pass`:** It reads PIPELINE.md to self-identify whether it is running a feel pass or a difficulty pass — invoke it the same way for both.

---

## Exit Condition

Ship gate passed — all Phase 3 items checked.

---

## Constraints

- Never skip a step-skill — all items must be run in order
