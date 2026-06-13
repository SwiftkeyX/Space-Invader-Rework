# Systems Design

> List every system in the game, its single responsibility, what it depends on, and which tier it belongs to. Fill this out as part of Phase 1 before writing any code.

## Systems Table

| System | Responsibility | Depends On | Tier |
|---|---|---|---|
| GameManager | Owns run state — lives, current level index, run start/end; singleton living in Bootstrap | None | 1 |
| SceneLoader | Loads/unloads Bootstrap · MainMenu · GameLogic · HUD scenes (no direct `SceneManager.LoadScene`) | None | 1 |
| InputManager | Reads keyboard+mouse, exposes move/fire intents to gameplay systems | None | 1 |
| PlayerShip | Horizontal-only movement, firing, 3 lives + i-frames on hit (D1, D2) | InputManager, GameManager, Projectile | 2 |
| Projectile | Player & enemy bullet movement, lifetime, and collision/damage dealing | None | 2 |
| EnemyFormation | Marching invader grid — movement, descent, formation state | GameManager | 2 |
| Enemy | Single invader: HP tiers, death, points value | EnemyFormation, Projectile | 2 |
| EnemyFireController | Selects shooters and fire patterns, scales density per level | EnemyFormation, Projectile, LevelManager | 2 |
| LevelManager | Drives the 6 fixed levels, applies HP/speed/fire-rate scaling, level clear → next (D5) | GameManager, EnemyFormation | 2 |
| ScoreSystem | Tracks score (combo multiplier TBD) and per-kill points | Enemy | 3 |
| PowerUpSystem | Between-level weapon-mod / power-up selection, run-scoped effects (D3, D4) | GameManager, PlayerShip, LevelManager | 3 |
| HUD/UIManager | Score, lives, level display; power-up pick screen; game-over/restart UI | GameManager, ScoreSystem, PowerUpSystem | 3 |
| AudioManager | Music + SFX playback hooks for game events | GameManager | 3 |
| JuiceManager | Screen shake, particles, hit-stop — reserved now, built in Phase 3 (D6) | GameManager | 3 |

## Tier Definitions

| Tier | Label | Must work before… |
|---|---|---|
| 1 | Foundation | Any gameplay can be tested |
| 2 | Core Loop | Win/lose is reachable end-to-end |
| 3 | Supporting | Content is complete and game is shippable |

## Notes

- **Collision/damage** is intentionally folded into Projectile + Enemy + PlayerShip rather than a standalone system — simpler for this scope.
- **Game-over / restart flow** lives in GameManager (state) + HUD/UIManager (presentation), not a separate system.
- **JuiceManager** is reserved in the architecture now per D6 but is not implemented until the Phase 3 juice pass.
- Open from `design-decisions.md`: combo-multiplier scoring (affects ScoreSystem) and the exact power-up catalog (affects PowerUpSystem) are deferred to Phase 2 GDDs.
