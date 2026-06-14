# InputManager

> **Status**: Draft
> **Last Updated**: 2026-06-13
> **Implements Pillar**: Foundation for "Fun / Chaotic" — tight, responsive controls so the player can always react to the on-screen chaos.

## Summary

InputManager is the single read point for player input. It reads keyboard + mouse via the Unity Input System and exposes intent — a horizontal move axis and a fire intent — to gameplay systems (PlayerShip). It translates raw devices into game-meaningful values so no gameplay script ever touches `Input.GetKey` or device APIs directly.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `None` (read by PlayerShip)

---

## Overview

The player controls a horizontally-moving ship that fires upward (D1). InputManager owns the mapping from physical input (A/D or ←/→ keys, and a fire button — left mouse and/or a key) to two pieces of intent: *how the player wants to move along X* and *whether the player wants to fire*. It exposes these as polled properties and/or a fire event, and PlayerShip reads them. Keeping input in one place makes the scheme rebindable, keeps gameplay code device-agnostic, and enforces the horizontal-only invariant at the source — there is no vertical axis to read.

## Player Fantasy

The player should feel the ship is a direct extension of their hand — instant, precise, no lag and no drift. In a screen full of bullets (Chaotic pillar), control fidelity is what makes the chaos feel *fair* rather than frustrating. The fantasy: *if I die, it was my read, not the controls.*

---

## Detailed Design

### Core Rules

1. InputManager uses the **Unity Input System** package (`UnityEngine.InputSystem`), never the legacy `Input` class (`best-practices.md`).
2. Input is defined in an **Input Action Asset** with a single `Gameplay` action map containing:
   - `Move` — a 1D axis (Value/Axis) bound to A/D and ←/→. Output range −1..+1 (left..right). **There is no vertical axis** (D1/N1 — horizontal-only invariant).
   - `Fire` — a Button action bound to left mouse button and Spacebar.
