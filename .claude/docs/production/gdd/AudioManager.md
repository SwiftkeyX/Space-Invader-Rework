# AudioManager

> **Status**: Draft
> **Last Updated**: 2026-06-13
> **Implements Pillar**: Explosive — "loud punchy SFX on every kill"; the audio half of the juice that makes the game feel like a fireworks finale.

## Summary

AudioManager plays all music and sound effects in response to game events. It subscribes to gameplay/state events (`Enemy.OnEnemyKilled`, `PlayerShip.OnPlayerHit`, `LevelManager.*`, `GameManager.*`) and triggers the matching clip — kill pops, hit/death sounds, level cues, run music. It fires no events. Crucially, it manages SFX density so that at level-6 bullet-hell volume the mix stays punchy and readable rather than a wall of noise.

> **Quick reference** — Layer: `Presentation` · Priority: `Vertical Slice` · Key deps: `Enemy`, `PlayerShip`, `LevelManager`, `GameManager`

---

## Overview

Audio is a first-class part of the "Explosive" pillar (D6) — every kill should *pop* audibly, hits should land, and music should carry the rising pressure of the run. AudioManager centralizes that: it listens to the same events the rest of the game broadcasts and maps each to a sound (or music change). It lives in `Bootstrap.unity` so music persists across scene loads. Because hundreds of enemies can die in quick succession, it must throttle/round-robin repetitive SFX (especially kills and enemy fire) so the mix never collapses into mush. It produces sound only — it never affects gameplay.

## Player Fantasy

The crunch. Each kill's pop, the satisfying thud of a hit, the quickening march, the swelling music as the level escalates, the triumphant clear sting — audio makes the player *feel* the spectacle and the pressure. The fantasy: the game sounds as explosive and chaotic as it looks, and the audio amplifies every win.

---

## Detailed Design

### Core Rules

1. **Event-driven playback**: AudioManager subscribes to:
   - `Enemy.OnEnemyKilled` → kill/pop SFX (throttled — see rule 4).
   - `PlayerShip.OnPlayerHit` → player-hit SFX; (final death gets a bigger death SFX, distinguishing hit vs run-end per the PlayerShip event resolution).
   - `LevelManager.OnLevelStarted` / `OnLevelCleared` → level-start cue, wave-clear sting.
   - `GameManager.OnRunStarted` / `OnRunEnded` → run music start, game-over / victory music.
   - (Optional) `ScoreSystem` combo events and `PowerUpSystem.OnPowerUpOffered`/`OnPowerUpChosen` → score/combo blips, pick-screen + power-up SFX.
2. **Music vs SFX channels**: separate `AudioSource`s (or an audio mixer with Music/SFX groups) so music and SFX volumes are independently controllable; music is looped, SFX are one-shots.
3. **Lives in Bootstrap**: AudioManager persists in `Bootstrap.unity` (no `DontDestroyOnLoad`) so music continues seamlessly across additive scene loads (menu → gameplay → restart).
4. **SFX throttling / pooling (critical for density)**: repetitive SFX (kills, enemy fire) are rate-limited and/or round-robined across a small pool of voices, with a max-concurrent-voices cap and a minimum re-trigger interval per clip. This prevents phasing/clipping and CPU spikes when many enemies die in one frame at L4–6 density. SFX use pooled `AudioSource`s (no per-shot `Instantiate`/`new`) to honor the zero-GC rule (`best-practices.md`).
5. **No events, no gameplay effect**: AudioManager fires nothing and never calls into gameplay. It is a pure sink for events.
6. **Pause/hit-stop behavior**: SFX/music should behave sensibly during hit-stop (`timeScale=0`) and pause —音 typically keeps playing (audio runs on real time regardless of timeScale); ensure clips aren't accidentally stalled. Any AudioManager-side timers (throttle windows) use real/unscaled time so throttling works during hit-stop.
7. **Mix management**: music may duck slightly under big SFX moments (clear sting, death) — optional polish via mixer snapshots (Phase 3).
8. AudioManager uses no `Find`/`FindObjectOfType`; it subscribes to the contract events and holds clip references via SO/Inspector.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Menu music` | `OnRunStarted` not yet / back to menu | `OnRunStarted` | Loops menu theme |
| `Gameplay music` | `OnRunStarted` | `OnRunEnded` | Loops gameplay track; reacts to level cues; plays SFX |
| `End music` | `OnRunEnded(result)` | `OnRunStarted` (restart) | Game-over or victory track |

> Music state is driven by GameManager run events; SFX play in any state as their events arrive.

### Interactions with Other Systems

| System | Interaction |
|---|---|
| Enemy | Subscribes to `OnEnemyKilled` → kill SFX (throttled). |
| PlayerShip | Subscribes to `OnPlayerHit`/`OnPlayerDeath` → hit/death SFX. |
| LevelManager | Subscribes to `OnLevelStarted`/`OnLevelCleared` → level cues. |
| GameManager | Subscribes to `OnRunStarted`/`OnRunEnded` → music start/end. |
| ScoreSystem / PowerUpSystem | (Optional) score/combo + power-up SFX. |

---

## Formulas

### SFX throttle / voice cap

```
play(clip) only if:
  (now - lastPlayed[clip]) >= MinRetriggerInterval[clip]
  AND activeVoices < MaxConcurrentVoices
