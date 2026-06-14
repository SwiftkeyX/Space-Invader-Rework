# UIManager (HUD / UI)

> **Status**: Draft
> **Last Updated**: 2026-06-13
> **Implements Pillar**: Fun — readable HUD despite the chaos; fast, clear restart loop.

## Summary

UIManager is the presentation layer: it renders the HUD (score, lives, level), the between-level power-up pick screen, and the game-over / victory / restart UI — all in UI Toolkit (`.uxml` + `.uss`, per `best-practices.md`). It subscribes to GameManager, ScoreSystem, and PowerUpSystem events to display state, and fires `OnRestartRequested` when the player restarts. It contains **no gameplay logic** — it only displays state and relays player UI input.

> **Quick reference** — Layer: `Presentation` · Priority: `Vertical Slice` · Key deps: `GameManager`, `ScoreSystem`, `PowerUpSystem`

---

## Overview

Everything the player reads on screen outside the playfield is UIManager: the live HUD during play, the power-up cards between levels, and the end screens. It lives in the `HUD` scene and binds its `UIDocument` once the scene loads. It is strictly a view: it listens to gameplay/state events and updates visual elements, and it sends player UI actions (pick a power-up, restart) back out as events/callbacks. It never moves the ship, changes score, or alters difficulty — that separation keeps UI swappable and logic testable.

## Player Fantasy

Clarity in chaos. Even when the screen is full of bullets and explosions, the player can glance and instantly read lives, score, and level. The restart flow is so fast and clear that failure invites "one more run" rather than frustration. The power-up screen feels like a satisfying reward moment, not a menu.

---

## Detailed Design

### Core Rules

1. **UI Toolkit only (hard rule)**: HUD, pick screen, and end screens are built with `.uxml` + `.uss` and driven via `UIDocument`/`rootVisualElement` queries — never UGUI Canvas (`best-practices.md`).
2. **Lives in the HUD scene**: UIManager and its `UIDocument` live in the `HUD` scene (additively loaded). It binds `rootVisualElement` after the scene loads (keys off `SceneLoader.OnSceneLoaded` if needed for timing).
3. **HUD display**: subscribes to:
   - `GameManager.OnLivesChanged` → update lives display.
   - `GameManager.OnLevelChanged` → update level (1–6) display.
   - `ScoreSystem.OnScoreChanged` → update score (and combo meter if combos ship).
4. **Power-up pick screen**: subscribes to `PowerUpSystem.OnPowerUpOffered(offerSet)` → builds and shows the card screen. When the player selects a card, UIManager relays the selection back to PowerUpSystem (callback/event), then hides the screen.
5. **Run-state screens**: subscribes to `GameManager.OnRunStarted` (show HUD, hide menus), `OnRunEnded(result)` → show game-over (Lost) or victory (Won) screen with final score.
6. **Restart**: the restart button on the game-over/victory screen fires `OnRestartRequested` → GameManager handles the reload. UIManager does not reload scenes itself.
7. **Presentation only**: UIManager makes no gameplay calls (architecture: "presentation only, no gameplay calls"). It reads events and emits UI-intent events; it never calls into PlayerShip/Enemy/etc.
8. **Pause-safe**: UI animations/timers that must run during a paused/hit-stop state use unscaled time where relevant (pick screen, end screens may appear while gameplay time is altered).
9. UIManager uses no `Find`/`FindObjectOfType` for gameplay objects; it resolves visual elements via `UIDocument` queries and subscribes to the contract events.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `MainMenu` | App start / from end screen | `OnRunStarted` | Title/start UI (in MainMenu scene flow) |
| `HUD` | `OnRunStarted` | `OnRunEnded` / `OnPowerUpOffered` | Live HUD: score/lives/level |
| `PickScreen` | `OnPowerUpOffered` | Player selects a card | Overlay cards; relays choice; HUD paused beneath |
| `GameOver` | `OnRunEnded(Lost)` | `OnRestartRequested` | Game-over panel + final score + restart |
| `Victory` | `OnRunEnded(Won)` | `OnRestartRequested` | Victory panel + final score + restart |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| GameManager | Subscribes to `OnRunStarted`/`OnRunEnded`/`OnLivesChanged`/`OnLevelChanged`; fires `OnRestartRequested` back. |
| ScoreSystem | Subscribes to `OnScoreChanged` (+ combo) → score display. |
| PowerUpSystem | Subscribes to `OnPowerUpOffered` → pick screen; relays selection back. |
| SceneLoader | Keys off `OnSceneLoaded` to bind the `UIDocument` at the right time. |

---

## Formulas

No formulas — UIManager maps state values to display. Any number animation (score tick-up) is presentation easing, not game logic.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Event fires before `UIDocument` is bound | Buffer/no-op until bound, then reflect current state on bind | Avoids null-VisualElement errors at scene-load timing. |
| Lives change to 0 same frame as run end | Show 0 lives, then game-over; no flicker of stale state | Clean end transition. |
| Power-up offered with fewer than N cards | Render only the available cards | Matches PowerUpSystem small-catalog edge. |
| Restart spammed | Fire `OnRestartRequested` once per restart; ignore repeats until reloaded | Prevent double-reload. |
| Pick screen open when window loses focus | Stays open; selection still works on return | Pick is a safe between-level state. |
| Combos disabled | Combo meter hidden entirely | UI adapts to ScoreSystem's combo decision. |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| GameManager | This depends on it | State trigger — lives/level/run events; fires `OnRestartRequested` |
| ScoreSystem | This depends on it | State trigger — `OnScoreChanged` |
| PowerUpSystem | Bidirectional | State trigger — `OnPowerUpOffered` in, selection out |
| SceneLoader | This depends on it | State trigger — `OnSceneLoaded` bind timing |

