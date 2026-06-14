# Enemy

> **Status**: Draft
> **Last Updated**: 2026-06-13
> **Implements Pillar**: Explosive — each invader is a thing that pops; killing it is the core dopamine beat.

## Summary

Enemy is a single invader: it holds hit points (tiered so front rows are tankier), takes damage from player projectiles, and on death awards points and fires `OnEnemyKilled` — the one event that many systems react to (formation count, score, audio, juice). It does not move itself (EnemyFormation moves it) or decide when to shoot (EnemyFireController) — it owns its own HP, death, and point value only.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `Projectile` (damages it); `EnemyFormation` (owns/moves it)

---

## Overview

The invader. Every one is a pooled object with an HP value and a point value set when it spawns (from the level config). Player bullets call `TakeDamage` on it; when HP hits 0 it dies — broadcasting `OnEnemyKilled` so the formation decrements its count, the score goes up, audio plays the pop, and (Phase 3) particles and shake fire. Enemies come in HP tiers within a wave (front row tankier, per the game-vision HP-shaping rule) to shape the kill order. The Enemy is deliberately minimal: HP in, death out.

## Player Fantasy

Killing an invader must feel *good* — instant, crunchy, rewarding. Tankier front-row enemies create a satisfying "break through the wall" moment when they finally pop. Every kill is a small explosive payoff that, multiplied across a formation, becomes the "fireworks finale."

---

## Detailed Design

### Core Rules

