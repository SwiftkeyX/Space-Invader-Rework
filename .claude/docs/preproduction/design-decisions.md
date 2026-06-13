# Design Decisions

Key decisions derived from `game-vision.md`. Each entry records **what** was decided and **why**, so later phases don't relitigate settled choices. Core-mechanic constraints here are the source for the GDD's "core mechanic constraint" routing (see `doc-conventions.md`).

## Core Mechanic Constraints (what the game IS)

| # | Decision | Why |
|---|---|---|
| D1 | **Horizontal-only player movement.** Player is locked to a bottom strip, moves left/right. | Preserves Space Invaders identity. Dodging is about reading and timing gaps in enemy fire, not free-roam evasion. Keeps the "rework" recognizable. |
| D2 | **Lives-based survival: 3 lives + brief i-frames on hit.** Run ends at 0 lives. | Classic arcade feel; gives breathing room for the bullet-hell density without the brutality of one-hit death. I-frames prevent multi-hit chain-deaths in dense patterns. |
| D3 | **Pure run-based roguelike, no meta-progression.** Every run starts identical; power-ups and weapon mods last only the current run. | Smallest honest scope for the roguelike layer. Skill-driven — keeps the "one more run" pull without a persistence/save system. |
| D4 | **Between-level power-up / weapon-mod choice.** Player picks an upgrade between levels (guaranteed offered by level 3). | This is the roguelike layer and the "power fantasy on a timer" from the vision. Drives build variety across runs. |
| D5 | **Six fixed levels, short & steep curve.** Escalation on HP, threat speed, and fire density (see game-vision Difficulty Curve). | Matches the "short & steep, 10–15 min full clear" vision. Fixed count keeps tuning tractable for a single dev. |
| D6 | **Heavy juice is a first-class feature, not polish.** Screen shake, particles, hit-stop, punchy SFX on every kill. | Directly serves the **Explosive** pillar. Deferred to Phase 3 (juice pass) but reserved in architecture now. |

## Explicit Non-Goals (what the game is NOT)

| # | Non-goal | Why |
|---|---|---|
| N1 | **No free 2-axis / vertical player movement.** | Rejected in favor of D1. A free-roam dodger would stop being Space Invaders. |
| N2 | **No destructible bunkers/shields.** | Cut for a cleaner bullet-hell read and reduced scope. Cover is positional timing, not static walls. |
| N3 | **No meta-progression, unlocks, or persistent save.** | Rejected in favor of D3. Out of scope for v1. |
| N4 | **No multiplayer / co-op.** | Single-player arcade focus. Not supported by the vision. |
| N5 | **No mobile/touch or gamepad build for v1.** | Platform decision is PC keyboard+mouse only. Other schemes are future work, not v1. |
| N6 | **No narrative/story campaign.** | Arcade score-and-survival loop only. |
| N7 | **No endless mode for v1.** | The curve is a fixed 6-level run; endless was explicitly not chosen. |

## Non-Obvious Choices (justification over alternatives)

- **Lives over one-hit death (D2):** one-hit maximizes tension but, combined with the level-4+ bullet density, would make the curve feel unfair and spike restart-rage. Three lives + i-frames keeps "tense but fair." If Phase 3 feel-tuning finds it too soft, lives count is the knob to turn — not the movement scheme.
- **Horizontal-only over a dodge-dash (D1):** a dash was tempting for bullet-hell, but it pulls the feel toward a modern twin-stick dodger and complicates the control scheme. Tension is instead delivered through fire density and i-frame management.
- **No bunkers (N2):** classic bunkers reward camping, which fights the **Chaotic** pillar (we want the player moving and reacting). Cutting them also removes a destructible-geometry system from scope.

## Scope Boundaries (in vs. out for v1)

**In scope (v1):**
- Horizontal player ship, shooting, 3 lives + i-frames
- Marching invader formations with HP tiers, enemy fire patterns scaling to bullet-hell density
- 6 fixed levels with the game-vision difficulty ramp
- Between-level power-up / weapon-mod selection (run-scoped)
- Score system + game-over/restart loop
- Full juice pass (Phase 3): shake, particles, hit-stop, SFX, music, UI animation
- PC keyboard+mouse build

**Out of scope (v1):** everything in Non-Goals — free movement, bunkers, meta-progression, multiplayer, narrative, endless mode, non-PC platforms.

## Open Questions (defer to Phase 2 design)

- Exact power-up / weapon-mod catalog and effect durations (route through the relevant GDD when designed).
- Whether scoring includes combo multipliers (leans toward yes given **Chaotic/Explosive**, but not yet committed).
