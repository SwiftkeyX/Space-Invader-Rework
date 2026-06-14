# GameManager

> **Status**: Draft
> **Last Updated**: 2026-06-13
> **Implements Pillar**: Foundation for all pillars — owns the run lifecycle that the Explosive/Chaotic loop plays out inside.

## Summary

GameManager is the single source of truth for run state: how many lives remain, which of the 6 levels is current, and whether a run is active, won, or lost. It is the only singleton in the project (lives in `Bootstrap.unity`, accessed via `GameManager.Instance`) and broadcasts every run-state change as a C# event so HUD, audio, and level systems react without querying it directly.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `SceneLoader` (direct), `PlayerShip` (listens to `OnPlayerDeath`)

---

## Overview

In this Space Invaders rework the player gets a single run: start with 3 lives, fight through 6 fixed levels, and either clear level 6 (win) or hit 0 lives (lose). GameManager owns that run-scoped state. It does not move the player, spawn enemies, or render UI — it only tracks lives and level index, decides when the run starts and ends, and tells everyone else by raising events. Because runs are pure and identical every time (D3, no meta-progression), GameManager holds no persistent data: a new run resets it completely.

## Player Fantasy

GameManager is invisible to the player, but it underwrites the "one more run" pull. The player should feel the stakes of a finite resource (3 lives) and forward momentum through a tightening curve (level 1 → 6) without ever seeing a loading seam or an inconsistent score/lives readout. The fantasy it protects: *every death matters and every run is a clean slate.*

---

## Detailed Design

### Core Rules

