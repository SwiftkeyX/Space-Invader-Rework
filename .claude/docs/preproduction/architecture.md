# Architecture Contract

> The authoritative record of how scripts in this project communicate. Claude reads this before touching any existing script.
>
> For system responsibilities, dependencies, and tier assignments, see `.claude/docs/preproduction/systems-design.md`.
> For coding conventions and anti-patterns, see `best-practices.md`.

## Communication Principles

- **GameManager** is the only singleton (lives in `Bootstrap.unity`, accessed via a static `Instance`). No other system uses `DontDestroyOnLoad`.
- **Loose coupling by default:** systems announce facts via C# events; listeners subscribe. A system never reaches "up" to query global state ad-hoc.
- **Direct method calls** are permitted only along the foundational, one-directional edges listed below (e.g. a spawned bullet damaging the thing it hit). They are never used for cross-tier state queries.
- **No `Find` / `FindObjectOfType` chains.** References are assigned in the Inspector, passed at spawn, or resolved through `GameManager`.
- Scene changes go through **SceneLoader** only — never `SceneManager.LoadScene` directly.

## Per-System Contract

### Tier 1 — Foundation

| Script | Responsibility | Fires | Listens to | Direct refs permitted |
|---|---|---|---|---|
| `GameManager.cs` | Owns run state (lives, level index, run start/end) as the sole singleton; also the cross-scene reference hub (exposes `InputManager` so GameLogic systems resolve it without `Find`) | `OnRunStarted`, `OnRunEnded`, `OnLivesChanged`, `OnLevelChanged` | `PlayerShipContext.OnPlayerDeath`, `LevelManager.OnLevelCleared` | `SceneLoader` (request scene loads), `InputManager` (held + exposed) |
| `SceneLoader.cs` | Loads/unloads Bootstrap · MainMenu · GameLogic · HUD scenes | `OnSceneLoaded` | — | — (called by GameManager) |
| `InputManager.cs` | Reads keyboard+mouse, exposes move/fire intents | `OnFirePressed` (or polled `MoveAxis`/`FireHeld` properties) | — | — (read by PlayerShip) |

### Tier 2 — Core Loop

| Script | Responsibility | Fires | Listens to | Direct refs permitted |
|---|---|---|---|---|
| `PlayerShipContext.cs` | Coordinator: thin Update() delegates to `PlayerShipState` (i-frames), `PlayerShipStat` (numeric stats), `Weapon` (fire logic). Horizontal movement + IDamageable hit surface. | `OnPlayerHit`, `OnPlayerDeath` | `InputManager`, `PowerUpSystem.OnPowerUpChosen` | Spawns `Projectile` via `Weapon` (player bullets); `PowerUpSystem` wired via Inspector |
| `Projectile.cs` | Bullet movement, lifetime, collision/damage dealing | — | — | Calls `IDamageable.TakeDamage` on collision (team-filtered) |
| `EnemyFormation.cs` | Marching invader grid — movement, descent, formation state | `OnFormationCleared` | `LevelManager.OnLevelStarted`, `Enemy.OnEnemyKilled` | Owns/spawns its `Enemy` children |
| `Enemy.cs` | Single invader: HP tiers, death, points value | `OnEnemyKilled` | — | — (damaged by `Projectile`) |
| `EnemyFireController.cs` | Selects shooters and fire patterns, scales density per level | — | `LevelManager.OnLevelStarted` | Reads `EnemyFormation` (shooter set); spawns `Projectile` (enemy bullets) |
| `LevelManager.cs` | Drives the 6 fixed levels, applies HP/speed/fire-rate scaling | `OnLevelStarted`, `OnLevelCleared` | `EnemyFormation.OnFormationCleared`, `GameManager.OnLevelChanged` | Reads `GameManager` level index |

### Tier 3 — Supporting

| Script | Responsibility | Fires | Listens to | Direct refs permitted |
|---|---|---|---|---|
| `ScoreSystem.cs` | Tracks score and per-kill points | `OnScoreChanged` | `Enemy.OnEnemyKilled` | — |
| `PowerUpSystem.cs` | Between-level weapon-mod / power-up selection (run-scoped) | `OnPowerUpOffered`, `OnPowerUpChosen` | `LevelManager.OnLevelCleared` | Applies effects to `PlayerShip` |
| `UIManager.cs` | HUD (score/lives/level), power-up pick screen, game-over/restart UI | `OnRestartRequested` | `GameManager.*`, `ScoreSystem.OnScoreChanged`, `PowerUpSystem.OnPowerUpOffered` | — (presentation only, no gameplay calls) |
| `AudioManager.cs` | Music + SFX playback for game events | — | `Enemy.OnEnemyKilled`, `PlayerShipContext.OnPlayerHit`, `LevelManager.*`, `GameManager.*` | — |
| `JuiceManager.cs` | Screen shake, particles, hit-stop (reserved; built Phase 3, D6) | — | `Enemy.OnEnemyKilled`, `PlayerShipContext.OnPlayerHit`, `LevelManager.OnLevelCleared` | Camera / particle pools |

## Communication Patterns (authoritative table)

| From | To | Method | Notes |
|---|---|---|---|
| InputManager | PlayerShipContext | Polled properties / `OnFirePressed` event | Player reads intent; input never knows about the ship |
| PlayerShipContext | Projectile | Direct spawn (via `Weapon`) | Instantiates player bullets |
| EnemyFireController | Projectile | Direct spawn | Instantiates enemy bullets |
| Projectile | IDamageable (Enemy / PlayerShipContext) | `IDamageable.TakeDamage(damage)` — team-filtered | Only on physical collision; the only "downward" direct call |
| Enemy | EnemyFormation, ScoreSystem, AudioManager, JuiceManager | `OnEnemyKilled` event | One kill, many independent reactions |
| PlayerShipContext | GameManager, AudioManager, JuiceManager | `OnPlayerHit` / `OnPlayerDeath` events | GameManager decrements lives; others react |
| GameManager | UIManager, AudioManager, LevelManager | `OnLivesChanged` / `OnLevelChanged` / `OnRunStarted` / `OnRunEnded` events | Single source of run state |
| GameManager | SceneLoader | Direct method call | Requests scene transitions (the only scene-load path) |
| EnemyFormation | LevelManager | `OnFormationCleared` event | Level advances when the formation is empty |
| LevelManager | GameManager | `OnLevelCleared` event → `HandleLevelCleared()` | GameManager advances the level index / wins the run at level 6 (the only LevelManager→GameManager edge) |
| GameManager | PlayerShipContext (and other GameLogic systems) | Exposes `Input` (InputManager) via `GameManager.Instance` | Cross-scene reference resolution hub — avoids `Find` for Bootstrap-resident services |
| LevelManager | EnemyFormation, EnemyFireController | `OnLevelStarted` event (carries level config) | Spawns/configures the wave for the level |
| LevelManager | PowerUpSystem, AudioManager, JuiceManager, UIManager | `OnLevelCleared` event | Triggers between-level power-up offer + payoff |
| ScoreSystem | UIManager | `OnScoreChanged` event | HUD reflects score |
| PowerUpSystem | UIManager | `OnPowerUpOffered` event | Shows the pick screen |
| PowerUpSystem | PlayerShipContext | `OnPowerUpChosen` event → `ApplyPowerUp()` (Inspector-wired, no Find) | Applies the chosen run-scoped effect |
| UIManager | GameManager | `OnRestartRequested` event | Restart loop after game over |

**Rule**: only the communication methods listed above are permitted. No ad-hoc `Find`/`FindObjectOfType` chains, no direct cross-tier state queries.