> Nature options: `Data dependency` · `State trigger` · `Rule dependency` · `Ownership handoff`

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| Score tick-up speed | snappy | — | Flashier climb | Instant set |
| HUD layout/scale | `.uss` | — | — | — |
| Pick-screen card count display | 3 | 2–4 | More cards shown | Fewer |

> UIManager has few balance knobs (it's presentation). Layout/style live in `.uss`; behavior values like tick-up speed are minor Inspector/`.uss` values. No gameplay magic numbers.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Lives changed | Lives icons/number update (lost-life flash = polish) | (life-lost SFX owned by AudioManager) | Vertical Slice |
| Score changed | Score counter (tick-up = polish) | (score blip — AudioManager) | Vertical Slice |
| Level changed | Level indicator update / banner | (level cue — AudioManager) | Alpha |
| Power-up offered | Card screen slide-in | (offer cue — AudioManager) | Vertical Slice |
| Run ended | Game-over / victory panel | (end music — AudioManager) | Vertical Slice |

> UIManager owns visuals; audio is AudioManager off the same events. UI animation polish is largely a Phase 3 juice item.

---

## Game Feel

### Feel Reference

> "HUD like a *modern arcade shooter* — minimal, glanceable, never in the way. Restart flow like *Hades* — fail screen to next attempt in seconds. NOT a cluttered HUD or a slow multi-click restart."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| State change → HUD updates | ≤ 1 frame | 1 frame |
| Card click → screen closes + choice applied | ≤ 100 ms | — |
| Restart click → reload begins | ≤ 100 ms | — |

### Animation Feel Targets

| Animation | Startup | Active | Recovery | Feel Goal |
|---|---|---|---|---|
| Pick screen in/out | brief | choice | — | Rewarding, snappy |
| Score tick-up | 0 | brief | — | Lively but not laggy |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Run end screen | — | Clear, immediate result + final score; one obvious restart action |

### Weight and Responsiveness

- **Weight**: Light, instant — UI must never feel sluggish.
- **Player control**: Direct on menus/pick/restart; instant feedback.
- **Snap quality**: Crisp — elements update the frame state changes.
- **Failure texture**: The game-over screen makes restarting effortless, turning failure into "one more run."

### Feel Acceptance Criteria

- [ ] HUD stays readable at peak (level 6) on-screen density.
- [ ] Restart returns to play in a couple of seconds with one clear action.
- [ ] No playtester calls the HUD cluttered or the restart flow slow.

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Score (+ combo if shipped) | HUD top | On `OnScoreChanged` | During run |
| Lives | HUD corner | On `OnLivesChanged` | During run |
| Level (1–6) | HUD top/center | On `OnLevelChanged` | During run |
| Active power-ups | HUD edge | On `OnPowerUpChosen` | When mods active |
| Power-up cards | Center overlay | On `OnPowerUpOffered` | Between levels |
| Final score + result | Center panel | On `OnRunEnded` | After run |
| Restart action | End panel | — | After run |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Lives/level/run + restart | `GameManager.md` | lives/level/run events, `OnRestartRequested` | State trigger |
| Score/combo | `ScoreSystem.md` | `OnScoreChanged`, combo meter | State trigger |
| Pick screen | `PowerUpSystem.md` | `OnPowerUpOffered`, selection callback | State trigger |
| Bind timing | `SceneLoader.md` | `OnSceneLoaded`, HUD scene | State trigger |
| UI Toolkit mandate | `best-practices.md` | UI Toolkit over UGUI | Rule dependency |

---

## Acceptance Criteria

- [ ] HUD (score, lives, level) renders in UI Toolkit and updates on the corresponding events.
- [ ] Power-up pick screen appears on `OnPowerUpOffered`, relays the player's choice to PowerUpSystem, and closes.
- [ ] Game-over and victory screens appear on `OnRunEnded` with the final score.
- [ ] Restart fires `OnRestartRequested` once; GameManager performs the reload.
- [ ] No UGUI Canvas anywhere; all UI is `.uxml` + `.uss`.
- [ ] UIManager makes no gameplay calls; it only displays state and relays UI intent.
- [ ] Events arriving before bind don't error; UI reflects current state once bound.
- [ ] No `Find`/`FindObjectOfType` for gameplay; subscribes via the contract.
- [ ] Performance: UI updates are event-driven (not per-frame polling); no steady-state GC from HUD updates.

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Does MainMenu's "Play" button live in UIManager (HUD scene) or a separate MainMenu UI? Ties to "who calls `StartRun()`" (GameManager Open Q). | designer / Claude | Before implementation | Leaning: MainMenu has its own small UI doc; UIManager focuses on HUD + pick + end screens. Resolve with the scene-flow decision. |
| Lives shown as icons or a number? | designer | Phase 3 feel | Leaning: icons (arcade feel); trivial to switch in `.uss`. |
| Is there a pause menu in v1? (Not in systems-design.) | designer | Before implementation | Leaning: no pause menu for v1 (arcade run); just HUD + end screens. |
| Combo meter UI — include now or gate on the ScoreSystem combo decision? | designer | With ScoreSystem combo decision | Pending — hidden if combos are cut. |
