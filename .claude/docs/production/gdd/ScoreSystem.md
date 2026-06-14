# ScoreSystem

> **Status**: Draft
> **Last Updated**: 2026-06-13
> **Implements Pillar**: Fun + Explosive — the score is the arcade reward that makes kills "count" and feeds the one-more-run loop.

## Summary

ScoreSystem tracks the run's score. It subscribes to `Enemy.OnEnemyKilled`, adds that enemy's point value (optionally scaled by a combo multiplier), and fires `OnScoreChanged` so the HUD updates. Score is run-scoped — it resets every run (D3, no persistence).

> **Quick reference** — Layer: `Feature` · Priority: `Vertical Slice` · Key deps: `Enemy` (listens to `OnEnemyKilled`)

---

## Overview

This is the arcade scoreboard. Every invader killed is worth points; ScoreSystem sums them for the current run and broadcasts the new total for the HUD to show. Optionally it applies a combo multiplier that rewards fast consecutive kills (an open design lean from `design-decisions.md`, fitting the Chaotic/Explosive pillars). It holds no persistence and no high-score table for v1 (D3/N3) — a run's score lives and dies with the run.

## Player Fantasy

The number going up *fast* is the reward. Clearing a formation in a flurry should send the score spiking (especially if combos land), giving the player a tangible "I crushed that" payoff and a target to beat next run. The fantasy: every kill matters and a great run produces a number worth chasing.

---

## Detailed Design

### Core Rules

1. **Score state**: a single integer `Score`, starting at 0 on run start.
2. **Awarding points**: subscribe to `Enemy.OnEnemyKilled` (carries point value). On the event, add `pointValue * comboMultiplier` to `Score` and fire `OnScoreChanged(Score)`.
3. **Combo multiplier (optional, leaning yes)**: rapid consecutive kills raise a multiplier that decays if the player stops killing. *(Whether combos ship in v1, and the exact curve, is an open decision — see Open Questions. If cut, `comboMultiplier` is always 1.)*
4. **Run-scoped reset (D3)**: on run start, `Score = 0` and any combo state resets. No score carries between runs; no save/high-score persistence in v1.
5. **No game logic side effects**: ScoreSystem only tracks/awards score and emits `OnScoreChanged`. It never changes lives, difficulty, or power-ups. (Score does not gate power-ups — those are between-level choices, D4.)
6. **Single source of score**: only ScoreSystem mutates `Score`; everyone else reads it via `OnScoreChanged` (HUD).
7. ScoreSystem uses no `Find`/`FindObjectOfType`; it subscribes to the contract events.

### States and Transitions

ScoreSystem is effectively stateless beyond its score/combo values; no formal state machine. (If combo is implemented, the only "state" is the active combo timer.)

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Idle` (no combo) | Run start / combo decayed | A kill | Awards base points |
| `Combo` (if implemented) | A kill within the combo window | Combo timer lapses | Awards points × current multiplier; refreshes timer on each kill |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| Enemy | Subscribes to `OnEnemyKilled` (point value). Data in: points. |
| UIManager | Subscribes to `OnScoreChanged` to render the score. Data out: current total. |
| GameManager | Resets score on run start (via `OnRunStarted`). |

---

## Formulas

### Score award

```
Score += pointValue * comboMultiplier
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `pointValue` | int | per enemy tier | Enemy / `LevelData` | Points for the killed enemy |
| `comboMultiplier` | float | 1.0–MaxCombo | ScoreSystem | 1.0 if combos disabled |

**Expected output range**: `Score` grows monotonically within a run; resets to 0 each run.
**Edge cases**: integer overflow not a concern at expected magnitudes; multiplier clamped to `MaxCombo`.

### Combo multiplier (if implemented)