else: skip (or steal oldest voice)
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `MinRetriggerInterval` | float | 0.02–0.15 s | SO/Inspector per clip | Min gap between same-clip plays |
| `MaxConcurrentVoices` | int | 8–32 | SO/Inspector | Voice cap for the SFX pool |
| `now` | float | real time | runtime | Unscaled/real timestamp |

**Expected output range**: at most `MaxConcurrentVoices` SFX at once; identical clips spaced by `MinRetriggerInterval`.
**Edge cases**: many kills same frame → only the first within the interval plays (or layered up to cap), avoiding a noise wall.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Dozens of enemies die same frame (formation wipe) | Play a capped/layered kill SFX, not dozens overlapping | Keeps the mix punchy, avoids clipping + CPU spike. |
| Scene reload (restart) | Music continues or restarts cleanly per design; no double music tracks | Bootstrap persistence prevents duplicate AudioManagers. |
| Event fires before clips/sources ready | No-op safely | Avoids null-source errors at startup/scene timing. |
| Hit-stop active (timeScale 0) | Audio keeps playing; throttle timers use real time | Audio shouldn't freeze; throttling must still work. |
| Player hit vs final death | Distinct SFX (hit tick vs big death) | Readability of the two outcomes. |
| Master/mute settings | Respect volume/mute (if options exist) | Player audio control. |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| Enemy | This depends on it | State trigger — `OnEnemyKilled` |
| PlayerShip | This depends on it | State trigger — `OnPlayerHit`/`OnPlayerDeath` |
| LevelManager | This depends on it | State trigger — `OnLevelStarted`/`OnLevelCleared` |
| GameManager | This depends on it | State trigger — run start/end music |
| ScoreSystem / PowerUpSystem | This depends on it (optional) | State trigger — score/combo/power-up SFX |

> Nature options: `Data dependency` · `State trigger` · `Rule dependency` · `Ownership handoff`

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| Music volume | 0.7 | 0–1 | Louder music | Quieter |
| SFX volume | 0.9 | 0–1 | Louder SFX | Quieter |
| `MaxConcurrentVoices` | 16 | 8–32 | Fuller, denser mix (CPU cost) | Cleaner, fewer overlaps |
| `MinRetriggerInterval` (kills) | 0.05 s | 0.02–0.15 | Sparser kill SFX (cleaner) | Denser (riskier mush) |
| Music duck amount (polish) | 0 | 0–6 dB | More dramatic SFX moments | Flatter mix |

> Volumes + throttle params live in an `AudioConfig` SO / mixer; clip references in SO/Inspector. No magic numbers.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Enemy killed | (visual owned by Enemy/Juice) | Punchy kill pop (throttled) | Vertical Slice |
| Player hit / death | (flash/shake owned by PlayerShip/Juice) | Hit tick / big death SFX | Vertical Slice |
| Player fire | (muzzle flash owned by PlayerShip) | Shoot SFX | Alpha |
| Enemy fire | — | Sparse enemy shoot SFX (heavily throttled) | Alpha |
| Level start / clear | (banner owned by UIManager) | Level cue / clear sting | Alpha |
| Run start / end | (screens owned by UIManager) | Music start / game-over / victory | Vertical Slice |
| March (formation) | — | Accelerating march heartbeat | Polish |

