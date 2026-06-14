# Projectile

> **Status**: Draft
> **Last Updated**: 2026-06-13
> **Implements Pillar**: Chaotic + Explosive — hundreds of bullets fill the screen; player shots make enemies pop.

## Summary

Projectile is the bullet — both player shots and enemy fire. It moves in a straight line at a set speed, expires after a lifetime or when it leaves the playfield, and on collision deals damage to the opposing team by calling `Enemy.TakeDamage` (player bullets) or `PlayerShip.TakeHit` (enemy bullets). Every projectile is pooled; none are `Instantiate`/`Destroy`-d during play.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `None` (spawned by PlayerShip + EnemyFireController; calls Enemy/PlayerShip)

---

## Overview

Both sides shoot in this game, and at level-4+ density there can be hundreds of live bullets. Projectile is the single shared bullet behaviour: it knows its team (player or enemy), its direction and speed, and how much damage it deals. It travels until it hits a valid target, exceeds its lifetime, or leaves the screen — then returns to its pool. It is deliberately "dumb": it carries data and resolves one collision; it does not decide who fires, how often, or what pattern. That keeps the hottest object in the game allocation-free and trivially poolable.

## Player Fantasy

Player bullets should feel like they *connect* — a clean line from muzzle to the enemy that pops. Enemy bullets should be readable as threats the player can track and weave through (Chaotic but fair). The projectile itself is the visual language of the bullet-hell: clear, fast, unambiguous.

---

## Detailed Design

### Core Rules

1. **Pooled (hard rule)**: projectiles are acquired from and released to an object pool. No `Instantiate`/`Destroy` and no per-frame allocation during play (`best-practices.md` zero-GC). Pools are pre-warmed.
2. **Team**: each projectile has a `Team` (`Player` or `Enemy`), set at spawn by its spawner (PlayerShip → Player; EnemyFireController → Enemy). A projectile only damages the opposing team.
3. **Movement**: each frame the projectile moves along its `Direction` at `Speed` (player bullets up, enemy bullets down or along a pattern vector). Movement uses `Time.deltaTime` (gameplay-paced; freezes correctly during pause/hit-stop since timeScale scales it).
4. **Lifetime / despawn**: a projectile returns to the pool when any of these occur:
   - It travels off the playfield bounds (top for player, bottom/sides for enemy).
   - Its `Lifetime` timer elapses (safety net so no bullet leaks).
   - It resolves a damaging collision (single-hit; see rule 6).
5. **Collision detection**: uses 2D physics triggers/layers. Player bullets collide only with enemies; enemy bullets collide only with the player. Layer/collision matrix is configured so opposing-team and same-team collisions are filtered cheaply (no manual team check needed at runtime where the matrix handles it; the `Team` field is the authoritative fallback).
6. **Damage dealing (the only "downward" direct call, per architecture)**:
   - Player projectile hits an Enemy → calls `Enemy.TakeDamage(Damage)`, then despawns.
   - Enemy projectile hits the PlayerShip → calls `PlayerShip.TakeHit()`, then despawns.
   - A projectile resolves **one** collision then returns to pool (no pierce in v1; pierce would be a power-up extension — see Open Questions).
7. **No events**: Projectile fires no events (per architecture — it is a leaf that calls methods on what it hits). Reactions to a kill/hit are fired by Enemy/PlayerShip, not by the bullet.
8. **Reset on acquire**: when pulled from the pool, all state (position, direction, speed, damage, lifetime timer, team, active collider) is fully re-initialized so no stale state leaks between uses.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Pooled` (inactive) | Released to pool | Acquired by a spawner | Disabled, no update, no collider |
| `Active` | Acquired + initialized | Off-screen / lifetime out / collision | Moves each frame; collider live; can damage opposing team |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| PlayerShip | Spawns player projectiles (sets Team=Player, dir=up, speed, damage); receives `TakeHit()` from enemy projectiles. |
| EnemyFireController | Spawns enemy projectiles (sets Team=Enemy, dir/pattern, speed). |
| Enemy | Receives `TakeDamage(damage)` from player projectiles. |
| Object Pool | Acquire on spawn, release on despawn. |

---

## Formulas

### Linear movement

```
position += Direction.normalized * Speed * Time.deltaTime
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `Direction` | Vector2 | unit vector | spawner | Up for player; down/pattern for enemy |
| `Speed` | float | 6–25 units/s | spawner / `LevelData` | Bullet speed (enemy speed scales per level, 1.0×→1.8×, game-vision) |
| `Damage` | int | 1+ | spawner / `PlayerData` | Damage dealt on hit (player bullet base 1; power-ups may raise) |