```
on kill: comboCount += 1
comboMultiplier = 1 + clamp(comboCount * ComboStep, 0, MaxCombo - 1)
combo decays to 1 if no kill within ComboWindow seconds
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `ComboStep` | float | 0.05–0.5 | SO/Inspector | Multiplier gained per chained kill |
| `MaxCombo` | float | 2–8 | SO/Inspector | Cap on the multiplier |
| `ComboWindow` | float | 0.5–3 s | SO/Inspector | Time before the combo decays |

> Combo timer should be pause/hit-stop-safe (`unscaledDeltaTime`) consistent with the project timer rule, so hit-stop doesn't silently drop a combo.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Two enemies die the same frame | Both award points (each `OnEnemyKilled` handled); combo increments per kill | Score reflects every kill. |
| Run restart | Score and combo reset to 0/1 | Run-scoped (D3). |
| Kill during hit-stop | Points award normally; combo timer unaffected (unscaled) | Hit-stop shouldn't penalize scoring. |
| Combo cut (no kill in window) | Multiplier returns to 1 | Rewards sustained aggression. |
| Score read before any kill | Returns 0 | Clean initial state. |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| Enemy | This depends on it | State trigger — `OnEnemyKilled` carries points |
| UIManager | It depends on this | State trigger — `OnScoreChanged` updates HUD |
| GameManager | This depends on it | State trigger — `OnRunStarted` resets score |

> Nature options: `Data dependency` · `State trigger` · `Rule dependency` · `Ownership handoff`

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `pointValue` (per enemy) | tier-based | — | Higher scores | Lower |
| `ComboStep` (if combos) | 0.1 | 0.05–0.5 | Combos ramp faster | Slower |
| `MaxCombo` (if combos) | 4 | 2–8 | Bigger score swings | Flatter |
| `ComboWindow` (if combos) | 1.5 s | 0.5–3 | Easier to sustain combos | Harder |

> Point values live in `LevelData`/Enemy config; combo params in a `ScoreConfig` SO/Inspector. No magic numbers.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Score changed | HUD score counter updates (tick-up animation = polish) | Optional score-tick blip | Vertical Slice |
| Combo increased (if implemented) | Combo meter / multiplier pop on HUD | Combo-up SFX | Polish |
| New combo tier | Brief flourish | Rising pitch SFX | Polish |

> ScoreSystem fires the data; UIManager owns the counter/combo-meter presentation (UI Toolkit).

---

## Game Feel

### Feel Reference

> "Score should feel like a *classic arcade scoreboard* — fast, satisfying number climb, with combos like *Tony Hawk*'s multiplier tension (keep the chain alive). NOT a quiet number that updates without fanfare."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Kill → score updates | ≤ 1 frame (same frame as `OnEnemyKilled`) | 1 frame |

### Animation Feel Targets

| Animation | Startup | Active | Recovery | Feel Goal |
|---|---|---|---|---|
| Score tick-up (UIManager) | 0 | brief | 0 | Snappy climb, not laggy |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Big multi-kill / combo spike | brief | Score lurches up + combo flourish (with juice in Phase 3) |

### Weight and Responsiveness

- **Weight**: Light — score is feedback, instant.
- **Player control**: The player drives score via kills and (if combos) by sustaining chains.
- **Snap quality**: Immediate — score reflects each kill the same frame.
- **Failure texture**: N/A; but losing a combo should be readable so the player learns to sustain it.

### Feel Acceptance Criteria

- [ ] Score visibly responds to every kill with no lag.
- [ ] If combos ship: sustaining a chain feels rewarding and losing it feels readable.

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Current score | HUD (top, UIManager) | On `OnScoreChanged` | Always during run |
| Combo multiplier (if implemented) | HUD near score | On combo change | When combo > 1 |
| Final score | Game-over / win screen (UIManager) | On run end | After run ends |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Points per kill | `Enemy.md` | `OnEnemyKilled` point value | State trigger |
| Score/combo display | `UIManager.md` | `OnScoreChanged`, combo meter | State trigger |
| Reset on run start | `GameManager.md` | `OnRunStarted` | State trigger |
| Combo lean | `design-decisions.md` | open question: combo multipliers | Rule dependency |

---

## Acceptance Criteria

- [ ] Score starts at 0 and increments by each killed enemy's point value on `OnEnemyKilled`.
- [ ] `OnScoreChanged` fires on every score change; UIManager reflects it.
- [ ] Score resets to 0 on run start (run-scoped, D3); no persistence/high-score in v1.
- [ ] If combos ship: multiplier ramps per chained kill, caps at `MaxCombo`, decays after `ComboWindow` (unscaled timer).
- [ ] Same-frame multi-kills award all points correctly (no missed/double counts).
- [ ] No `Find`/`FindObjectOfType`; subscribes via the contract.
- [ ] Performance: score handling is O(1) per kill, allocation-free.
- [ ] No hardcoded values — point values and combo params from SO/config.

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| **Do combo multipliers ship in v1?** `design-decisions.md` leans yes (Chaotic/Explosive) but it's uncommitted. | designer | Before implementation | **Pending — needs a decision.** If yes, use the combo formula above; if no, `comboMultiplier = 1` and drop the combo UI. |
| If combos ship: decay model — flat timer reset per kill, or gradual decay? | designer | With the combo decision | Leaning: timer refresh per kill, snap to 1 on lapse (readable). |
| Any high-score persistence at all (even session-local, not saved)? | designer | Before UIManager | Leaning: session-best shown on game-over, no disk save (respects N3). |
