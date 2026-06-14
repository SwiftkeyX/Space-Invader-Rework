# LevelManager

> **Status**: Draft
> **Last Updated**: 2026-06-13
> **Implements Pillar**: Fun (pressure that builds) — drives the short, steep 6-level curve that defines a run.

## Summary

LevelManager drives the run's 6 fixed levels (D5). For the current level it loads the matching `LevelData` ScriptableObject and broadcasts `OnLevelStarted` (carrying the level's HP/speed/fire config) so EnemyFormation and EnemyFireController build the wave. When the formation is cleared it fires `OnLevelCleared`, which triggers the between-level power-up offer and advances the run toward the next level (or the win). It owns the per-level difficulty config; it does not own lives or score.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `GameManager`, `EnemyFormation`

---

## Overview

A run is six levels, each noticeably harder (game-vision difficulty curve). LevelManager is the conductor of that progression: it knows which level is current (from GameManager's level index), pulls that level's tuned values from a `LevelData` asset, and tells the enemy systems to spawn and configure the wave. When the wave is wiped, it announces the clear so the power-up system can offer an upgrade and the run can step to the next level — until level 6 is cleared (win). All the difficulty knobs of the curve live in the `LevelData` assets it reads, making the curve the primary Phase 3 tuning surface.

## Player Fantasy

"Pressure that builds." Each level should feel like a clear step up — the player notices the formation marching faster, more bullets, tankier front rows — and the between-level power-up choice is the breather and the build moment before the next spike. The fantasy: a tight, escalating gauntlet that ends in either a sweaty victory at level 6 or a death that makes them want one more run.

---

## Detailed Design

### Core Rules

1. **Six fixed levels (D5)**: `TotalLevels = 6` (mirrors GameManager). There is no level 7 and no endless mode (N7).
2. **LevelData assets**: each level has a `LevelData` ScriptableObject holding all tuned values — formation layout (rows × cols), per-row HP tiers, march base/max speed, fire config (shooters, interval, patterns, bullet speed), point values, and pacing target. No level values are hardcoded (`best-practices.md`).
3. **Starting a level**: LevelManager learns the current level (1-based) from `GameManager.OnLevelChanged` (and at run start). It selects the matching `LevelData` and fires `OnLevelStarted(levelData)`. EnemyFormation spawns/configures the grid; EnemyFireController applies the fire config.
4. **Level cleared**: LevelManager subscribes to `EnemyFormation.OnFormationCleared`. On that event it fires `OnLevelCleared(levelIndex)`. PowerUpSystem (offered reliably by level 3, D4), AudioManager, JuiceManager, and UIManager react.
5. **Advancing the run**: after a level is cleared (and any power-up choice resolved), the run advances to the next level. GameManager owns the level index and the win condition: clearing level 6 → run win. **Handshake**: `OnLevelCleared` → GameManager increments the level index (or ends the run as a win at level 6) → GameManager fires `OnLevelChanged` → LevelManager starts the next level. *(This requires GameManager to react to `OnLevelCleared` — an edge not yet in `architecture.md`; see Open Questions / GameManager flag.)*
6. **Power-up gate between levels (D4)**: the between-level beat (clear → power-up offer → choose → next level) is sequenced so the next `OnLevelStarted` does not fire until the power-up choice is resolved. LevelManager coordinates the timing with PowerUpSystem via `OnLevelCleared`/`OnPowerUpChosen`. *(Exact sequencing ownership — LevelManager vs PowerUpSystem vs GameManager — see Open Questions.)*
7. **Scaling source**: all HP/speed/fire-rate scaling is read from `LevelData`, never computed by multiplying hardcoded constants in script. The game-vision per-level table is the source for the asset values.
8. LevelManager uses no `Find`/`FindObjectOfType`; it reads the level index via the GameManager event, holds the `LevelData` asset list (Inspector), and communicates via the contract.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Loading` | `OnLevelChanged` / run start | `OnLevelStarted` fired + wave spawned | Selects `LevelData`, fires `OnLevelStarted` |
| `Playing` | Wave spawned | `OnFormationCleared` | Level is live; awaits formation clear |
| `Cleared` | `OnFormationCleared` | Power-up resolved → next `OnLevelChanged` (or run win) | Fires `OnLevelCleared`; gates the power-up beat |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| GameManager | Reads level index via `OnLevelChanged`; reports `OnLevelCleared` so GameManager advances the index / ends run at level 6. |
| EnemyFormation | Fires `OnLevelStarted(levelData)` → formation spawns/configures; listens to `OnFormationCleared`. |
| EnemyFireController | Fires `OnLevelStarted(levelData)` → fire controller applies fire config. |
| PowerUpSystem | Fires `OnLevelCleared` → power-up offer; the next level waits on the choice (`OnPowerUpChosen`). |
| UIManager / AudioManager / JuiceManager | React to `OnLevelStarted` / `OnLevelCleared` for HUD level display, cues, and clear payoff. |

---

## Formulas

LevelManager computes no formulas itself — it **reads** pre-authored per-level values from `LevelData`. The game-vision difficulty table defines those authored values (illustrative):

| Level | Enemy HP (base unit) | March speed | Bullet speed | Fire density | Pacing target |
|---|---|---|---|---|---|
| 1 | 1× | 1.0× | 1.0× | 1 shooter, aimed | ~60–90s |
| 2 | 1× | 1.15× | 1.1× | +1 shooter | ~75s |
| 3 | 2× (front row) | 1.3× | 1.25× | short bursts | ~90s (power-up guaranteed) |
| 4 | 2× | 1.5× | 1.4× | overlapping streams | ~90s |
| 5 | 3× | 1.7× | 1.6× | dense + spread | ~100s |
| 6 | 4× (mini-boss formation) | 2.0× | 1.8× | peak density | ~120s (win on clear) |

> These are the authored target values for the six `LevelData` assets, not runtime formulas.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Formation cleared | Fire `OnLevelCleared` once; gate power-up; then advance | Single clean clear → progression. |
| Level 6 cleared | Run win (via GameManager) — do not start level 7 | 6 fixed levels (D5/N7). |
| Player dies (run lost) during a level | Level progression halts; GameManager ends run; no `OnLevelCleared` | Death pre-empts clear. |
| Power-up choice pending | Next `OnLevelStarted` is withheld until `OnPowerUpChosen` | The between-level breather must resolve first (D4). |
| `LevelData` missing for an index | Log a loud error; do not silently spawn an empty/garbage wave | Config bug surfaced, not hidden. |
| Restart mid-run | GameManager resets index to 1; LevelManager starts level 1 fresh | Pure run reset (D3). |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| GameManager | This depends on it | State trigger — reads `OnLevelChanged`; reports `OnLevelCleared` |
| EnemyFormation | It depends on this | Data dependency + state trigger — `OnLevelStarted` config; `OnFormationCleared` back |
| EnemyFireController | It depends on this | Data dependency — `OnLevelStarted` fire config |
| PowerUpSystem | It depends on this | State trigger — `OnLevelCleared` triggers the offer |
| UIManager / AudioManager / JuiceManager | It depends on this | State trigger — level start/clear cues |

> Nature options: `Data dependency` · `State trigger` · `Rule dependency` · `Ownership handoff`

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| Per-level `LevelData` (HP/speed/fire/layout) | game-vision table | — | Harder level | Easier level |
| `TotalLevels` | 6 (fixed) | 6 | n/a (D5) | n/a (D5) |
| Pacing targets (per level) | 60–120s | — | Longer levels | Shorter |
| Power-up-guaranteed-by level | 3 (D4) | 1–3 | Later guaranteed upgrade | Earlier |

> Every difficulty value is a `LevelData` field — this is the **primary Phase 3 difficulty-tuning surface**. `TotalLevels` is fixed by design.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Level start | HUD level indicator updates; (optional level banner) | Level-start cue | Alpha |
| Level cleared | Clear payoff (Phase 3 juice); transition to power-up screen | Wave-clear sting | MVP signal / Polish payoff |
| Final level (6) cleared | Victory payoff | Victory fanfare | Alpha |

> LevelManager fires the events; UIManager/AudioManager/JuiceManager own the presentation.

---

## Game Feel

### Feel Reference

> "Progression like a *classic arcade loop* — each stage a clear, readable step harder. The between-level power-up is the *Slay the Spire* / roguelike breather-and-build beat. NOT a flat difficulty that never escalates."

### Input Responsiveness

Not directly player-driven; the felt responsiveness is the immediacy of the clear→reward→next-level beat (no dead air beyond the intended breather).

### Animation Feel Targets

| Animation | Startup | Active | Recovery | Feel Goal |
|---|---|---|---|---|
| Level transition | brief | power-up screen | next wave spawn | A satisfying beat, not a long load |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Level clear | — | Explosive payoff + transition into the power-up choice (Phase 3 juice) |
| Run win (L6 clear) | — | Biggest payoff in the game |

### Weight and Responsiveness

- **Weight**: The curve carries weight — each level should *feel* heavier than the last.
- **Player control**: Player controls pace by how fast they clear; the breather is a deliberate, controlled beat.
- **Snap quality**: Clear → reward → next should be crisp, with only the intended power-up pause.
- **Failure texture**: Difficulty spikes must feel earned/telegraphed (the curve is steep but readable), so losing at L5–6 invites another run.

### Feel Acceptance Criteria

- [ ] Each level is perceptibly harder than the last (playtesters feel the ramp).
- [ ] The between-level power-up beat feels like a reward/breather, not an interruption.
- [ ] A full clear lands around 10–15 minutes (vision pacing).

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Current level (1–6) | HUD (UIManager) | On `OnLevelStarted`/`OnLevelChanged` | Always during run |
| Level cleared / transition | Center screen (UIManager) | On `OnLevelCleared` | Between levels |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Level index + win + advance | `GameManager.md` | `OnLevelChanged`, level index, run-win, `OnLevelCleared` handshake | State trigger |
| Wave spawn config | `EnemyFormation.md` | `OnLevelStarted(levelData)`, `OnFormationCleared` | Data dependency / state trigger |
| Fire config | `EnemyFireController.md` | `OnLevelStarted` fire fields | Data dependency |
| Power-up offer gate | `PowerUpSystem.md` | `OnLevelCleared`, `OnPowerUpChosen` | State trigger |
| Curve values | `game-vision.md` | difficulty curve table | Data dependency |

---

## Acceptance Criteria

- [ ] Six `LevelData` ScriptableObjects exist, authored to the game-vision curve; no level values hardcoded.
- [ ] On level start, `OnLevelStarted(levelData)` fires and the wave spawns/configures from it.
- [ ] `EnemyFormation.OnFormationCleared` triggers `OnLevelCleared`; the power-up offer gates the next level.
- [ ] The run advances level-by-level; clearing level 6 results in a run win (no level 7).
- [ ] The next level does not start until the power-up choice is resolved.
- [ ] Missing `LevelData` logs a loud error rather than spawning garbage.
- [ ] No `Find`/`FindObjectOfType`; communicates only via the contract.
- [ ] Performance: level setup is a one-time cost per level; no per-frame allocation.

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| **Architecture-contract flag**: the clear→advance handshake needs a `LevelManager.OnLevelCleared` → GameManager edge (GameManager must react to advance the index / win at L6). `architecture.md` has GameManager listening only to `PlayerShip.OnPlayerDeath`. **Add this edge to `architecture.md` before coding.** | designer / Claude | Before GameManager + LevelManager implementation | **Pending — same flag as GameManager GDD.** Leaning: GameManager subscribes to `LevelManager.OnLevelCleared`. |
| Who owns the between-level sequencing (clear → offer → choose → next): LevelManager, PowerUpSystem, or GameManager? | designer / Claude | Before PowerUpSystem (Tier 3) | Leaning: LevelManager coordinates timing; PowerUpSystem owns the offer/choice; GameManager owns the index advance. Finalize with PowerUpSystem. |
| Level 6 "mini-boss formation" — authored purely as a high-HP `LevelData` wave, or special-cased? | designer / Claude | Before implementation | Leaning: authored as a high-HP/HP-tier `LevelData` wave (no separate boss system; none exists in systems-design). |
| Is there an inter-level transition delay/banner, or instant next-wave after power-up? | designer | Phase 3 | Pending — MVP can be near-instant; banner is polish. |