1. GameManager is a singleton. On `Awake` it assigns `Instance = this`. It lives only in `Bootstrap.unity`; it never calls `DontDestroyOnLoad` (Bootstrap persistence comes from the scene split, per `best-practices.md`).
2. If a second GameManager ever awakes (duplicate), it destroys itself immediately and does not overwrite `Instance`.
3. Run state consists of: `Lives` (int), `CurrentLevelIndex` (int, 1-based, 1..6), and `State` (enum: `Boot`, `Running`, `Won`, `Lost`).
4. **Starting a run**: `StartRun()` sets `Lives = StartingLives` (default 3), `CurrentLevelIndex = 1`, `State = Running`, then fires `OnRunStarted`, `OnLivesChanged(Lives)`, and `OnLevelChanged(1)`.
5. **Player death**: GameManager subscribes to `PlayerShip.OnPlayerDeath`. On that event it decrements `Lives` by 1 and fires `OnLivesChanged(Lives)`. If `Lives <= 0` it ends the run as a loss (see rule 7).
6. **Advancing a level**: when LevelManager reports the player cleared a level, GameManager increments `CurrentLevelIndex` and fires `OnLevelChanged(CurrentLevelIndex)`. If the cleared level was the last (`CurrentLevelIndex` would exceed `TotalLevels` = 6), it ends the run as a win (see rule 7). *(See Open Questions — exact trigger edge between LevelManager and GameManager.)*
7. **Ending a run**: `State` becomes `Lost` (lives depleted) or `Won` (level 6 cleared). GameManager fires `OnRunEnded(result)` carrying the outcome. No further lives/level events fire until the next `StartRun()`.
8. **Restart**: GameManager subscribes to `UIManager.OnRestartRequested`. On that event it requests the GameLogic scene reload via `SceneLoader`, then calls `StartRun()` to reset state for a fresh, identical run (D3).
9. GameManager requests all scene transitions through `SceneLoader` (direct method call). It never calls `SceneManager.LoadScene` directly.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Boot` | App launch (Bootstrap loaded) | `StartRun()` called (player starts from MainMenu) | Idle; no run events fire. Awaits start. |
| `Running` | `StartRun()` | Lives reach 0, or level 6 cleared | Lives/level events fire; gameplay is live. |
| `Won` | Level 6 cleared while `Running` | `StartRun()` (restart) | Fires `OnRunEnded(Won)`; awaits restart. |
| `Lost` | Lives reach 0 while `Running` | `StartRun()` (restart) | Fires `OnRunEnded(Lost)`; awaits restart. |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| PlayerShip | Listens to `PlayerShip.OnPlayerDeath` → decrements lives. Data in: death event. No data back (PlayerShip reads nothing from GameManager). |
| SceneLoader | Direct method call to request scene loads (start run, restart). The only scene-load path in the project. |
| LevelManager | LevelManager subscribes to `OnLevelChanged` to know which level config to spawn. GameManager learns "level cleared" from LevelManager so it can advance the index. |
| UIManager | Subscribes to `OnLivesChanged`, `OnLevelChanged`, `OnRunStarted`, `OnRunEnded` to render HUD and game-over/win screens. Fires `OnRestartRequested` back to GameManager. |
| AudioManager | Subscribes to `OnRunStarted` / `OnRunEnded` for music/stinger cues. |

---

## Formulas

No formulas. GameManager performs integer increments/decrements only (lives ±1, level +1); all balance lives in `LevelData` ScriptableObjects owned by LevelManager.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Player death event arrives while `State != Running` (e.g. during game-over) | Ignore — no lives change | Prevents negative lives and double-counting a death after the run already ended. |
| Lives reach exactly 0 | End run as `Lost`, fire `OnRunEnded(Lost)` once | A single, clean loss transition; no negative lives. |
| Level 6 cleared | End run as `Won`, do NOT increment to a non-existent level 7 | 6 fixed levels (D5); there is no level 7. |
| Duplicate GameManager instantiated | The duplicate destroys itself, leaves `Instance` untouched | Single-singleton invariant (`best-practices.md`). |
| `OnPlayerDeath` and a level-clear arrive in the same frame | Process death first (lives), then level advance; if lives hit 0, the loss wins and the level advance is dropped | Death is the harder stop; a dead player cannot advance. |
| Restart requested mid-run | Reload GameLogic via SceneLoader, then `StartRun()` resets to lives=3/level=1 | Pure run reset, no carry-over (D3). |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| SceneLoader | This depends on it | Ownership handoff — GameManager calls it to load/reload scenes |
| PlayerShip | This depends on it | State trigger — listens to `OnPlayerDeath` to decrement lives |
| LevelManager | It depends on this | State trigger — reads `OnLevelChanged`; reports level-cleared so GameManager advances |
| UIManager | It depends on this | State trigger — listens to lives/level/run events; fires `OnRestartRequested` |
| AudioManager | It depends on this | State trigger — listens to run start/end events |

> Nature options: `Data dependency` · `State trigger` · `Rule dependency` · `Ownership handoff`

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `StartingLives` | 3 | 1–5 | More forgiving; lowers restart-rage; softens curve (D2 knob) | Harsher; closer to one-hit death tension |
| `TotalLevels` | 6 | 6 (fixed for v1) | n/a — fixed by D5 | n/a — fixed by D5 |

> Both exposed via the Inspector on the GameManager component (a small `GameConfig` ScriptableObject is acceptable if other globals accrue). `StartingLives` is the explicit lever called out in D2 for the Phase 3 feel pass.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Run started | HUD shows lives=3, level=1 (UIManager) | Run-start music cue (AudioManager) | MVP |
| Lives changed | HUD lives counter updates (UIManager) | Life-lost stinger (AudioManager) | MVP |
| Level changed | HUD level indicator updates (UIManager) | Level-up cue (AudioManager) | Alpha |
| Run ended (Lost) | Game-over screen (UIManager) | Game-over music (AudioManager) | MVP |
| Run ended (Won) | Victory screen (UIManager) | Victory fanfare (AudioManager) | Alpha |

> GameManager produces no visuals/audio itself — it only fires the events these systems render.

---

## Game Feel

### Feel Reference

> "Run transitions should feel like *Arcade-era coin-op continuity* — the readout (lives/level) is always instant and correct, never a stale or flickering number. NOT a modern game with a visible loading hitch between levels."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Restart pressed → fresh run visible | ≤ 100 ms (one scene reload) | n/a (scene transition, not per-frame) |
| Player death → lives readout drops | ≤ 1 frame (same frame as death event) | 1 frame |

### Animation Feel Targets

Not applicable — GameManager owns no animations. Lives/level readout animation is UIManager's responsibility.

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Final-life loss → game over | immediate | `OnRunEnded(Lost)` fires the same frame lives hit 0 (juice/hit-stop owned by JuiceManager in Phase 3) |

### Weight and Responsiveness

- **Weight**: Light and immediate — state changes are instantaneous integer flips; the *feel* of weight comes from the systems that react (audio stinger, screen flash).
- **Player control**: Player cannot course-correct a death once the event fires; the lives decrement is committed.
- **Snap quality**: Crisp and binary — a life is gone or it isn't; a level is current or it isn't.
- **Failure texture**: Fair — i-frames (PlayerShip/D2) ensure a death only counts once, so the lives readout never feels like it "stole" a life.

### Feel Acceptance Criteria

- [ ] Lives/level HUD is never visibly out of sync with actual state during a run.
- [ ] No playtester reports losing "two lives at once" from one hit (guarded by i-frames + the same-frame edge rule).
- [ ] Restart produces a visibly identical fresh run every time.

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Lives remaining | HUD (UIManager) | On `OnLivesChanged` | Always during a run |
| Current level (1–6) | HUD (UIManager) | On `OnLevelChanged` | Always during a run |
| Run result (Win/Lose) | Full-screen end panel (UIManager) | On `OnRunEnded` | After run ends |

> GameManager supplies the data via events; UIManager owns all layout/markup (UI Toolkit, `best-practices.md`).

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Lives decrement trigger | `PlayerShip.md` | `OnPlayerDeath` event | State trigger |
| Scene load/reload | `SceneLoader.md` | scene-load method | Ownership handoff |
| Level advance + count | `LevelManager.md` | level-cleared signal, 6-level curve (D5) | State trigger / rule dependency |
| HUD + restart | `UIManager.md` | `OnRestartRequested`, lives/level/run events | State trigger |
| Run music cues | `AudioManager.md` | run start/end events | State trigger |

---

## Acceptance Criteria

- [ ] `GameManager.Instance` is set on Awake in Bootstrap; a second instance self-destructs without overwriting `Instance`.
- [ ] `StartRun()` sets lives=3, level=1, state=Running and fires `OnRunStarted`, `OnLivesChanged(3)`, `OnLevelChanged(1)`.
- [ ] `PlayerShip.OnPlayerDeath` decrements lives by exactly 1 and fires `OnLivesChanged`.
- [ ] Lives reaching 0 fires `OnRunEnded(Lost)` exactly once; subsequent death events are ignored.
- [ ] Clearing level 6 fires `OnRunEnded(Won)` and never advances to level 7.
- [ ] `OnRestartRequested` reloads GameLogic via SceneLoader and resets to a fresh run.
- [ ] No `SceneManager.LoadScene`, no `DontDestroyOnLoad`, no `Find`/`FindObjectOfType` anywhere in the script.
- [ ] Performance: all state updates are O(1) integer ops; system update completes within < 0.1 ms per frame.
- [ ] No hardcoded values in implementation — `StartingLives` and `TotalLevels` exposed via Inspector.

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Exact handshake for "level cleared → advance": does LevelManager call a GameManager method, or does GameManager subscribe to `LevelManager.OnLevelCleared`? architecture.md lists `LevelManager` firing `OnLevelCleared` but GameManager listening only to `PlayerShip.OnPlayerDeath`. | designer / Claude | Before LevelManager GDD (Tier 2) | Pending — resolve when designing LevelManager so the edge is declared in architecture.md before coding either. |
| Who calls `StartRun()` first — MainMenu "Play" button (via UIManager) or an auto-start on GameLogic load? | designer | Before UIManager GDD (Tier 3) | Pending |
| Should `GameConfig` be a ScriptableObject now, or are two Inspector ints on GameManager enough for v1? | Claude | Before implementation | Leaning: two Inspector ints for v1; promote to SO only if more globals appear. |
