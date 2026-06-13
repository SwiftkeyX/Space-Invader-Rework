# PIPELINE.md

## Phase 1 — Pre-production

- [x] Fill out `docs/design/game-vision.md`
- [x] Fill out `docs/design/design-decisions.md`
- [x] Fill out `docs/technical/technical-preferences.md` (engine, platform, performance budgets)
- [x] Fill out `docs/design/systems-design.md` — list every system, tier, and dependencies
- [ ] Fill out `docs/technical/architecture.md` with finalized script table
- [ ] Fill out `docs/technical/best-practices.md` — add project-critical patterns section
- [ ] Milestone 0 — vision complete, all systems tiered, architecture and tech stack finalized

## Phase 2 — Production

### Tier 1 — Foundation
- [ ] Create `docs/design/technical-design/<system>.md` for each Tier 1 system (copy `_template.md`)
- [ ] *(fill in your Tier 1 systems)*

### Tier 2 — Core Loop
- [ ] Create `docs/design/technical-design/<system>.md` for each Tier 2 system (copy `_template.md`)
- [ ] *(fill in your Tier 2 systems)*

- [ ] Milestone 1 — core loop playable end-to-end

### Tier 3 — Supporting Systems
- [ ] Create `docs/design/technical-design/<system>.md` for each Tier 3 system (copy `_template.md`)
- [ ] *(fill in your Tier 3 systems)*

- [ ] Milestone 2 — all features in, content complete

## Phase 3 — Beta

- [ ] Juice pass — screen shake, particles, hit-stop, SFX, music, UI animations
- [ ] Feel tuning — tweak values via ScriptableObjects/Inspector
- [ ] Difficulty tuning — curve, pacing, escalation
- [ ] Bug pass — all known issues fixed (`docs/process/known-issues.md` clear)
- [ ] Performance pass — GC allocs and frame rate within budgets (`docs/technical/technical-preferences.md`)
- [ ] Ship — final build, smoke test, release (`docs/process/build-notes.md` checklist)