3. InputManager exposes intent to gameplay as:
   - `MoveAxis` (float, −1..+1, polled property) — the current horizontal intent.
   - `FireHeld` (bool, polled property) — true while a fire input is held (for auto/held-fire weapons).
   - `OnFirePressed` (C# event) — fires once on the frame fire is first pressed (for single-shot/tap weapons).
4. PlayerShip reads `MoveAxis`/`FireHeld` and/or subscribes to `OnFirePressed`. InputManager never knows about PlayerShip or any gameplay object — input is strictly one-directional (input → reader).
5. InputManager performs **no gameplay logic**: no movement, no spawning bullets, no rate-limiting fire. It reports intent; PlayerShip decides what to do with it (movement speed, fire rate are PlayerShip/weapon concerns).
6. Input is read each frame in `Update`; `MoveAxis`/`FireHeld` reflect the latest device state. `OnFirePressed` is raised on the press edge only.
7. Platform is PC keyboard + mouse only for v1 (N5). No gamepad or touch bindings required, though the Input Action Asset structure leaves room to add a control scheme later without changing gameplay code.
8. When gameplay is not active (menus, game-over, paused), input intent should be suppressed or ignored by the reader — InputManager may expose an `InputEnabled` flag GameManager/UIManager toggles, or PlayerShip simply stops reading. (See Open Questions.)

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Enabled` | Gameplay active | Gameplay paused / run ended / in menu | Reads devices; updates `MoveAxis`/`FireHeld`; raises `OnFirePressed` |
| `Disabled` | Not in gameplay | Gameplay (re)starts | `MoveAxis` reports 0, `FireHeld` false, no `OnFirePressed` |

> If suppression is handled by the reader rather than InputManager, this table collapses to a single always-reading state — resolved in Open Questions.

### Interactions with Other Systems

| System | Interaction |
|---|---|
| PlayerShip | Reads `MoveAxis`/`FireHeld`; subscribes to `OnFirePressed`. Data out: intent values. Data in: none — input never reads ship state. |
| GameManager / UIManager | May toggle `InputEnabled` so input is dead on menus / game-over (TBD — see Open Questions). |

---

## Formulas

No formulas. `MoveAxis` is the raw Input System axis value (−1..+1); any smoothing/acceleration is PlayerShip's choice, not InputManager's.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Both left and right pressed simultaneously | Axis resolves to 0 (Input System default opposing-key cancel) | Predictable neutral rather than last-pressed jitter. |
| Fire held across multiple frames | `FireHeld` stays true; `OnFirePressed` fires only on the initial press | Lets readers choose held-fire vs tap-fire semantics. |
| Window loses focus mid-hold | Input releases (no stuck `FireHeld`/`MoveAxis`) | Prevents a stuck-input "ghost" run when the player alt-tabs. |
| Input read during pause/game-over | Intent reads neutral (0 / false) | Player can't move a dead/paused ship; protects the run-end state. |
| Key rebinding (future) | Reader code unchanged because it reads intent, not keys | The whole reason input is centralized. |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| PlayerShip | It depends on this | Data dependency — reads move/fire intent |
| Unity Input System package | This depends on it | Data dependency — devices + Action Asset |
| GameManager / UIManager | (Optional) it depends on this | State trigger — toggles `InputEnabled` outside gameplay |

> Nature options: `Data dependency` · `State trigger` · `Rule dependency` · `Ownership handoff`

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| Key bindings (Move/Fire) | A/D + ←/→ ; LMB + Space | n/a (rebind list) | Adds alternate keys | Removes alternates |
| `AxisDeadzone` (if smoothing added) | 0 | 0–0.2 | More tolerance for stray axis input | More sensitive |

> Input has minimal balance tunables — feel knobs like move speed and fire rate live in **PlayerShip**, not here. Bindings live in the Input Action Asset (Inspector-editable). InputManager intentionally exposes raw intent so the feel surface stays in one place (PlayerShip).

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| (none) | — | — | InputManager produces no feedback itself |

> Fire/move feedback (muzzle flash, thruster, SFX) is owned by PlayerShip/AudioManager off the *actions* input triggers, not by InputManager.

---

## Game Feel

### Feel Reference

> "Controls should feel like *Galaga / Ikaruga* lateral movement — instant, 1:1, no input lag or analog mush. NOT a physics-y ship with momentum drift you fight against." (Momentum, if any, is PlayerShip's deliberate choice, not an input artifact.)

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Move key → `MoveAxis` updated | ≤ 1 frame (same-frame read in Update) | 1 frame |
| Fire press → `OnFirePressed` raised | ≤ 1 frame | 1 frame |

### Animation Feel Targets

Not applicable — InputManager owns no animation.

### Impact Moments

None — InputManager is pure input plumbing.

### Weight and Responsiveness

- **Weight**: Weightless — input intent is reported instantly with no buffering.
- **Player control**: Maximum — this system exists to give the player crisp control.
- **Snap quality**: Crisp and immediate; any smoothing is a deliberate PlayerShip decision, not added here.
- **Failure texture**: Deaths must read as the player's misjudgment, never as dropped or laggy input.

### Feel Acceptance Criteria

- [ ] No playtester describes the controls as "laggy," "floaty," or "drifting."
- [ ] Movement direction changes register on the same frame the key state changes.

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| (none) | — | — | A rebinding screen is out of scope for v1 |

> A controls/rebind UI is future work (the Action Asset supports it); not required for v1.

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Who consumes input | `PlayerShip.md` | `MoveAxis`, `FireHeld`, `OnFirePressed` | Data dependency |
| Horizontal-only invariant | `design-decisions.md` | D1 / N1 (no vertical axis) | Rule dependency |
| Input System usage | `best-practices.md` | Input System over legacy `Input` | Rule dependency |
| Input suppression off-gameplay | `GameManager.md` | run state gating | State trigger |

---

## Acceptance Criteria

- [ ] An Input Action Asset exists with a `Gameplay` map containing a 1D `Move` axis (A/D + ←/→) and a `Fire` button (LMB + Space).
- [ ] No vertical/second movement axis exists anywhere (D1/N1 enforced at the source).
- [ ] `MoveAxis` returns −1..+1 reflecting current horizontal intent; `FireHeld` reflects held state; `OnFirePressed` fires once per press.
- [ ] PlayerShip reads intent through InputManager only — no gameplay script references the legacy `Input` class or devices directly.
- [ ] Losing window focus releases all inputs (no stuck movement/fire).
- [ ] Input reads neutral outside active gameplay (menus / game-over / pause).
- [ ] Performance: per-frame input read is allocation-free (no GC) and completes well within the 8.3 ms @120fps budget.
- [ ] No hardcoded key checks — all bindings live in the Input Action Asset.

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Is fire **held/auto** (use `FireHeld`) or **tap/single-shot** (use `OnFirePressed`) for the base weapon? Power-ups may change this. | designer | Before PlayerShip implementation | Leaning: expose both; base weapon uses tap with a PlayerShip-side fire-rate cap, power-ups can switch to held. |
| Does InputManager own an `InputEnabled` flag, or does PlayerShip just stop reading off-gameplay? | Claude | Before implementation | Leaning: InputManager exposes `InputEnabled` toggled by GameManager run state — single chokepoint, cleaner than per-reader gating. |
| Fire on mouse position or fixed upward? (D1 implies upward fire; mouse may only be the fire button, not an aim vector.) | designer | Before PlayerShip GDD finalize | Leaning: fire is straight up; mouse is fire button only (no aim) for the Space Invaders identity. |
