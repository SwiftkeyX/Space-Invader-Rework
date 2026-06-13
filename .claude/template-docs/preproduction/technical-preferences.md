# Technical Preferences

<!-- Fill this out during pre-production. All agents reference this file for project-specific standards and conventions. -->
<!-- Update whenever a technical decision is locked in. -->

## Engine & Language

| Field | Value |
|---|---|
| **Engine** | *(e.g. Unity 6 LTS)* |
| **Language** | C# |
| **Rendering** | *(e.g. URP, HDRP, Built-in)* |
| **Physics** | *(e.g. Unity 2D Physics / Box2D, Unity 3D Physics / PhysX)* |

## Input & Platform

| Field | Value |
|---|---|
| **Target Platforms** | *(e.g. PC, WebGL, Android)* |
| **Input Methods** | *(e.g. Keyboard/Mouse, Gamepad, Touch)* |
| **Primary Input** | *(e.g. Keyboard/Mouse)* |
| **Gamepad Support** | *(Full / Partial / None)* |
| **Touch Support** | *(Full / Partial / None)* |

**Platform Notes**

*(Document any input constraints that affect architecture. e.g. "All gameplay-critical actions must have both keyboard and gamepad bindings — no mouse-only interactions.")*

## Performance Budgets

Set these after your first profiling pass. Leave as TBD until then.

| Budget | Target | Notes |
|---|---|---|
| **Target Framerate** | *(e.g. 60fps)* | |
| **Frame Budget** | *(e.g. 16.6ms)* | Derived from framerate |
| **Draw Calls** | *(TBD — set after first profiling pass)* | 2D sprite games typically ~50–150 |
| **Memory Ceiling** | *(TBD — set after first profiling pass)* | |
| **GC Alloc / Frame** | Zero in steady state | Allocations cause frame spikes |

## Testing

| Field | Value |
|---|---|
| **Framework** | NUnit (Unity Test Runner) |
| **Test types** | Edit Mode (pure logic), Play Mode (scene/runtime) |
| **Minimum Coverage** | *(e.g. all gameplay systems, all state machine transitions, all formulas)* |

**Required Tests** — list specific systems that must have tests before shipping:

- *(e.g. Core loop win/lose state transitions)*
- *(e.g. Damage formula)*
- *(e.g. Scoring calculation)*

## Forbidden Patterns

<!-- Tech-stack level rules: banned Unity subsystems, external APIs, or third-party libraries. -->
<!-- NOT for code style anti-patterns (those live in coding-style.md). -->
<!-- Format: brief name — reason -->

- *(None configured yet — add as decisions are made)*

## Allowed Libraries / Addons

<!-- Only add when actively integrating. Do not add speculatively. -->
<!-- Format: Package name — purpose — how approved -->

- *(None configured yet — add as dependencies are approved)*

## Architecture Decisions Log

<!-- Quick reference linking to full ADR files in docs/other/adr/. -->
<!-- Create a new ADR file for every significant technical decision. -->
<!-- Format: [Short title](../other/adr/adr-NNN-title.md) — one-line summary -->

- *(No ADRs yet — create docs/other/adr/adr-001-*.md for your first decision)*

## Agent / Specialist Routing

<!-- Defines which Claude Code agent or skill to invoke per task type. -->
<!-- Update when you add new skills or identify tasks that need specialized handling. -->

| Task Type | Agent / Skill | Notes |
|---|---|---|
| General C# scripts, scene wiring | `gameplay-programmer` | Default for most Unity work |
| Architecture review, code audit | `technical-director` | Read-only — advises, does not implement |
| Shader / material work | *(e.g. custom skill or manual)* | |
| UI implementation | `gameplay-programmer` | Specify UI Toolkit vs UGUI in task |
| Asset loading / Addressables | `gameplay-programmer` | |
| Security review | `/security-review` skill | |

### File Extension Routing

| File Type | Agent to Use |
|---|---|
| `.cs` game scripts | `gameplay-programmer` |
| `.shader`, `.shadergraph`, `.mat` | *(define)* |
| `.uxml`, `.uss`, Canvas prefabs | `gameplay-programmer` |
| `.unity`, `.prefab` | `gameplay-programmer` (via coplay MCP) |
| Architecture review | `technical-director` |
