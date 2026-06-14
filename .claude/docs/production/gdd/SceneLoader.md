# SceneLoader

> **Status**: Draft
> **Last Updated**: 2026-06-13
> **Implements Pillar**: Foundation — enables "Fun / quick stakes": instant, seamless restart (< 2s back into action).

## Summary

SceneLoader is the single gateway for all scene transitions. It additively loads and unloads the four scenes of the project's scene split — `Bootstrap`, `MainMenu`, `GameLogic`, `HUD` — and fires `OnSceneLoaded` when a load completes. Nothing else in the project is allowed to call `SceneManager.LoadScene`; GameManager requests every transition through SceneLoader.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `None` (called by GameManager)

---

## Overview

Unity loads scenes; this game needs that to happen in exactly one controlled place so the Bootstrap/MainMenu/GameLogic/HUD split stays intact and persistent objects are never duplicated. SceneLoader wraps `SceneManager` with an intent-level API ("show the menu", "start gameplay", "restart gameplay") and performs the underlying additive load/unload. The player never sees it — they just see a menu, then gameplay with a HUD, then an instant restart on death. SceneLoader makes those transitions atomic and announces completion so dependent systems (e.g. UIManager binding to a freshly-loaded HUD) initialize at the right moment.

## Player Fantasy

Invisible, but it guards the "death is cheap, restart is instant" promise from the vision (< 2s back into action). The player should feel zero friction between runs — no long load, no flicker, no lost HUD. The fantasy: *the game is always ready for one more run.*

---

## Detailed Design

### Core Rules

