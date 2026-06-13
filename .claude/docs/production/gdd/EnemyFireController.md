# EnemyFireController

> **Status**: Draft
> **Last Updated**: 2026-06-13
> **Implements Pillar**: Chaotic — the escalating bullet-hell density that crosses into "controlled overwhelm" by level 4.

## Summary

EnemyFireController is the brain behind enemy fire: it picks which living invaders shoot, decides the firing pattern and cadence, and spawns enemy `Projectile`s — scaling shooter count, fire rate, and pattern complexity per level so density ramps from sparse aimed shots (L1) to dense bullet-hell (L6). It reads the live shooter set from EnemyFormation and reads its per-level config from LevelManager; it owns the *when/where/how-many* of enemy fire.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `EnemyFormation`, `Projectile`, `LevelManager`

---

## Overview

The invaders need to shoot back, and the *amount* and *pattern* of that fire is the single biggest difficulty lever in the game (game-vision: fire-rate/density ramp is the genre's "threat speed"). EnemyFireController centralizes that: on a timer, it selects one or more living enemies (typically front-of-column shooters), spawns bullets from them in the level's pattern, and repeats — with more simultaneous shooters and denser patterns each level. Individual enemies don't decide to fire; the controller does, so density is tuned in one place per level.

## Player Fantasy

Pressure that builds. Early levels let the player learn to read single aimed shots; later levels fill the screen with overlapping streams that demand a built-up loadout and constant movement. The fantasy: the player is *always reacting*, threading gaps that get tighter every level — frantic but fair.

---

## Detailed Design

### Core Rules

1. **Config per level**: on `LevelManager.OnLevelStarted`, EnemyFireController reads the level's fire config: number of simultaneous shooters, fire interval, pattern type(s), and bullet speed multiplier (game-vision table per level).
2. **Shooter selection**: on each fire tick, select up to `ActiveShooters` living enemies from EnemyFormation's live set. Default selection favors the front-most enemy per column (classic — only the bottom invader in a column can shoot), then scales to more columns/shooters at higher levels. Selection never picks a dead/pooled enemy.
3. **Fire patterns** (scaling by level, per game-vision):
   - L1: single aimed shot (toward player's current X) or straight down — sparse.
   - L2: +1 shooter — two streams.
   - L3: short bursts begin.
   - L4: overlapping streams (true bullet-hell threshold).
   - L5: dense patterns + spread (multi-direction).
   - L6: peak density.
   The pattern set is data-driven from the level config; the controller maps a pattern type to bullet `Direction`(s) and timing.
4. **Spawning**: bullets are enemy `Projectile`s acquired from the pool (zero-GC, `best-practices.md`), spawned at the selected enemy's muzzle, with Team=Enemy, the pattern's direction, and the level's bullet speed.
5. **Cadence / timing**: a fire-interval timer drives ticks. Burst patterns fire N bullets at a sub-interval. Timing should use a pause/hit-stop-safe approach consistent with the project rule — gameplay timers honoring timeScale so fire pauses with the game (i-frame/power-up timers are the ones mandated unscaled; enemy fire cadence pauses with gameplay). *(Confirm timer basis at implementation — see Open Questions.)*
6. **No events**: EnemyFireController fires no C# events (per architecture). It reads state and spawns bullets.
7. **Aim**: aimed patterns target the player's current X position. The controller may read the player position via a reference passed at setup (not via `Find`) — or aim straight down for non-aimed patterns. *(Aiming reference path — see Open Questions; must respect the no-Find rule.)*
8. **Density safety**: total live enemy bullets are bounded by pool sizing; the controller respects the pool (never forces `Instantiate` mid-frame if the pool policy is fixed-size).
9. EnemyFireController uses no `Find`/`FindObjectOfType`; EnemyFormation and Projectile pool references are assigned per the contract; level config arrives via `OnLevelStarted`.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Idle` | Before a level / formation cleared | `OnLevelStarted` | No firing |
| `Firing` | `OnLevelStarted` received, enemies alive | Formation cleared / level ends | Runs fire-interval ticks, selects shooters, spawns patterned bullets |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| LevelManager | Listens to `OnLevelStarted` for per-level fire config (shooter count, interval, patterns, bullet speed). |
| EnemyFormation | Reads the live shooter candidate set (front-of-column enemies) and their muzzle positions. |
| Projectile | Spawns enemy bullets from the pool (Team=Enemy, direction, speed). |
| PlayerShip | (Read-only, indirect) aimed patterns target the player's X position via a passed reference. |

---

## Formulas

### Fire density (per level)

```
shotsPerSecond ≈ ActiveShooters / FireInterval   (+ burst multiplier)
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `ActiveShooters` | int | 1 (L1) → many (L6) | `LevelData` | Simultaneous shooters |
| `FireInterval` | float | ~1.5s (L1) → ~0.4s (L6) | `LevelData` | Seconds between fire ticks |
| `burstCount` | int | 1 (L1) → N (L3+) | `LevelData` | Shots per burst |
| `bulletSpeedMult` | float | 1.0× (L1) → 1.8× (L6) | `LevelData` | Enemy bullet speed scale |

**Expected output range**: sparse single shots → dense overlapping streams across L1→L6.
**Edge cases**: clamp `ActiveShooters` to living enemy count; if no enemies alive, fire nothing.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Fewer living enemies than `ActiveShooters` | Fire from all living enemies, no error | Late in a wave the count shrinks. |
| Selected shooter dies between selection and spawn | Skip it; spawn from a still-living shooter or skip the slot | Never spawn a bullet from a pooled enemy. |
| Formation cleared mid-burst | Stop firing immediately | No bullets after the wave is done. |
| Bullet pool exhausted at peak density | Respect pool policy (skip or grow per config); never spike GC | Maintains zero steady-state GC. |
| Hit-stop / pause | Fire cadence pauses with gameplay | Enemy fire shouldn't tick during a freeze. |
| Aimed pattern with player dead/disabled | Fall back to straight-down (no valid aim target) | Avoids aiming at a null/disabled ship. |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| LevelManager | This depends on it | Data dependency — `OnLevelStarted` fire config |
| EnemyFormation | This depends on it | Data dependency — live shooter set + muzzle positions |
| Projectile | This depends on it | Ownership handoff — spawns enemy bullets |
| PlayerShip | This depends on it (read-only) | Data dependency — player X for aimed patterns |

> Nature options: `Data dependency` · `State trigger` · `Rule dependency` · `Ownership handoff`

---

## Tuning Knobs

| Parameter | Default (per level) | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `ActiveShooters` | L1: 1 → L6: many | 1–~12 | Denser fire, harder | Sparser, easier |
| `FireInterval` | L1: ~1.5s → L6: ~0.4s | 0.2–2.0s | Slower fire (easier) | Faster fire (harder) |
| `burstCount` | L1: 1 → L3+: N | 1–6 | Burstier pressure spikes | Steadier fire |
| `bulletSpeedMult` | 1.0×→1.8× | 0.8–2.2 | Harder to dodge | Easier to weave |
| Pattern type per level | aimed→spread | enum set | More complex/chaotic | Simpler/readable |

> All per-level fire values live in `LevelData` ScriptableObjects — this is the **primary difficulty-tuning surface** for the Phase 3 difficulty pass (game-vision fire-rate/density ramp). No hardcoded cadence.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Enemy fires | Muzzle flash on shooter + bullet appears | Enemy shoot SFX (sparse so it doesn't muddy at density) | Alpha |
| Dense pattern (L4+) | Readable bullet spacing/coloring | (mix-managed so it stays readable) | MVP readability |

> At high density, audio must not become noise — AudioManager should throttle/round-robin enemy shoot SFX. Bullet readability is owned by Projectile visuals.

---

## Game Feel

### Feel Reference

> "Escalation like *Ikaruga* / *Galaga* — fire density that ramps from learnable to frantic, always with readable gaps to thread. NOT random spam with no dodgeable lanes."

### Input Responsiveness

Not player-driven directly. The feel goal is that patterns are *readable* — the player can always find a gap, so death feels like a misread, not RNG.

### Animation Feel Targets

| Animation | Startup | Active | Recovery | Feel Goal |
|---|---|---|---|---|
| Shooter fire tell | brief | shot | — | A small tell so dense fire feels fair to read |

### Impact Moments

None directly — the controller's "impact" is cumulative pressure, not a single moment.

### Weight and Responsiveness

- **Weight**: Builds across the level — light early, oppressive late.
- **Player control**: Indirect — the player manages density by killing shooters (fewer columns → fewer shots).
- **Snap quality**: Pattern timing should feel rhythmic/readable, not stuttery.
- **Failure texture**: Every death should trace to a thread-able gap the player missed — fairness is paramount given the density.

### Feel Acceptance Criteria

- [ ] Density visibly ramps each level; L4 reads as the "chaotic threshold."
- [ ] Playtesters can identify dodgeable gaps even at L5–6 (deaths feel fair).

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| (none) | — | — | No UI |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Per-level fire config | `LevelManager.md` | `OnLevelStarted` payload (shooters/interval/patterns/speed) | Data dependency |
| Shooter set | `EnemyFormation.md` | live front-of-column enemies + muzzles | Data dependency |
| Enemy bullets | `Projectile.md` | enemy bullet spawn (Team, dir, speed) | Ownership handoff |
| Aim target | `PlayerShip.md` | player X position | Data dependency |
| Density ramp values | `best-practices.md` | LevelData SO rule | Rule dependency |

---

## Acceptance Criteria

- [ ] On `OnLevelStarted`, fire config (shooters, interval, patterns, bullet speed) is applied from `LevelData`.
- [ ] Only living enemies are selected as shooters; never a pooled/dead enemy.
- [ ] Fire density, pattern complexity, and bullet speed visibly escalate L1→L6 per the game-vision table.
- [ ] Enemy bullets are pooled (Team=Enemy); no `Instantiate`/`Destroy`; zero steady-state GC at peak density.
- [ ] Firing stops immediately when the formation is cleared.
- [ ] Aimed patterns target player X; fall back to straight-down if no valid target.
- [ ] No `Find`/`FindObjectOfType`; references resolved per the contract.
- [ ] Performance: shooter selection + spawn loop is allocation-free, within 8.3 ms @120fps at L6 density.
- [ ] No hardcoded cadence/pattern values — all from `LevelData`.

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| How does the controller get the player-X aim reference without `Find`? (Inspector ref into GameLogic scene, or passed at level start, or via GameManager.) | Claude | Before implementation | Leaning: reference passed at level setup (GameLogic scene wiring); must not use `Find`. |
| Exact pattern catalog (aimed / straight / spread / burst) and which levels use which — needs the full `LevelData` schema. | designer / Claude | Defined alongside `LevelManager.md` `LevelData` | Pending — finalize the LevelData fire schema with LevelManager. |
| Fire-cadence timer basis: scaled (pauses with gameplay/hit-stop) confirmed? | Claude | Before implementation | Leaning: scaled deltaTime so fire pauses during hit-stop/pause (only i-frame/power-up timers are mandated unscaled). |
| Classic "only bottom enemy in a column can fire" restriction, or any living enemy? | designer | Before implementation | Leaning: front-of-column for the classic read at low levels; relax at high levels for density. |
