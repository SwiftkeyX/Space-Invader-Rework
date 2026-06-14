Pipeline enforcer: reads PIPELINE.md, reports current stage, detects regressions, validates gates before advancing, and routes to the correct orchestrator skill.

---

## Agent

`claude`

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/PIPELINE.md` | Read + Write | Source of truth for all gate checks and checkbox updates |
| `.claude/docs/beta/known-issues.md` | Read | Bug pass gate validation |
| `.claude/docs/beta/build-notes.md` | Read | Ship gate validation |

---

## Step 1 — Scan all phases and detect regressions

Read `.claude/docs/PIPELINE.md` in full.

**Regression check:** If any phase that has _some_ checked items also has unchecked items that appear _before_ already-checked items in an earlier phase, report it immediately:

> "Regression detected: Phase N has unchecked items. Run `/<orchestrator>` to address them before continuing Phase M."

List every unchecked item in the regressed phase. Do not proceed to Step 2 until the user acknowledges.

---

## Step 2 — Report current phase and progress

Identify the **active phase**: the earliest phase that has any unchecked `- [ ]` items.

Report in this format:
```
Phase N — <Name>: X/Y done
Entry: <entry condition status — met or not>
Next: <first unchecked item>
```

If all phases are complete: "Pipeline complete — all items checked. Ready to ship."

---

## Step 3 — Enforce entry condition

Check the active phase's **Entry:** line in PIPELINE.md.

- **Phase 1:** Entry is always open — proceed.
- **Phase 2:** Entry requires Milestone 0 = [x]. If not, refuse: "Phase 2 is locked. Complete Phase 1 and check off Milestone 0 first."
- **Phase 3:** Entry requires Milestone 3 = [x] **and** Architecture pass = [x]. If either is unchecked, refuse: "Phase 3 is locked. Complete all Phase 2 systems (Milestone 3) and the architecture pass (`/architecture-pass`) first."

---

## Step 4 — Validate gate when user requests a tick

If the user says an item is done and wants it checked off, validate its gate first:

**Milestone 1 — all system GDDs written and approved (Phase 2, Sub-phase A):**
Tickable only when every "GDD written" cell across all three Phase 2 tier tables is `[x]` **and** the
user has confirmed they approved the GDDs. If any GDD is missing or unapproved, list what's outstanding
and refuse. This milestone is the hard gate for Sub-phase B coding.

**Test Gate (Phase 2):**
Call `play_game`, let it run 10–15 seconds, call `stop_game`, and report the console state. Ask the
user to confirm the gate's checklist passed (the gate line text in PIPELINE.md). Only tick on an
explicit "yes". Until the gate is `[x]`, refuse to tick any system in the next tier.

**Architecture pass (Phase 2 exit):**
This item is ticked by `/architecture-pass` itself, only after a `technical-director` audit reports
clean and the refactor PR(s) merged. Do not tick it here on request — if the user asks, direct them
to run `/architecture-pass` and refuse until its audit is clean.

**Feel tuning / Difficulty tuning:**
Ask the user directly:
- Feel: "Does the game now match the Chaotic · Explosive · Fun pillars described in `game-vision.md`?"
- Difficulty: "Does the difficulty curve escalate correctly across all levels per the Difficulty Curve spec in `game-vision.md`?"
Only tick on explicit "yes."

**Bug pass:**
Read `.claude/docs/beta/known-issues.md`. If the Open table has any real rows (not the `*(no issues yet)*` placeholder), list every open item and refuse to tick until all are resolved.

**Performance pass:**
Call `get_worst_gc_frames`. If any steady-state frame shows non-zero GC alloc, report the worst offender and refuse to tick. Also verify all frames are under 16.6ms.

**Ship:**
- Confirm all other Phase 3 items are `[x]` in PIPELINE.md
- Read `beta/build-notes.md` and confirm every checklist item is ticked
If anything is unticked, list what remains and refuse.

If the gate passes: update `- [ ]` to `- [x]` and report "Gate passed — item checked off."
If the gate fails: report exactly what blocked it and what must be fixed first.

---

## Step 5 — Route to the correct orchestrator

Based on the active phase, tell the user which skill to run:

| Phase | Orchestrator |
|---|---|
| Phase 1 (Pre-production) | Run `/preproduction-task` |
| Phase 2 (Production) | Run `/production-task` |
| Phase 3 (Beta) | Run `/beta-task` |

Format: "Run `/beta-task` to continue."

---

## Constraints

- Always scan all phases for regressions before reporting status — never assume earlier phases are clean
- Never tick a gated item without running the gate check — not even if the user insists
- Never tick a system in a tier while the previous tier's test gate is unchecked — the test gate blocks tier advancement
- Never tick a system's *Implemented* cell (Sub-phase B) while `Milestone 1 — all system GDDs written and approved` is unchecked — coding is gated on the GDD milestone
- Keep the response short: status line, next action, route suggestion. No preamble.
