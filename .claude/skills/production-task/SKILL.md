Phase 2 production orchestrator: enforces Phase 1 completion, then runs two mandatory sub-phases — design all GDDs first, then implement all systems in tier order.

---

## Agent

Orchestrator — routes to sub-skills. See sub-skill agent assignments.

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/PIPELINE.md` | Read | Verify entry condition; find unchecked systems |
| `.claude/rules-for-skill/rule-what-to-do-get-block-by-previous-step.md` | Read | When to call /regress instead of patching |

---

## Entry Condition

Milestone 0 = [x] in PIPELINE.md. If not met, stop: "Phase 2 is locked — complete Phase 1 and check off Milestone 0 first."

---

## Steps

**Step 1 — Entry check**
Read PIPELINE.md. Confirm Milestone 0 = [x]. If not, stop with the message above.

**Step 2 — Sub-phase A: Design All (auto-sequential)**

For every system listed in PIPELINE.md Phase 2 that does NOT yet have a GDD at `.claude/docs/production/gdd/<SystemName>.md`, run `/design-system` for that system. Work in tier order: Tier 1 → Tier 2 → Tier 3. Continue automatically to the next system after each GDD is written.

After all GDDs exist:

> "All GDDs written. Please review each file in `.claude/docs/production/gdd/`. Reply 'approved' when all are confirmed — Sub-phase B will not start until you do."

Wait for explicit user approval before proceeding to Sub-phase B.

**Step 3 — Sub-phase B: Code All (auto-sequential, with Core Playable gate)**

For each system in PIPELINE.md Phase 2 that is still unchecked, run `/code-system` in order:
Tier 1 → Tier 2 Core Invader Loop → [gate] → Tier 2 Advanced Features → Tier 3.

**When all Core Invader Loop systems are ticked and the Core Playable gate line is reached:**

1. Call `play_game`. Let it run for 10–15 seconds. Call `stop_game`.
2. Check the console for errors.
3. Present to the user:

   > "⏸️ Core Playable gate reached.
   > The base invader loop is implemented: player moves + shoots, enemies march and die, win/lose triggers.
   > Console: [clean / N errors found].
   >
   > Please play-test the game. Reply 'continue' when the base game feels right, or describe any issues to fix first.
   >
   > Remaining after this gate: MothershipBoss, PowerUpSystem, then Tier 3."

4. Wait for explicit user confirmation before running `/code-system` for MothershipBoss.
5. If user describes issues, fix them (via `/debug` or inline), then re-test, then re-present the gate message.
6. After confirmation, continue with Tier 2 Advanced Features then Tier 3 as normal.

After all systems are checked:
- Confirm Milestone 1 is ticked (all Tier 1 + Tier 2 systems done)
- Confirm Milestone 2 is ticked (all systems done)

**Step 4 — Report**
"Phase 2 complete — all systems designed, implemented, and tested."

---

## Exit Condition

Milestone 2 = [x] in PIPELINE.md.

---

## Constraints

- Sub-phase B must not begin before the user explicitly approves all GDDs
- Sub-phase B must pause at the Core Playable gate and wait for user confirmation before continuing to Tier 2 Advanced Features (MothershipBoss, PowerUpSystem)