1. SceneLoader lives in `Bootstrap.unity` (loaded first, never unloaded for the app's lifetime). It does not use `DontDestroyOnLoad`.
2. **Bootstrap** is the persistent root scene (GameManager, SceneLoader, AudioManager live here). It is loaded once at startup and never unloaded.
3. The other three scenes load **additively** (`LoadSceneMode.Additive`) on top of Bootstrap and are unloaded when no longer needed:
   - `MainMenu` — title/start screen.
   - `GameLogic` — the playfield (player, enemies, projectiles).
   - `HUD` — UI Toolkit overlay (score/lives/level, power-up pick, game-over).
4. SceneLoader exposes intent methods, not raw scene names, e.g. `LoadMainMenu()`, `LoadGameplay()` (loads GameLogic + HUD together), `ReloadGameplay()`, `UnloadGameplay()`. Exact method names finalized at implementation; the rule is callers express intent, SceneLoader maps it to additive load/unload sequences.
5. Each load is asynchronous (`LoadSceneAsync`). When the requested scene(s) finish loading and activate, SceneLoader fires `OnSceneLoaded` carrying which logical scene set is now active.
6. **Transition atomicity**: when switching contexts (e.g. menu → gameplay), SceneLoader loads the new scene set, then unloads the old, so Bootstrap-owned objects always have a valid scene around them; it never leaves the game with zero non-Bootstrap scenes mid-transition.
7. **Restart** path: `ReloadGameplay()` unloads GameLogic + HUD and loads them fresh, then fires `OnSceneLoaded`. GameManager calls this on `OnRestartRequested`, then `StartRun()`.
8. SceneLoader never reads or writes run state (lives/level/score). It only loads scenes and reports completion. It holds no game logic.
9. No system other than SceneLoader calls `SceneManager.LoadScene`/`LoadSceneAsync` or uses `LoadSceneMode.Single` (`best-practices.md`).

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Idle` | A load/unload completed | A load/unload request arrives | No work; last active scene set is live. |
| `Loading` | A load/unload request arrives | Async op completes + scenes activated | Drives `LoadSceneAsync`/`UnloadSceneAsync`; ignores further requests until done (see edge cases). |

> SceneLoader is effectively a tiny two-state async wrapper; the meaningful "states" of the game are run states owned by GameManager, not here.

### Interactions with Other Systems

| System | Interaction |
|---|---|
| GameManager | GameManager calls SceneLoader's intent methods (direct call). Data in: load request. Data out: none returned synchronously — completion is signalled by `OnSceneLoaded`. |
| UIManager | Subscribes to `OnSceneLoaded` to know when the HUD scene is live and it can bind its `UIDocument` and event listeners. |
| Any system in GameLogic/HUD | Initializes via its own `Awake`/`OnEnable` after the scene loads; may also key off `OnSceneLoaded` if it must wait for a sibling scene. |

---

## Formulas

No formulas. SceneLoader performs async load/unload calls only.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| A second load request arrives while `Loading` | Queue or ignore until the current op completes; never run two conflicting loads concurrently | Concurrent additive loads/unloads can leave duplicate or orphaned scenes. |
| Restart requested while gameplay scenes are mid-load | Defer the reload until the current load finishes, then reload | Avoids unloading a scene that hasn't finished activating. |
| Requested scene name not in Build Settings | Log a clear error; do not crash; remain in last valid state | A missing scene is a build-config bug, surfaced loudly not silently. |
| Bootstrap accidentally targeted for unload | Refuse — Bootstrap is never unloaded | Persistent objects (GameManager, SceneLoader, Audio) must survive. |
| `LoadGameplay` called when gameplay already loaded | Treat as reload (unload then load) or no-op by design choice (see Open Questions) | Prevents stacked duplicate GameLogic scenes. |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| GameManager | It depends on this | Ownership handoff — GameManager requests all transitions |
| UIManager | It depends on this | State trigger — waits on `OnSceneLoaded` to bind HUD |
| Unity `SceneManager` / Build Settings | This depends on it | Data dependency — scenes must be registered in Build Settings |

> Nature options: `Data dependency` · `State trigger` · `Rule dependency` · `Ownership handoff`

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `MinTransitionTime` (optional) | 0 s | 0–1 s | Longer guaranteed transition (room for a fade) | Snappier, possible visible pop |

> SceneLoader has almost no tunables — it is plumbing. `MinTransitionTime` is optional and only relevant if a fade/wipe is added later; default 0 keeps restart instant per the vision. Scene names/paths are configuration constants, not balance knobs.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Scene transition (menu↔gameplay, restart) | Optional fade/wipe (deferred; instant cut for MVP) | None from SceneLoader (music handled by AudioManager off run events) | Polish |

> MVP is an instant cut. A transition fade is a Phase 3 juice/polish item, not required for the core loop.

---

## Game Feel

### Feel Reference

> "Restart should feel like *an arcade cabinet's instant respawn* — press, and you're back in. NOT a console game's 3-second 'Loading…' screen between attempts."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Restart → gameplay live again | < 2 s (vision requirement) | n/a (async scene op) |
| Menu "Play" → gameplay live | < 2 s | n/a (async scene op) |

### Animation Feel Targets

Not applicable — SceneLoader owns no animations. An optional transition fade would be a separate presentation concern.

### Impact Moments

None — SceneLoader is intentionally invisible. Its quality is the *absence* of a hitch.

### Weight and Responsiveness

- **Weight**: Weightless by design — transitions should feel instant.
- **Player control**: Player triggers transitions indirectly (Play, Restart); SceneLoader commits them.
- **Snap quality**: Crisp — a scene set is loaded or it isn't.
- **Failure texture**: A failed load logs loudly and stays in the last good state rather than dumping the player into a broken scene.

### Feel Acceptance Criteria

- [ ] Restart returns the player to live gameplay in under 2 seconds with no visible duplicate objects.
- [ ] No playtester notices a "loading" beat between menu and gameplay or between deaths.

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| (none) | — | — | SceneLoader displays no UI itself |

> An optional transition fade overlay, if added, would be owned by UIManager, not SceneLoader.

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Who requests transitions | `GameManager.md` | scene-load calls, restart flow | Ownership handoff |
| HUD bind timing | `UIManager.md` | `OnSceneLoaded` subscription | State trigger |
| Scene split rules | `best-practices.md` | Bootstrap/MainMenu/GameLogic/HUD, no `DontDestroyOnLoad` | Rule dependency |

---

## Acceptance Criteria

- [ ] Bootstrap loads first and is never unloaded; SceneLoader and GameManager persist in it without `DontDestroyOnLoad`.
- [ ] MainMenu, GameLogic, HUD load additively and unload cleanly with no orphaned or duplicated scenes.
- [ ] `OnSceneLoaded` fires after the requested scene set is loaded and activated.
- [ ] Restart (`ReloadGameplay`) returns to live gameplay in < 2 s with a single clean GameLogic + HUD pair.
- [ ] Concurrent/overlapping load requests never run two conflicting ops at once.
- [ ] No `SceneManager.LoadScene`, `LoadSceneMode.Single`, or `DontDestroyOnLoad` outside SceneLoader; SceneLoader itself uses additive loads only.
- [ ] All four scenes are registered in Build Settings.
- [ ] Performance: scene-load logic adds no per-frame cost while Idle; the async load itself does not stall the main thread visibly.

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Are GameLogic and HUD always loaded/unloaded as a pair, or independently (e.g. HUD persists across a GameLogic reload)? | designer / Claude | Before implementation | Leaning: load/unload as a pair for simplicity; revisit if HUD-persist saves restart time. |
| Does the app auto-load MainMenu after Bootstrap, or does Bootstrap go straight to gameplay for v1? | designer | Before MainMenu/UIManager work | Pending — ties to GameManager's "who calls StartRun first" question. |
| Is a transition fade in scope for v1, or strictly Phase 3 polish? | designer | Phase 3 | Pending — MVP uses instant cut. |
| The four scenes don't exist yet (only `SampleScene.unity`). Should implementation create them, and is `SampleScene` deleted/renamed? | Claude | At SceneLoader implementation | Pending — implementation step creates Bootstrap/MainMenu/GameLogic/HUD under `Assets/Scenes/`. |