**Expected output range**: position advances smoothly; despawns at bounds.
**Edge cases**: `Direction` must be normalized; zero-length direction is invalid (guard at spawn).

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Bullet leaves the screen | Released to pool immediately | No leaked/forever-travelling bullets; pool stays healthy. |
| Two collisions in one physics step | Resolve the first valid hit, despawn; ignore the rest | Single-hit rule; avoids double-damage from one bullet. |
| Player bullet overlaps another player bullet / enemy bullet overlaps enemy | No collision (filtered by layer matrix + Team) | Friendly fire and bullet-on-bullet are off in v1. |
| Pool exhausted at peak density | Pool grows (or oldest recycled) per pool policy; never `Instantiate` mid-frame if avoidable | Maintains zero-GC; sizing validated in performance pass. |
| Collision while target already dead/despawned | No-op; bullet still despawns | Target may have died same frame from another bullet. |
| Hit-stop (timeScale 0) | Bullet freezes mid-flight (uses scaled deltaTime) and resumes | Hit-stop should visibly freeze bullets for impact. |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| PlayerShip | It depends on this (spawns) / This calls it | Ownership handoff + `TakeHit` call |
| EnemyFireController | It depends on this (spawns) | Ownership handoff |
| Enemy | This depends on it | Rule dependency — calls `TakeDamage` |
| Object Pool | This depends on it | Ownership handoff — acquire/release |

> Nature options: `Data dependency` · `State trigger` · `Rule dependency` · `Ownership handoff`

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| Player `Speed` | 18 | 10–25 | Snappier shots, easier to land | Slower, more dodgeable by enemies (n/a) |
| Enemy `Speed` (base) | 7 | 4–14 | Harder to dodge; raises difficulty | Easier to weave |
| Player `Damage` | 1 | 1–5 | Kills tankier enemies faster | Slower kills |
| `Lifetime` | 3 s | 1–6 | Bullets persist longer off-screen safety | Shorter safety net |
| Pool prewarm size | sized in perf pass | — | More headroom, more memory | Less memory, risk of runtime grow |

> Enemy bullet speed is driven per-level by `LevelData` (game-vision threat-speed ramp). Player projectile values come from `PlayerData`/power-ups. No magic numbers in script.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Player bullet in flight | Bright tracer sprite | (fire SFX on spawn, owned by PlayerShip) | MVP |
| Enemy bullet in flight | Distinct color/shape from player bullets (readability) | — | MVP |
| Bullet impact | Small hit spark (Phase 3 particles) | Impact tick (Phase 3) | Polish |

> Player vs enemy bullets must be visually distinct at a glance — critical for bullet-hell readability (Fun pillar).

---

## Game Feel

### Feel Reference

> "Player bullets like *Nuclear Throne* — fast, bright, immediate connection. Enemy bullets like *Ikaruga* — large, readable, fair to track even at high density. NOT tiny hard-to-see dots that feel like unfair hits."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Spawn → bullet visibly moving | ≤ 1 frame after spawn | 1 frame |

### Animation Feel Targets

Not applicable — bullets are simple moving sprites (optional spin/trail is polish).

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Bullet hits enemy | instant | Despawn + enemy hit reaction (owned by Enemy/Juice) |
| Bullet hits player | instant | `TakeHit` → hit-stop/shake owned by PlayerShip/Juice |

### Weight and Responsiveness

- **Weight**: Weightless and fast — bullets are pure threat/feedback, no momentum.
- **Player control**: None directly (fired-and-forget); the *firing* is controlled by PlayerShip.
- **Snap quality**: Binary — a bullet hits or misses; one collision, one resolution.
- **Failure texture**: Enemy bullets must be readable enough that being hit feels like a misread, not a cheap shot.

### Feel Acceptance Criteria

- [ ] Player and enemy bullets are instantly distinguishable.
- [ ] At peak density (level 6) bullets stay readable and frame rate holds (perf pass).

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| (none) | — | — | Projectile shows no UI |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Player bullet spawn + `TakeHit` | `PlayerShip.md` | spawn params, `TakeHit()` | Ownership handoff |
| Enemy bullet spawn + speed scaling | `EnemyFireController.md` / `LevelManager.md` | spawn, per-level bullet speed | Ownership handoff / data dependency |
| Damage application | `Enemy.md` | `TakeDamage(damage)` | Rule dependency |
| Zero-GC pooling | `best-practices.md` | pool-everything-hot rule | Rule dependency |

---

## Acceptance Criteria

- [ ] All projectiles are pooled; zero `Instantiate`/`Destroy` and zero GC alloc during sustained fire.
- [ ] Player bullets damage only enemies; enemy bullets damage only the player (layer matrix + Team).
- [ ] A bullet resolves exactly one collision, then despawns (no double-damage, no pierce in v1).
- [ ] Bullets despawn off-screen and on lifetime expiry — no leaks at peak density.
- [ ] State fully resets on pool acquire (no stale team/damage/timer).
- [ ] Enemy bullet speed scales per level from `LevelData`.
- [ ] Performance: hundreds of simultaneous projectiles hold frame budget (8.3 ms @120fps) with zero steady-state GC.
- [ ] No hardcoded speed/damage in script — sourced from SO/spawner.

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Do enemy bullets travel straight down only, or along pattern vectors (spread/aimed)? | designer | Defined in `EnemyFireController.md` | Projectile supports an arbitrary `Direction`; pattern logic lives in EnemyFireController. |
| Pierce / multi-hit as a power-up — does Projectile need a `pierceCount`, or is that a v2 concern? | designer | Phase 3 / v2 | Leaning: single-hit for v1; reserve a `Hits` field if a pierce power-up is added. |
| Bullet-on-bullet cancellation (shoot down enemy bullets) — in scope? | designer | v1 decision | Leaning: out for v1 (off via layer matrix); could be a power-up later. |
| Pool prewarm sizes per bullet type. | Claude | Performance pass | Pending — sized empirically at level-6 density in the Phase 3 perf pass. |