> AudioManager owns all audio; visuals belong to the firing systems. This table is AudioManager's responsibility column of the project's feedback map.

---

## Game Feel

### Feel Reference

> "SFX like *Nuclear Throne* — fat, crunchy, immediate kill pops. Music escalation like an *arcade shmup* — driving, rising with the run. NOT thin UI-beep SFX or a flat looping track that ignores the action."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Event fires → SFX audible | ≤ 1 frame (one-shot trigger) | 1 frame |

### Animation Feel Targets

Not applicable — audio, not animation. (Sync of SFX to the visual hit is the relevant target: same-frame trigger.)

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Kill pop | short | Crunchy one-shot, throttled at density |
| Player death | longer | Big death SFX + (music duck, Phase 3) |
| Formation clear | — | Satisfying clear sting (the "fireworks" audio payoff) |

### Weight and Responsiveness

- **Weight**: Punchy and immediate — SFX should feel fat, not thin.
- **Player control**: Indirect — the player's actions trigger the sounds.
- **Snap quality**: Tight sync to the visual event (same frame).
- **Failure texture**: Death/hit sounds are distinct and clear so the player audibly reads what happened.

### Feel Acceptance Criteria

- [ ] Kills sound punchy and stay clean even at L6 density (no mush/clipping).
- [ ] Music escalates with the run; clear/death stings land.
- [ ] No playtester describes the audio as thin, noisy, or laggy.

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| (volume sliders — if options menu) | Options UI (UIManager) | on change | If an options screen ships |

> No HUD of its own; volume controls (if any) are a UIManager options screen reading AudioManager settings.

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Kill SFX | `Enemy.md` | `OnEnemyKilled` | State trigger |
| Hit/death SFX | `PlayerShip.md` | `OnPlayerHit`/`OnPlayerDeath` | State trigger |
| Level cues | `LevelManager.md` | `OnLevelStarted`/`OnLevelCleared` | State trigger |
| Run music | `GameManager.md` | `OnRunStarted`/`OnRunEnded` | State trigger |
| Zero-GC SFX pooling | `best-practices.md` | pool-everything rule | Rule dependency |
| Bootstrap persistence | `best-practices.md` / `SceneLoader.md` | Bootstrap, no `DontDestroyOnLoad` | Rule dependency |

---

## Acceptance Criteria

- [ ] Subscribes to and plays correct audio for `OnEnemyKilled`, `OnPlayerHit`/`OnPlayerDeath`, `OnLevelStarted`/`OnLevelCleared`, `OnRunStarted`/`OnRunEnded`.
- [ ] Music persists across additive scene loads (lives in Bootstrap, no duplicate AudioManager, no `DontDestroyOnLoad`).
- [ ] Repetitive SFX are throttled/voice-capped — formation wipes don't produce a noise wall or CPU spike.
- [ ] Music and SFX are on independent volume channels.
- [ ] SFX use pooled `AudioSource`s; zero steady-state GC during sustained play.
- [ ] Throttle timers use real/unscaled time so they work during hit-stop.
- [ ] Fires no events and makes no gameplay calls.
- [ ] No `Find`/`FindObjectOfType`; subscribes via the contract; clips referenced via SO/Inspector.
- [ ] Performance: audio handling stays within budget at L6 density (no audio-driven frame spikes).

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Audio mixer groups (Music/SFX/Master) vs plain `AudioSource` volume control? | Claude | Before implementation | Leaning: an AudioMixer with Music/SFX groups (cleaner ducking + master volume). |
| Does music change per level (intensity layers) or one gameplay track for the run? | designer | Phase 3 | Leaning: one gameplay track for v1; intensity layering is polish. |
| Are clips generated (e.g. via SFX tooling) or sourced assets? | designer / Claude | Before implementation | Pending — can generate placeholder SFX/music during implementation; finalize in Phase 3. |
| Options/volume UI in v1? | designer | Before UIManager finalize | Leaning: a minimal master/music/SFX slider set, or defer to Phase 3. |
