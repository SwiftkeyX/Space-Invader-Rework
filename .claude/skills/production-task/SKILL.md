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

Wait for explicit user approval before proceeding to Sub-phase B. On `approved`, tick
`- [ ] Milestone 1 — all system GDDs written and approved` → `[x]` in PIPELINE.md.

**Step 3 — Sub-phase B: Code All (auto-sequential, with a test gate after every tier)**

**Gate:** do not run `/code-system` for any system until `Milestone 1 — all system GDDs written and approved` = `[x]`. If it is unchecked, return to Step 2.

For each system in PIPELINE.md Phase 2 that is still unchecked, run `/code-system` in tier order:
Tier 1 → [Test Gate 1] → Tier 2 → [Test Gate 2] → Tier 3 → [Test Gate 3].

**When every system in a tier is ticked, run that tier's test gate before starting the next tier.**
The procedure is the same for all three gates:

1. Call `play_game`. Let it run for 10–15 seconds. Call `stop_game`.
2. Check the console for errors.
3. Present to the user — use the gate's checklist text from PIPELINE.md:

   > "⏸️ Test Gate N reached — <gate line text from PIPELINE.md>.
   > Console: [clean / N errors found].
   >
   > Please play-test. Reply 'continue' when it passes, or describe any issues to fix first."

4. If the user describes issues, fix them (via `/debug` or inline), re-test, then re-present.
5. On explicit confirmation, tick the `🧪 Test Gate N` line in PIPELINE.md, then start the next tier.

After all systems and all three test gates are checked:
- Confirm Milestone 2 is ticked (all Tier 1 + Tier 2 systems done, Test Gates 1 & 2 passed)
- Confirm Milestone 3 is ticked (all systems done, Test Gate 3 passed)

**Step 4 — Report**
"All systems designed, implemented, and tested. Milestone 3 reached."

**Step 5 — Go/no-go gate (continue or ditch)**

The base game is built and proven. Present the decision before any architecture work:

> "✅ Base game is built and tested (all tiers + test gates passed, Milestone 3 reached).
> Two ways forward:
> **continue** → run `/architecture-pass` to refactor the code architecture to match the GDDs before Phase 3 polish, or
> **ditch** → the idea didn't pan out; stop here (no point polishing a game you'll drop).
> Reply `continue` or `ditch`."

- On `continue` → run `/architecture-pass`. Phase 2 is complete only when it ticks the Architecture pass item.
- On `ditch` → stop and report: "Stopped at Milestone 3 — game not carried into the architecture pass. Phase 3 stays locked."

---

## Exit Condition

Milestone 3 = [x] in PIPELINE.md, and the user has either passed the architecture pass (Architecture pass = [x], via `/architecture-pass`) or chosen to ditch the game.

---

## Constraints

- Sub-phase B must not begin before the user explicitly approves all GDDs — that approval ticks `Milestone 1`, which is the hard gate for coding (no `/code-system` while Milestone 1 is unchecked)
- Sub-phase B must pause at each tier's test gate and wait for explicit user confirmation before starting the next tier — never tick a test gate before its `play_game` pass is confirmed
- Never enter `/architecture-pass` without an explicit `continue` at the go/no-go gate — `ditch` is a valid, final outcome
