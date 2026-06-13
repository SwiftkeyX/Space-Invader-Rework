# JuiceManager

> **Status**: Draft — **RESERVED (implemented in Phase 3 juice pass, per D6)**
> **Last Updated**: 2026-06-13
> **Implements Pillar**: Explosive — the screen shake, particles, and hit-stop that make every kill a fireworks moment.

## Summary

JuiceManager owns non-audio game feel: screen shake, particle bursts, and hit-stop (brief `timeScale` freezes). It subscribes to the same broadcast events as audio (`Enemy.OnEnemyKilled`, `PlayerShip.OnPlayerHit`, `LevelManager.OnLevelCleared`) and plays the matching effect. It is **reserved now** (an architecture seat per D6) but not implemented until the Phase 3 juice pass — this GDD specifies its contract so the rest of the project can wire to it without it existing yet.

> **Quick reference** — Layer: `Presentation` · Priority: `Full Vision` (Phase 3) · Key deps: `Enemy`, `PlayerShip`, `LevelManager`

---

## Overview

Heavy juice is a first-class feature in this game, not afterthought polish (D6). JuiceManager is the visual-feel counterpart to AudioManager: when an enemy dies it bursts particles and nudges the camera; when the player is hit it shakes and briefly freezes time (hit-stop) for impact; when a formation clears it delivers the big "fireworks finale." It listens to gameplay events and renders effects from pooled particle systems and a camera-shake routine. It is built during the Phase 3 juice pass, but its event contract is fixed now so other systems already fire the events it will consume.

## Player Fantasy

Spectacle. The Explosive pillar lives here — every kill pops with particles and a kick, big moments freeze for a beat of impact, and clearing a level erupts. The fantasy: the game *feels* as explosive as an arcade cabinet on its loudest setting; destruction is viscerally satisfying.

---

## Detailed Design

### Core Rules

1. **Reserved seat (D6)**: JuiceManager exists in the architecture now (so events have a consumer) but its effects are authored/tuned in Phase 3. Until then, it may be a no-op stub or absent; nothing else depends on its output to function.
2. **Event-driven effects**: subscribes to:
   - `Enemy.OnEnemyKilled` → particle burst at the kill position + small camera shake (scaled by combo/clear size).
   - `PlayerShip.OnPlayerHit` → strong shake + hit-stop + impact flash; (final death → bigger effect).
   - `LevelManager.OnLevelCleared` → big celebratory burst / screen effect (the "fireworks finale").
3. **Hit-stop (the critical constraint)**: hit-stop sets `Time.timeScale = 0` for a brief window (~60–120 ms) then restores it. **This is the reason for the project-wide unscaled-timer rule** (`best-practices.md`): any i-frame / power-up / throttle timer that used scaled time would silently stall during hit-stop. JuiceManager's own hit-stop timer therefore uses real/unscaled time (`WaitForSecondsRealtime`).
4. **Camera shake**: a decaying positional/rotational offset on the gameplay camera, driven by an intensity + duration per event, using unscaled time so it animates during hit-stop. Multiple shakes combine/clamp rather than stacking unbounded.
5. **Particles pooled (hard rule)**: particle bursts come from pre-warmed pooled particle systems — no `Instantiate`/`Destroy` per kill (`best-practices.md` zero-GC). At L6 density, kill bursts must be pooled and capped.
6. **Density safety**: like audio, visual juice must throttle/cap at high kill rates — cap concurrent particle bursts and shake intensity so a formation wipe is a satisfying eruption, not a frame-killing storm. Effects degrade gracefully under load (skip/merge bursts before dropping frames).
7. **No events, no gameplay effect** (except the deliberate, shared `timeScale` change for hit-stop): JuiceManager fires no C# events and changes no gameplay state other than the global hit-stop freeze, which is its defined responsibility. It must always restore `timeScale` (guard against leaving the game frozen).
8. JuiceManager uses no `Find`/`FindObjectOfType`; it subscribes to the contract events; camera/particle-pool references are assigned per the contract.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Reserved/stub` | Phases 1–2 | Phase 3 juice pass | No-op or absent; events have no visible effect yet |
| `Idle` (Phase 3+) | No active effect | An event arrives | Camera at rest, no bursts |
| `Shaking/Bursting` | Kill/hit/clear event | Effect duration elapses (unscaled) | Plays shake + particles |
| `HitStop` | Player hit / big impact | Hit-stop duration elapses (real time) → restore `timeScale` | `timeScale=0` for the window, then restored |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| Enemy | Subscribes to `OnEnemyKilled` → particles + shake at position. |
| PlayerShip | Subscribes to `OnPlayerHit`/`OnPlayerDeath` → shake + hit-stop + flash. |
| LevelManager | Subscribes to `OnLevelCleared` → celebratory burst. |
| Camera | Direct ref — applies/restores shake offset. |
| Particle pools | Acquire/release pooled burst systems. |
| (Whole game) | Hit-stop changes global `timeScale` — the shared reason for the unscaled-timer rule across systems. |

---

## Formulas

### Camera shake (decaying)

```
shakeOffset = randomUnitVector * currentIntensity
currentIntensity -= (startIntensity / duration) * Time.unscaledDeltaTime   // decays to 0
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `startIntensity` | float | 0.05–0.6 units | SO per event | Initial shake magnitude |
| `duration` | float | 0.05–0.5 s | SO per event | Shake decay time |