1. **HP**: each enemy has `MaxHealth` set at spawn from the level config (HP tier per row — front rows tankier, game-vision HP scaling: 1×→4× across levels, stepped at L3/L5/L6). Current HP starts at `MaxHealth`.
2. **Point value**: each enemy has a `PointValue` set at spawn (from config; may vary by row/type). On death it reports this value via `OnEnemyKilled` so ScoreSystem can award it.
3. **Taking damage**: a player projectile calls `TakeDamage(amount)` (direct call, the only downward call per architecture). Subtract `amount` from current HP. If HP > 0, play a non-fatal hit reaction (flash). If HP <= 0, die.
4. **Death**: on death, fire `OnEnemyKilled` (carrying point value and position for score/audio/juice), then release back to the pool. Death happens once — a dead/despawning enemy ignores further `TakeDamage`.
5. **No self-movement**: Enemy does not move itself. EnemyFormation moves the formation; the enemy holds its grid slot. (If a future "diving" enemy type is added, that's an extension — Open Questions.)
6. **No self-firing**: Enemy does not decide to shoot. EnemyFireController selects shooters and spawns their bullets. Enemy may expose its muzzle position for the fire controller.
7. **Pooling (hard rule)**: enemies are acquired/released via pool. On acquire, HP, point value, visuals, and collider are fully reset. No `Instantiate`/`Destroy` during play.
8. **HP tiers / visual**: an enemy's tier should be visually distinguishable (e.g. color/sprite per HP tier) so the player can read the kill order. Tier data comes from the level config.
9. Enemy uses no `Find`/`FindObjectOfType`; it fires `OnEnemyKilled` and is acted on via `TakeDamage`.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Pooled` | Released to pool | Acquired by EnemyFormation | Inactive, no collider, no update |
| `Alive` | Spawned + initialized | HP <= 0 | Holds grid slot; collider live; can take damage |
| `Dying` (transient) | HP hit 0 | Released to pool (same frame / after death VFX) | Fires `OnEnemyKilled`; ignores further damage |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| Projectile | Receives `TakeDamage(amount)` from player bullets (direct call on collision). |
| EnemyFormation | Owns/spawns/moves this enemy; listens to `OnEnemyKilled` to track remaining. Provides spawn config (HP tier, points). |
| ScoreSystem | Subscribes to `OnEnemyKilled` → awards `PointValue`. |
| AudioManager / JuiceManager | Subscribe to `OnEnemyKilled` → pop SFX, particles, shake (Phase 3). |
| EnemyFireController | Reads enemy position/muzzle when this enemy is selected to fire. |

---

## Formulas

### Damage application

```
currentHealth -= amount
isDead = (currentHealth <= 0)
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `amount` | int | 1+ | Projectile `Damage` | Damage from the player bullet |
| `currentHealth` | int | 0..MaxHealth | runtime | Remaining HP |
| `MaxHealth` | int | 1–4+ (per tier/level) | `LevelData` | HP tier (game-vision scaling) |
| `PointValue` | int | per type | `LevelData` | Score awarded on death |

**Expected output range**: `currentHealth` clamps at/below 0 for death.
**Edge cases**: overkill (amount > HP) still dies once; never awards points twice.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Two bullets hit the same frame, both would kill | Dies once; `OnEnemyKilled` fires once; points awarded once | No double-score / double-count. |
| `TakeDamage` called after death (already despawning) | No-op | Dead enemies don't re-die or re-award. |
| Overkill damage (amount ≫ HP) | Dies normally | HP just goes ≤ 0. |
| Enemy released while a bullet is mid-collision with it | Bullet's collision no-ops gracefully; enemy already pooled | Matches Projectile "target already dead" edge. |
| Level reset / run reset with enemies alive | All enemies released to pool, state reset | Clean wave handoff. |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| Projectile | This depends on it | Rule dependency — `TakeDamage` is called by player bullets |
| EnemyFormation | This depends on it | Ownership handoff — spawned/owned/moved; provides config |
| ScoreSystem | It depends on this | State trigger — `OnEnemyKilled` → points |
| AudioManager / JuiceManager | It depends on this | State trigger — `OnEnemyKilled` → feedback |
| EnemyFireController | It depends on this | Data dependency — reads position/muzzle to fire |

> Nature options: `Data dependency` · `State trigger` · `Rule dependency` · `Ownership handoff`

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `MaxHealth` (per tier/level) | L1 front: 1 | 1–6 | Tankier, slower to clear (harder) | Squishier, faster clear |
| `PointValue` (per type) | tier-based | — | Higher score reward | Lower |
| Tier→row mapping | front tankier | — | Shapes kill order | — |

> All values live in `LevelData` ScriptableObjects (difficulty in SOs — `best-practices.md`). HP tiers per row are the game-vision "front-row tankier" shaping rule.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Non-fatal hit | Flash / brief tint | Light hit tick | Alpha |
| Death | Pop / explosion (big in Phase 3 juice), particles | Punchy death SFX (per D6) | MVP signal / Polish payoff |
| HP tier | Distinct color/sprite per tier | — | Alpha |

> Death feedback is a first-class feature (D6) — the "every kill pops" promise. The Enemy fires the event; JuiceManager/AudioManager own the spectacle.

---

## Game Feel

### Feel Reference

> "Kills like *Nuclear Throne* / *Vampire Survivors* — instant, crunchy, no death animation lag. NOT a slow fade-out that delays the satisfaction."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Bullet connects → enemy reacts (hit/death) | ≤ 1 frame | 1 frame |

### Animation Feel Targets

| Animation | Startup | Active | Recovery | Feel Goal |
|---|---|---|---|---|
| Hit flash | 0 | 1–2 frames | 0 | Immediate "I hit it" |
| Death pop | 0 | brief | despawn | Snappy, satisfying |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Enemy death | instant + ~Phase 3 particles | Pop + particles + (formation-wide shake on big clears) |

### Weight and Responsiveness

- **Weight**: Light per enemy, but tankier tiers add a satisfying "resistance then break" weight.
- **Player control**: Player kills via aim/fire; the enemy reacts instantly.
- **Snap quality**: Binary — alive or dead; hit feedback is crisp.
- **Failure texture**: N/A (enemy side) — but readable HP tiers let the player plan, which feels fair.

### Feel Acceptance Criteria

- [ ] Kills feel instant and satisfying (no death-animation delay before the pop).
- [ ] HP tiers are readable at a glance so the player can prioritize.

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| (HP not shown numerically) | — | — | HP read via tier color, not a bar, in v1 |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Damage application | `Projectile.md` | `TakeDamage(amount)`, player `Damage` | Rule dependency |
| Spawn/ownership + count | `EnemyFormation.md` | spawn config, `OnEnemyKilled` listener | Ownership handoff |
| Scoring | `ScoreSystem.md` | `OnEnemyKilled` → `PointValue` | State trigger |
| Death spectacle | `AudioManager.md` / `JuiceManager.md` | `OnEnemyKilled` | State trigger |
| HP/points values | `best-practices.md` | LevelData SO rule | Rule dependency |

---

## Acceptance Criteria

- [ ] Enemy spawns with `MaxHealth` and `PointValue` from the level config (HP tier per row).
- [ ] `TakeDamage` reduces HP; non-fatal hits show feedback; HP ≤ 0 triggers death.
- [ ] Death fires `OnEnemyKilled` exactly once (point value + position) and releases to pool.
- [ ] Post-death `TakeDamage` is a no-op; no double-score or double-count even on same-frame multi-hit.
- [ ] HP tiers are visually distinguishable.
- [ ] Enemies are pooled with full state reset on acquire; no `Instantiate`/`Destroy` per wave.
- [ ] No `Find`/`FindObjectOfType`; no self-movement, no self-firing.
- [ ] Performance: a full grid of enemies is allocation-free in steady state, within 8.3 ms @120fps.
- [ ] No hardcoded HP/points — sourced from `LevelData`.

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Are there distinct enemy *types* (visual/behaviour), or one sprite with HP-tier coloring for v1? | designer | Before implementation | Leaning: one base invader with tier coloring for v1; types are content polish. |
| Does any enemy ever leave the formation (dive/charge), or is all movement strictly formation-bound in v1? | designer | Before implementation | Leaning: strictly formation-bound for v1 (preserves Space Invaders identity). |
| Level 6 "mini-boss formation" (game-vision) — is the boss a special Enemy with high HP, or a separate system? | designer / Claude | Before LevelManager finalize | Leaning: a high-HP Enemy tier within the formation, not a new system (keeps scope tight; note: no `MothershipBoss` system exists in systems-design). |
