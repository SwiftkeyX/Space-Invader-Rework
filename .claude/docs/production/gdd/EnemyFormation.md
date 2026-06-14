# EnemyFormation

> **Status**: Draft
> **Last Updated**: 2026-06-13
> **Implements Pillar**: Chaotic — the marching wall of invaders that fills the screen and bears down on the player.

## Summary

EnemyFormation owns the marching invader grid: it spawns the wave's `Enemy` children in a grid, moves the whole formation horizontally in lockstep, drops it down and reverses at the screen edges (classic Space Invaders march), and tracks how many invaders remain. When the last enemy in the formation dies, it fires `OnFormationCleared`. Movement speed comes from the level config and accelerates as the formation thins.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `LevelManager`, `Enemy`

---

## Overview

This is the iconic Space Invaders behaviour: a block of invaders sweeping left, hitting the edge, stepping down a row, sweeping right, and so on — getting faster and more threatening as the player thins their ranks. EnemyFormation is the conductor of that grid. It spawns the enemies for the current level (using the level's layout/HP config), moves them as one body, and announces when the formation is wiped so LevelManager can advance. It does not decide who shoots (EnemyFireController) or what an individual invader is worth (Enemy) — it owns *position and formation state* only.

## Player Fantasy

The player should feel a rising tide bearing down — a wall that's manageable at first and frantic by the time it's near the bottom and moving fast. Thinning the formation should feel like progress *and* danger: fewer enemies but faster, building toward the explosive payoff of clearing the last one.

---

## Detailed Design

### Core Rules

1. **Spawning**: on `LevelManager.OnLevelStarted` (which carries the level config), EnemyFormation spawns a grid of `Enemy` instances from the pool — rows × columns per the level layout, positioned with fixed spacing, parented under the formation. Per-enemy HP tier (front rows tankier) comes from the level config (game-vision HP scaling).
2. **Lockstep march**: the whole formation moves horizontally at `MarchSpeed`. All living enemies move together as one rigid body (the formation moves; enemies hold their relative grid slots).
3. **Edge step-down**: when any edge enemy reaches the horizontal play boundary, the formation (a) reverses horizontal direction and (b) drops down by `StepDownDistance`. This is the classic march-and-descend.
4. **Speed-up as it thins**: `MarchSpeed` increases as enemies die — fewer alive → faster march (classic Space Invaders tension). The relationship is defined in the formula below and driven by level-scaled base/max speeds.
5. **Step-style movement vs continuous**: movement may be continuous (smooth) or stepped (discrete hops). v1 target is smooth continuous march for a modern feel; the classic stepped cadence is an option (see Open Questions).
6. **Tracking remaining**: EnemyFormation subscribes to `Enemy.OnEnemyKilled` for its own children and decrements a live count. It owns the set of its enemies (it spawned them).
7. **Formation cleared**: when the live count reaches 0, fire `OnFormationCleared`. LevelManager listens and advances the level.
8. **Reaching the player line (lose condition)**: if the formation descends to the player's row / bottom threshold, that is a fail condition. *(Exact consequence — instant run loss vs. cost a life — see Open Questions; ties to GameManager run-end.)*
9. **Pooling**: enemies are acquired/released via pool (`best-practices.md`); on level clear or run reset, all enemies are released back.
10. EnemyFormation uses no `Find`/`FindObjectOfType`; it holds references to the enemies it spawned and listens to the events in the contract.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Idle` | Before a level / after clear | `OnLevelStarted` received | No enemies; no movement |
| `Marching` | Spawned a wave | Live count = 0, or reaches player line | Lockstep horizontal march + edge step-down; speeds up as it thins |
| `Cleared` | Live count hit 0 | Next `OnLevelStarted` | Fires `OnFormationCleared`; releases enemies |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| LevelManager | Listens to `OnLevelStarted` (carries layout/HP/speed config) to spawn + configure the wave. Fires `OnFormationCleared` → LevelManager advances. |
| Enemy | Spawns/owns Enemy children; listens to `Enemy.OnEnemyKilled` to track remaining and trigger clear. |
| EnemyFireController | Reads the formation's live enemies (shooter set) to pick who fires. EnemyFormation exposes the current shooter candidates; it does not fire itself. |
| GameManager | Indirect — reaching the player line is a run-fail condition routed via the agreed mechanism (Open Questions). |

---

## Formulas

### March speed vs remaining enemies

```
fractionAlive = enemiesAlive / enemiesAtStart
MarchSpeed = lerp(MaxMarchSpeed, BaseMarchSpeed, fractionAlive)   // fewer alive → closer to MaxMarchSpeed
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `enemiesAlive` | int | 0..start | runtime | Current live count |
| `enemiesAtStart` | int | grid size | level config | Wave size |
| `BaseMarchSpeed` | float | per level | `LevelData` | Speed at full formation (1.0×→2.0× across levels, game-vision) |
| `MaxMarchSpeed` | float | per level | `LevelData` | Speed when ~1 enemy remains |

**Expected output range**: `BaseMarchSpeed` (full) → `MaxMarchSpeed` (nearly empty).
**Edge cases**: clamp so speed never exceeds `MaxMarchSpeed`; guard divide-by-zero if start count is 0.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Last enemy killed | Fire `OnFormationCleared` exactly once; release all enemies | Single clean clear signal to LevelManager. |
| Two edge enemies hit the boundary same frame | One reverse + one step-down (not two) | Prevents double-drop / jitter at the edge. |
| Enemy killed mid-step-down | Count decrements; march continues; speed recomputed | Death and movement are independent. |
| Formation reaches player line | Trigger run-fail per agreed mechanism | Classic lose condition (Open Questions). |
| Level cleared while enemies mid-flight to pool | All released cleanly; no orphaned enemies into next level | Clean handoff between waves. |
| Player kills entire wave extremely fast | Clear fires immediately; LevelManager handles pacing | No artificial delay required (juice/transition is Phase 3). |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| LevelManager | This depends on it | Data dependency + state trigger — `OnLevelStarted` config; reports clear back |
| Enemy | This depends on it | Ownership handoff — spawns/owns; listens to `OnEnemyKilled` |
| EnemyFireController | It depends on this | Data dependency — reads live shooter candidates |
| GameManager | It depends on this (indirect) | State trigger — formation-reaches-line fail |

> Nature options: `Data dependency` · `State trigger` · `Rule dependency` · `Ownership handoff`

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `BaseMarchSpeed` (per level) | L1: 1.0× | — | Faster sweep, more pressure | Slower, easier |
| `MaxMarchSpeed` (per level) | L1: ~2× base | — | More frantic endgame of each wave | Flatter tension curve |
| `StepDownDistance` | 0.5 unit | 0.25–1.0 | Reaches player faster (harder) | Slower descent (easier) |
| Grid rows × columns | per level | — | Bigger wall (harder, denser) | Smaller wave |
| Horizontal spacing | fixed | — | Wider formation | Tighter |

> All per-level values live in `LevelData` ScriptableObjects (difficulty in SOs — `best-practices.md`). The march-speed ramp is a primary difficulty surface (game-vision threat-speed axis).

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| March step | (optional step bob) | Classic march "heartbeat" SFX that quickens as formation thins | Alpha |
| Edge step-down | Formation drops a row | Step-down beat | Alpha |
| Formation cleared | (big payoff — Phase 3 juice) | Wave-clear sting | MVP signal / Polish payoff |

> The accelerating march SFX is a signature Space Invaders feel cue — pair it with the speed-up formula (AudioManager, off formation state).

---

## Game Feel

### Feel Reference

> "The march should feel like *Space Invaders'* accelerating descent — that quickening dread as the ranks thin. NOT a static enemy block that just sits and shoots."

### Input Responsiveness

Not directly player-driven; responsiveness is about the formation reacting visibly to kills (speed-up felt within a few kills).

### Animation Feel Targets

| Animation | Startup | Active | Recovery | Feel Goal |
|---|---|---|---|---|
| March step | — | continuous | — | Relentless, quickening |
| Edge drop | instant | step | — | A threatening "lurch" toward the player |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Formation cleared | — | Explosive payoff (Phase 3 juice) — the "fireworks finale" from the vision |

### Weight and Responsiveness

- **Weight**: Heavy and relentless — the formation is an inexorable advancing mass.
- **Player control**: Indirect — the player controls the formation's *speed* by how fast they kill.
- **Snap quality**: Smooth march with crisp edge lurches.
- **Failure texture**: The descent is readable; the player can always see how close the wall is to the line.

### Feel Acceptance Criteria

- [ ] The speed-up as the formation thins is felt (playtesters notice the quickening).
- [ ] Clearing the last enemy delivers a satisfying payoff (with Phase 3 juice).

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| (enemies remaining — optional) | HUD (UIManager) | On kill | Optional; not required for v1 |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Wave config + advance | `LevelManager.md` | `OnLevelStarted` payload, `OnFormationCleared` listener | Data dependency / state trigger |
| Enemy lifecycle | `Enemy.md` | `OnEnemyKilled`, HP tiers, spawn | Ownership handoff |
| Shooter selection | `EnemyFireController.md` | live shooter candidate set | Data dependency |
| Difficulty values | `best-practices.md` | LevelData SO rule | Rule dependency |

---

## Acceptance Criteria

- [ ] On `OnLevelStarted`, spawns the configured grid (rows × cols, per-row HP tiers) from the pool.
- [ ] Formation marches horizontally in lockstep; on edge contact it reverses and steps down by `StepDownDistance`.
- [ ] `MarchSpeed` increases as enemies die per the formula; never exceeds `MaxMarchSpeed`.
- [ ] Killing the last enemy fires `OnFormationCleared` exactly once and releases all enemies.
- [ ] Formation reaching the player line triggers the agreed run-fail.
- [ ] Enemies are pooled; no `Instantiate`/`Destroy` per wave; clean release between levels.
- [ ] No `Find`/`FindObjectOfType`; communicates only via the contract.
- [ ] Performance: full-grid lockstep movement is allocation-free, within 8.3 ms @120fps.
- [ ] No hardcoded layout/speed — all from `LevelData`.

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Continuous smooth march vs classic discrete stepped cadence for v1? | designer | Before implementation | Leaning: smooth continuous for a modern feel; stepped march SFX layered on top. |
| What happens when the formation reaches the player line — instant run loss, or cost a life and push back? | designer / Claude | Before implementation (touches GameManager run-end) | **Pending — resolve with GameManager.** Leaning: instant run loss (classic), routed to GameManager as a run-end. |
| Does EnemyFormation own enemy spawning, or does LevelManager spawn and hand enemies to the formation? architecture.md says formation "Owns/spawns its Enemy children." | Claude | Before implementation | Resolved per architecture: EnemyFormation owns/spawns its children using LevelManager's config payload. |
| Should the speed-up be smooth (lerp) or stepped at thresholds (e.g. last 4, last 1)? | designer | Phase 3 feel pass | Leaning: smooth lerp now; revisit for the iconic "last invader sprints" beat. |
