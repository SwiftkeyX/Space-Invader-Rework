# Design Decisions

> Terminology and constraints that Claude must apply consistently throughout the codebase.
> For core loop, win/lose, and mechanics — those live in `game-vision.md` (the source of truth).
> Add an entry here only when a decision is finalized AND it affects how Claude names or constrains things in code.

## Terminology

Define canonical names so Claude uses them consistently. Every term here overrides any synonym.

| Term | Means | Not |
|---|---|---|
| *(e.g. Vessel)* | *(the player-controlled object)* | *(not "character", "hero", or "player")* |

## Boundaries & Constraints

Spatial and mechanical limits that affect implementation decisions.

| Constraint | Value | Notes |
|---|---|---|
| *(e.g. Play area width)* | *(e.g. ±10 world units)* | *(e.g. camera is fixed; anything outside is OOB)* |