### Hit-stop

```
timeScale = 0 for HitStopDuration (real time), then timeScale = 1
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `HitStopDuration` | float | 0.04–0.12 s | SO per event | Freeze length (~80 ms target, D6) |

**Edge cases**: never leave `timeScale = 0`; clamp combined shake intensity; cap concurrent bursts.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Many kills same frame | Cap concurrent bursts + clamp shake; merge into one bigger eruption | Satisfying, not frame-killing. |
| Overlapping hit-stops | Use the longest/most-recent; never additively freeze indefinitely | Avoid a stuck-frozen game. |
| Exception mid-hit-stop | `timeScale` restored in a `finally`/guaranteed path | Never strand the game at `timeScale=0`. |
| Effects requested before Phase 3 build | No-op (reserved) | JuiceManager isn't required for the core loop to function. |
| Shake during pause | Respect pause (no shake while genuinely paused) vs hit-stop (shake continues) | Distinguish intentional pause from hit-stop. |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| Enemy | This depends on it | State trigger — `OnEnemyKilled` |
| PlayerShip | This depends on it | State trigger — `OnPlayerHit`/`OnPlayerDeath` |
| LevelManager | This depends on it | State trigger — `OnLevelCleared` |
| Camera / particle pools | This depends on them | Ownership handoff — shake + burst rendering |
| (All timed systems) | They depend on its constraint | Rule dependency — hit-stop forces unscaled timers project-wide |

> Nature options: `Data dependency` · `State trigger` · `Rule dependency` · `Ownership handoff`

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| Kill shake intensity | 0.08 | 0–0.6 | More kick per kill | Subtler |
| Player-hit shake intensity | 0.3 | 0–0.6 | More dramatic hit | Subtler |
| `HitStopDuration` | 0.08 s | 0.04–0.12 | Heavier impact (riskier feel) | Snappier |
| Max concurrent bursts | 24 | 8–48 | Denser spectacle (perf cost) | Lighter |
| Clear-burst scale | large | — | Bigger finale | Smaller |

> All juice values live in SOs / Inspector — this is the **primary Phase 3 juice-pass tuning surface**. No magic numbers. Defaults above are starting points for the juice pass.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Enemy killed | Particle pop + small shake | (kill SFX — AudioManager) | Full Vision (Phase 3) |
| Player hit | Strong shake + hit-stop + flash | (hit SFX — AudioManager) | Full Vision (Phase 3) |
| Player death | Big shake + explosion particles | (death SFX — AudioManager) | Full Vision (Phase 3) |
| Formation/level cleared | Fireworks burst / screen flash | (clear sting — AudioManager) | Full Vision (Phase 3) |

> JuiceManager owns the non-audio spectacle; AudioManager owns the matching sound off the same events.

---

## Game Feel

### Feel Reference

> "Juice like *Nuclear Throne* / *Vlambeer* — screen shake, hit-stop, and particle spray that make every action feel huge. NOT a static camera with tiny puff effects."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Event → effect visible | ≤ 1 frame | 1 frame |

### Animation Feel Targets

| Animation | Startup | Active | Recovery | Feel Goal |
|---|---|---|---|---|
| Hit-stop | 0 | ~80 ms freeze | instant resume | Punchy impact, not sluggish |
| Camera shake | 0 | 0.05–0.5 s decay | settle | Kinetic but controllable |
| Kill burst | 0 | brief | pool release | Snappy pop |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Player hit | ~80 ms | Hit-stop + strong shake + flash |
| Enemy kill | instant | Particle pop + micro-shake |
| Level clear | longer | Fireworks eruption (the vision's "finale") |

### Weight and Responsiveness

- **Weight**: Heavy, impactful — juice is what gives the game its punch.
- **Player control**: Effects must enhance, never obscure — readability (Fun pillar) caps how much shake/particle is acceptable.
- **Snap quality**: Crisp impacts; hit-stop resumes instantly.
- **Failure texture**: Getting hit *feels* like a real impact (shake + freeze), reinforcing the stakes fairly.

### Feel Acceptance Criteria

- [ ] Playtesters comment on the juice/impact unprompted (D6 success signal).
- [ ] Effects never obscure incoming bullets enough to cause unfair deaths (readability holds).
- [ ] Frame rate holds at L6 density with full juice (perf pass).
- [ ] `timeScale` is never left stuck at 0.

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| (none) | — | — | JuiceManager shows no UI; screen-flash overlay, if used, is its own effect layer |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Kill particles/shake | `Enemy.md` | `OnEnemyKilled` (+ position) | State trigger |
| Hit-stop/shake | `PlayerShip.md` | `OnPlayerHit`/`OnPlayerDeath` | State trigger |
| Clear finale | `LevelManager.md` | `OnLevelCleared` | State trigger |
| Unscaled-timer mandate | `best-practices.md` | hit-stop / `timeScale=0` rule | Rule dependency |
| Reserved-until-Phase-3 | `design-decisions.md` / `systems-design.md` | D6 (juice reserved now, built Phase 3) | Rule dependency |
| Phase 3 juice pass | `.claude/skills/juice-pass` | implementation step | Ownership handoff |

---

## Acceptance Criteria

> Verified in the **Phase 3 juice pass**, not at Tier 3 production coding (reserved seat).

- [ ] Subscribes to `OnEnemyKilled`, `OnPlayerHit`/`OnPlayerDeath`, `OnLevelCleared` and plays the matching effect.
- [ ] Hit-stop sets and reliably restores `timeScale`; never leaves the game frozen (guaranteed restore path).
- [ ] Camera shake and effect timers use unscaled/real time so they animate during hit-stop.
- [ ] Particles are pooled; zero steady-state GC; concurrent bursts + shake intensity capped at L6 density.
- [ ] Fires no events; changes no gameplay state except the defined hit-stop `timeScale` freeze.
- [ ] No `Find`/`FindObjectOfType`; subscribes via the contract.
- [ ] Until Phase 3, a no-op/absent JuiceManager does not break the core loop.
- [ ] Performance: full juice holds the 8.3 ms @120fps budget at peak density.

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Camera-shake implementation: custom decaying offset vs Cinemachine Impulse? | Claude | Phase 3 juice pass | Leaning: lightweight custom shake for control; evaluate Cinemachine Impulse if a virtual camera is already in use. |
| Is a screen-flash/vignette overlay JuiceManager's (effect layer) or UIManager's? | Claude | Phase 3 | Leaning: JuiceManager owns gameplay-impact overlays (flash); UIManager owns HUD/menus. |
| Hit-stop on enemy kills too (tiny), or only player hits / big moments? | designer | Phase 3 feel pass | Leaning: hit-stop on player hits + big clears only; micro-shake (no freeze) on regular kills to avoid choppiness. |
| Global "juice intensity" accessibility slider (reduce shake)? | designer | Phase 3 | Leaning: yes — a reduce-shake option is cheap and good practice. |
