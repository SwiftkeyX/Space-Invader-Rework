# PIPELINE.md

## Phase 1 — Pre-production

- [x] Fill out `docs/design/game-vision.md`
- [x] Fill out `docs/design/design-decisions.md`
- [x] Fill out `docs/technical/technical-preferences.md` (engine, platform, performance budgets)
- [x] Fill out `docs/design/systems-design.md` — list every system, tier, and dependencies
- [x] Fill out `docs/technical/architecture.md` with finalized script table
- [x] Fill out `docs/technical/best-practices.md` — add project-critical patterns section
- [x] Milestone 0 — vision complete, all systems tiered, architecture and tech stack finalized

## Phase 2 — Production

> Each system gets a GDD at `.claude/docs/production/gdd/<System>.md` (Sub-phase A), then is implemented (Sub-phase B). Work in tier order.

- [x] Milestone 1 — all system GDDs written and approved (Sub-phase A complete; coding is locked until this is checked)

### Tier 1 — Foundation

| System | GDD written | Implemented |
|---|---|---|
| GameManager | [x] | [x] |
| SceneLoader | [x] | [x] |
| InputManager | [x] | [x] |

- [x] 🧪 Test Gate 1 — Foundation boots: scenes load (Bootstrap → MainMenu → GameLogic → HUD), input registers, console clean

### Tier 2 — Core Loop

| System | GDD written | Implemented |
|---|---|---|
| PlayerShip | [x] | [x] |
| Projectile | [x] | [x] |
| EnemyFormation | [x] | [x] |
| Enemy | [x] | [x] |
| EnemyFireController | [x] | [x] |
| LevelManager | [x] | [x] |

- [x] 🧪 Test Gate 2 — Base game playable end-to-end: player moves + shoots, enemies march & die, win/lose triggers, console clean
- [x] Milestone 2 — core loop playable end-to-end

### Tier 3 — Supporting Systems

| System | GDD written | Implemented |
|---|---|---|
| ScoreSystem | [x] | [x] |
| PowerUpSystem | [x] | [x] |
| HUD/UIManager | [x] | [x] |
| AudioManager | [x] | [x] |
| JuiceManager | [x] | [ ] |

- [x] 🧪 Test Gate 3 — Full content playthrough: score, HUD, power-ups, audio function across a level, console clean
- [x] Milestone 3 — all features in, content complete
- [ ] 🏛️ Architecture pass — refactor code to match GDDs via the PR-review loop until a technical-director audit is clean (or ditch the game if the idea didn't pan out)

> Note: JuiceManager is reserved here but implemented in Phase 3 (per D6 / systems-design.md).
> The Architecture pass is the Phase 2 → Phase 3 bridge — run `/architecture-pass`. Phase 3 is locked until it is `[x]`.

## Phase 3 — Beta

- [ ] Juice pass — screen shake, particles, hit-stop, SFX, music, UI animations
- [ ] Feel tuning — tweak values via ScriptableObjects/Inspector
- [ ] Difficulty tuning — curve, pacing, escalation
- [ ] Bug pass — all known issues fixed (`docs/process/known-issues.md` clear)
- [ ] Performance pass — GC allocs and frame rate within budgets (`docs/technical/technical-preferences.md`)
- [ ] Ship — final build, smoke test, release (`docs/process/build-notes.md` checklist)
