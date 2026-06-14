# PlayerShip

> **Status**: Draft
> **Last Updated**: 2026-06-14
> **Implements Pillar**: Chaotic + Fun — the player's instrument for reacting to bullet-hell density; tight, fair, survivable.

## Summary

The player-ship system is split across four classes (architecture pass, 2026-06-14):

| Class | Type | Responsibility |
|---|---|---|
| `PlayerShipContext` | MonoBehaviour | Thin coordinator: lifecycle, event surface, wires subsystems |
| `PlayerShipState` | plain C# | Runtime conditions: i-frame timer (unscaled), invuln flag |
| `PlayerShipStat` | plain C# | Modifiable numeric stats: speed, cooldown, damage, multiShot + power-up `Apply()` |
| `Weapon` / `BasicWeapon` | abstract / concrete MonoBehaviour | Fire logic, projectile pool, spread patterns |

`PlayerShipContext` implements `IDamageable` so `Projectile` can call `TakeDamage()` without knowing the concrete type. `PowerUpSystem` is wired via Inspector (no `FindFirstObjectByType`).

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `InputManager`, `Projectile`, `GameManager`, `PowerUpSystem`

---

## Overview

This is the thing the player controls. Per D1/N1 the ship moves on the X axis only — left/right along a fixed bottom band — and fires straight up. It does not free-roam (that would stop being Space Invaders). The player has 3 lives (D2); each damaging hit costs a life and grants a brief i-frame window so dense bullet patterns can't chain-kill in a single frame. Between levels, power-ups can modify the ship's weapon and stats for the rest of the run (D3/D4, run-scoped). PlayerShip owns movement, firing, and damage-taking; it does *not* own the lives counter (that is GameManager's run state) — it just reports when it was hit.

## Player Fantasy

"Power fantasy on a timer." The ship should feel agile and precise under pressure — the player threads through bullets, and when a power-up lands they feel briefly unstoppable as their fire blooms. Death should feel fair: the i-frame flash tells them they were hit, the brief invulnerability gives a breath, and they know exactly why they died.

---

## Detailed Design

### Core Rules

1. **Movement (D1/N1)**: PlayerShip moves only on the X axis. It reads `InputManager.MoveAxis` (−1..+1) each frame and translates it to horizontal velocity. There is no Y movement, no dash, ever.
2. **Bounds**: the ship is clamped to a horizontal range (left/right screen edges, minus a margin). It can never leave the bottom strip vertically because it has no vertical movement.
3. **Firing**: when fire intent occurs (`InputManager.OnFirePressed` for tap, or `FireHeld` for held — see Open Questions), and the fire cooldown has elapsed, PlayerShip spawns a player `Projectile` at its muzzle, travelling straight up. Fire rate is gated by `FireCooldown`.
4. **Projectile spawning**: player bullets are acquired from a pool (no `Instantiate` per shot — `best-practices.md` zero-GC). PlayerShip sets the bullet's direction (up), speed, and "player" team tag, then releases it to the pool on expiry/collision (Projectile owns its own lifetime).
5. **Taking a hit**: an enemy `Projectile` collision calls `PlayerShipContext.TakeDamage(damage)` via the `IDamageable` interface. On `TakeDamage`:
   - If currently **invulnerable** (i-frames active): the hit deals **zero** damage and costs **no life** (I-frame invariant, D2/`best-practices.md`). Return immediately.
   - If **vulnerable**: fire `OnPlayerHit` (feedback hook — flash/SFX/shake), fire `OnPlayerDeath` (state hook — GameManager decrements one life), then start the i-frame window.
6. **I-frames (D2)**: after a vulnerable hit, the ship is invulnerable for `InvulnDuration`. The i-frame timer uses **`Time.unscaledDeltaTime`** so it does not stall during hit-stop (`Time.timeScale = 0`) — hard rule from `best-practices.md`. During i-frames the ship blinks/flashes for readability and ignores all incoming damage.
7. **Death vs run-end**: PlayerShip does not track lives. It reports each life-losing hit via `OnPlayerDeath`; GameManager owns the lives count and ends the run when lives reach 0. After a non-final hit, the ship continues (still controllable, now flashing during i-frames). *(Whether the ship visually "respawns"/recenters on hit vs stays in place — see Open Questions.)*
8. **Power-ups (D3/D4, run-scoped)**: PlayerShip subscribes to `PowerUpSystem.OnPowerUpChosen` and applies the chosen modifier to its weapon/stats (e.g. fire rate, multi-shot, projectile speed). Effects last the rest of the run and reset on a new run. The exact power-up catalog is defined in `PowerUpSystem.md` (Tier 3) — PlayerShip exposes the stat surface those effects mutate.
9. **Timers**: i-frame and any power-up-duration timers use `unscaledDeltaTime`/`WaitForSecondsRealtime` (hard rule). The fire-cooldown timer may use scaled time so firing pauses with the game.
10. `PlayerShipContext` uses no `Find`/`FindObjectOfType`; `InputManager` is resolved via `GameManager.Instance.Input`; `PowerUpSystem` is assigned in the Inspector.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Active` | Run started / after i-frames expire | Vulnerable hit taken | Reads input, moves, fires, can take damage |
| `Invulnerable` | A vulnerable hit was taken | `InvulnDuration` elapsed (unscaled) | Reads input, moves, fires; ignores all damage; visibly flashing |
| `Disabled` | Run ended (lives = 0) or not in gameplay | Run (re)starts | No input read, no fire, no collision response |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| InputManager | Reads `MoveAxis`/`FireHeld`; subscribes to `OnFirePressed`. Intent in; nothing out. |
| Projectile | Spawns player bullets (direct spawn from pool); receives `TakeHit()` from enemy projectiles (direct call on collision). |
| GameManager | Fires `OnPlayerDeath` → GameManager decrements lives and may end the run. Reads no GameManager state directly. |
| PowerUpSystem | Subscribes to `OnPowerUpChosen`; applies run-scoped stat/weapon modifiers. |
| AudioManager / JuiceManager | Subscribe to `OnPlayerHit` (and `OnPlayerDeath`) for SFX, flash, shake, hit-stop. |

---

## Formulas

### Horizontal movement

```
position.x += MoveAxis * MoveSpeed * Time.deltaTime
position.x = clamp(position.x, leftBound, rightBound)
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `MoveAxis` | float | −1..+1 | InputManager | Horizontal intent |
| `MoveSpeed` | float | 5–15 | Inspector / ScriptableObject | Units/sec at full deflection |
| `leftBound`/`rightBound` | float | scene-derived | Inspector | Playfield X clamp |

**Expected output range**: position.x stays within `[leftBound, rightBound]`.
**Edge cases**: clamp prevents leaving the playfield; opposing input → MoveAxis 0 → no movement.

### Fire gating

```
canFire = (timeSinceLastShot >= FireCooldown)
FireCooldown = BaseFireCooldown / fireRateMultiplier   // power-ups raise the multiplier
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `BaseFireCooldown` | float | 0.1–0.6 s | Inspector / SO | Base seconds between shots |
| `fireRateMultiplier` | float | 1.0–3.0 | PowerUpSystem | Run-scoped fire-rate buff |

**Expected output range**: effective cooldown 0.05–0.6 s.
**Edge cases**: clamp effective cooldown to a sane minimum so power-ups can't drive it to 0.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Hit lands during i-frames | Zero damage, no life lost, no `OnPlayerDeath` | I-frame invariant (D2). |
| Two enemy bullets hit in the same frame while vulnerable | Only the first costs a life; the second lands inside the just-started i-frame window and is ignored | Prevents single-frame chain-death — the whole point of i-frames. |
| Last life lost | Fire `OnPlayerHit` + `OnPlayerDeath`; GameManager ends run; PlayerShip → `Disabled` | Clean run-end; no further input/fire. |
| Fire pressed during cooldown | No bullet spawned | Fire-rate gating. |
| Ship at exact left/right bound, pushing further | Stays clamped, no jitter | Bounds clamp. |
| Power-up applied mid-i-frame | Stat change applies immediately; does not cancel i-frames | Power-ups modify stats, not damage state. |
| Hit-stop active (timeScale 0) when i-frames running | I-frame timer keeps counting (unscaled) and expires correctly | Hard timer rule — avoids stuck-invulnerable or stuck-vulnerable bug. |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| InputManager | This depends on it | Data dependency — reads move/fire intent |
| Projectile | This depends on it | Ownership handoff — spawns player bullets; receives `TakeHit` |
| GameManager | It depends on this | State trigger — `OnPlayerDeath` decrements lives |
| PowerUpSystem | This depends on it | Rule dependency — applies run-scoped modifiers via `OnPowerUpChosen` |
| AudioManager / JuiceManager | It depends on this | State trigger — react to `OnPlayerHit`/`OnPlayerDeath` |

> Nature options: `Data dependency` · `State trigger` · `Rule dependency` · `Ownership handoff`

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `MoveSpeed` | 9.0 | 5–15 | Faster, more agile, twitchier | Slower, more sluggish |
| `BaseFireCooldown` | 0.25 s | 0.1–0.6 | Slower fire | Faster fire (denser player bullets) |
| `InvulnDuration` | 1.0 s | 0.5–2.0 | More forgiving; easier curve (D2 knob) | Less forgiving; more chain-death risk |
| `leftBound`/`rightBound` | scene-derived | — | Wider play area | Narrower |
| Muzzle/projectile speed | (see Projectile) | — | — | — |

> All exposed via Inspector or a `PlayerShipData`/`PlayerData` ScriptableObject (difficulty/feel values live in SOs — `best-practices.md`). `InvulnDuration` and `MoveSpeed` are primary Phase 3 feel knobs.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Fire | Muzzle flash | Shoot SFX | MVP |
| Move | (thruster wisp — polish) | — | Polish |
| Hit taken (vulnerable) | Flash + start blink during i-frames; (screen shake/hit-stop in Phase 3) | Hit/explosion SFX | MVP |
| I-frame active | Ship blinks/translucent (readability cue) | — | MVP |
| Final death | Ship explosion (big in Phase 3 juice) | Death SFX | MVP |
| Power-up applied | Ship buff VFX (glow/aura) | Power-up SFX | Alpha |

---

## Game Feel

### Feel Reference

> "Movement like *Galaga*'s lateral glide — instant and precise. Firing like *Nuclear Throne*'s weapons — punchy, immediate, satisfying. NOT a momentum-heavy ship that slides past where you aimed."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Move input → ship moves | ≤ 1 frame | 1 frame |
| Fire input → bullet visible | ≤ 2 frames | 2 frames |

### Animation Feel Targets

| Animation | Startup Frames | Active Frames | Recovery Frames | Feel Goal |
|---|---|---|---|---|
| Fire | 0–1 | bullet leaves immediately | cooldown-gated | Snappy, no wind-up |
| Hit/i-frame blink | 0 | duration of i-frames | 0 | Instantly readable "I got hit" |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Player hit | ~80 ms (Phase 3) | Hit-stop + screen shake + flash on the hit (JuiceManager, Phase 3) |
| Final death | longer | Bigger explosion + shake (Phase 3) |

### Weight and Responsiveness

- **Weight**: Light and reactive — the ship is an agile instrument, not a heavy vehicle. Minimal/zero momentum.
- **Player control**: Full mid-action course-correction; movement is never committed.
- **Snap quality**: Crisp — direction changes are immediate.
- **Failure texture**: Fair — the i-frame flash + cause-of-death must be readable; the player should never feel a death was "stolen."

### Feel Acceptance Criteria

- [ ] Playtesters describe movement as "tight"/"responsive," never "floaty"/"drifty."
- [ ] Firing feels punchy (paired with juice in Phase 3).
- [ ] No player reports dying twice to "one hit" (i-frames working).

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Lives (owned by GameManager) | HUD (UIManager) | On `OnLivesChanged` | Always during run |
| Active power-up/weapon mod | HUD (UIManager) | On `OnPowerUpChosen` | When a mod is active |

> PlayerShip drives no HUD directly; lives/power-up display is UIManager off GameManager/PowerUpSystem events.

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Move/fire intent | `InputManager.md` | `MoveAxis`, `FireHeld`, `OnFirePressed` | Data dependency |
| Bullet spawn + collision | `Projectile.md` | pool spawn, `TakeHit` callback | Ownership handoff |
| Lives decrement | `GameManager.md` | `OnPlayerDeath` listener | State trigger |
| Run-scoped buffs | `PowerUpSystem.md` | `OnPowerUpChosen`, modifiable stats | Rule dependency |
| Hit feedback | `AudioManager.md` / `JuiceManager.md` | `OnPlayerHit`/`OnPlayerDeath` | State trigger |

---

## Acceptance Criteria

- [ ] Ship moves only on X, clamped to bounds; no vertical movement exists (D1/N1).
- [ ] Firing spawns pooled player projectiles upward, gated by `FireCooldown`; no per-shot `Instantiate`/`Destroy`.
- [ ] A vulnerable hit fires `OnPlayerHit` + `OnPlayerDeath` and starts i-frames; GameManager decrements one life.
- [ ] A hit during i-frames deals zero damage and costs no life (I-frame invariant).
- [ ] I-frame timer uses `unscaledDeltaTime` and expires correctly even with `timeScale = 0`.
- [ ] `OnPowerUpChosen` applies a run-scoped modifier that resets on a new run.
- [ ] No `Find`/`FindObjectOfType`; no legacy `Input` usage (reads through InputManager).
- [ ] Performance: movement+fire update allocation-free; within the 8.3 ms @120fps budget.
- [ ] No hardcoded feel values — `MoveSpeed`, `BaseFireCooldown`, `InvulnDuration`, bounds exposed via Inspector/SO.

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| **Architecture-contract flag**: `architecture.md` lists GameManager listening to `OnPlayerDeath` and "decrements lives." This GDD treats `OnPlayerHit` as the feedback hook and `OnPlayerDeath` as the per-hit life-loss signal (both fire per vulnerable hit). Should we instead split them as `OnPlayerHit` = per-hit (decrements life) and `OnPlayerDeath` = final death only? That would change which event GameManager listens to → requires updating `architecture.md` first. | designer / Claude | Before PlayerShip + GameManager implementation | **Pending — must resolve before coding either system.** Two events firing together is slightly redundant; collapsing/clarifying touches the frozen contract. |
| Tap-fire (`OnFirePressed`) vs held/auto-fire (`FireHeld`) for the base weapon? | designer | Before implementation | Leaning: tap + PlayerShip cooldown cap; power-ups may switch to held (mirror of InputManager Open Question). |
| On a non-final hit, does the ship recenter/"respawn" or stay where it was hit? | designer | Before implementation | Leaning: stay in place + i-frame blink (less disorienting than a teleport); revisit in feel pass. |
| Exact modifiable stat surface for power-ups (fire rate, multi-shot, spread, projectile speed, extra life?). | designer | Defined in `PowerUpSystem.md` (Tier 3) | Pending — finalized when PowerUpSystem is designed. |
