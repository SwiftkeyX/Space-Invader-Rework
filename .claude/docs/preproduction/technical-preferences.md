# Technical Preferences

<!-- Fill this out during pre-production. All agents reference this file for project-specific standards and conventions. -->
<!-- Update whenever a technical decision is locked in. -->

## Engine & Language

| Field | Value |
|---|---|
| **Engine** | Unity 6 LTS |
| **Language** | C# |
| **Rendering** | URP (Universal Render Pipeline, 2D Renderer) |
| **Physics** | Unity 2D Physics (Box2D) |

## Input & Platform

| Field | Value |
|---|---|
| **Target Platforms** | PC (Windows desktop) |
| **Input Methods** | Keyboard / Mouse |
| **Primary Input** | Keyboard / Mouse |
| **Gamepad Support** | None (v1) |
| **Touch Support** | None (v1) |

**Platform Notes**

- Single-player, PC keyboard+mouse only for v1 (see `design-decisions.md` N5). No gamepad or touch bindings required.
- Built on the **New Input System** package — all gameplay actions go through an Input Action Asset, not legacy `UnityEngine.Input`. Keep action maps minimal: Move (horizontal), Fire, Pause/UI.

## Performance Budgets

| Budget | Target | Notes |
|---|---|---|
| **Target Framerate** | 120 fps | High-refresh target to match the fast, twitchy feel. |
| **Frame Budget** | 8.3 ms | Derived from 120 fps. |
| **Draw Calls** | TBD — set after first profiling pass | 2D sprite game; expect ~50–150. Bullet-hell density means heavy sprite/particle batching — favor sprite atlases + batching. |
| **Memory Ceiling** | TBD — set after first profiling pass | |
| **GC Alloc / Frame** | Zero in steady state | Allocations cause frame spikes; critical at 120 fps. Pool bullets/enemies/particles — no per-frame `new`. |

## Testing

| Field | Value |
|---|---|
| **Framework** | NUnit (Unity Test Runner) |
| **Test types** | Edit Mode (pure logic), Play Mode (scene/runtime) |
| **Minimum Coverage** | All gameplay state-machine transitions, all difficulty-scaling formulas, scoring calculation |

**Required Tests** — list specific systems that must have tests before shipping:

- Core loop win/lose state transitions (level clear → next level; 0 lives → game over → restart)
- Difficulty-scaling formulas (per-level HP, threat speed, and fire-rate ramps from `game-vision.md`)
- Scoring calculation (and combo multiplier if adopted)
- Lives / i-frame logic (hit registration, invulnerability window)

## Forbidden Patterns

<!-- Tech-stack level rules: banned Unity subsystems, external APIs, or third-party libraries. -->

- **`DontDestroyOnLoad()`** — banned. Persistent objects live in `Bootstrap.unity` instead (see `unity-editor.md`).
- **`SceneManager.LoadScene` / `LoadSceneMode.Single` (direct)** — banned. All scene loads go through `SceneLoader`.
- **Legacy Input Manager (`UnityEngine.Input`)** — banned. Use the New Input System action assets.
- **Per-frame heap allocation** in gameplay hot paths (LINQ in `Update`, boxing, `new` for bullets/enemies/effects) — banned by the zero-GC budget. Use object pools.
- **Single-scene god layout** — banned. Follow the Bootstrap / MainMenu / GameLogic / HUD scene split.

## Allowed Libraries / Addons

<!-- Only add when actively integrating. Do not add speculatively. -->

- **Input System** (`com.unity.inputsystem`) — primary input — approved (core decision).
- **Universal RP** (`com.unity.render-pipelines.universal`) — rendering + 2D post-FX for juice — approved (core decision).

## Architecture Decisions Log

<!-- Quick reference linking to full ADR files in docs/other/adr/. -->

- *(No ADRs yet — create `docs/other/adr/adr-001-*.md` for the first significant technical decision, e.g. object-pooling strategy.)*

## Agent / Specialist Routing

| Task Type | Agent / Skill | Notes |
|---|---|---|
| General C# scripts, scene wiring | `gameplay-programmer` | Default for most Unity work |
| Architecture review, code audit | `technical-director` | Read-only — advises, does not implement |
| Shader / material work | Manual (no dedicated agent) | URP 2D shader graphs hand-authored; rare in v1 |
| UI implementation | `ui-programmer` | UGUI Canvas for HUD/menus (specify in task) |
| Audio (SFX/music) | `audio-engineer` | Juice pass — generates and wires clips |
| Level / brick-grid layout | `level-designer` | Invader formation layouts, level data |
| Asset loading / Addressables | `gameplay-programmer` | |
| Security review | `/security-review` skill | |

### File Extension Routing

| File Type | Agent to Use |
|---|---|
| `.cs` game scripts | `gameplay-programmer` |
| `.shader`, `.shadergraph`, `.mat` | Manual |
| `.uxml`, `.uss`, Canvas prefabs | `ui-programmer` |
| `.unity`, `.prefab` | `gameplay-programmer` (via coplay MCP) |
| Architecture review | `technical-director` |
