# Project Snapshot Index
Last updated: 2026-06-14 (juice pass — JuiceManager added)

## Scenes

| Scene | Path | Root GameObjects |
|---|---|---|
| Bootstrap | Assets/Scenes/Bootstrap.unity | Main Camera, GameManager, SceneLoader, InputManager, AudioManager |
| GameLogic | Assets/Scenes/GameLogic.unity | Player, LevelManager, EnemyFormation, EnemyFireController, ScoreSystem, PowerUpSystem |
| HUD | Assets/Scenes/HUD.unity | UIManager |
| MainMenu | Assets/Scenes/MainMenu.unity | (empty) |

### Bootstrap hierarchy detail

| GameObject | Components |
|---|---|
| Main Camera | Transform, Camera, AudioListener, UniversalAdditionalCameraData |
| GameManager | Transform, GameManager |
| SceneLoader | Transform, SceneLoader |
| InputManager | Transform, InputManager |
| AudioManager | Transform, AudioManager |
| JuiceManager | Transform, JuiceManager |

### GameLogic hierarchy detail

| GameObject | Components |
|---|---|
| Player | Transform, SpriteRenderer, BoxCollider2D, PlayerShipContext, BasicWeapon, PlayerShipStat |
| Player/Muzzle | Transform |
| LevelManager | Transform, LevelManager |
| EnemyFormation | Transform, EnemyFormation |
| EnemyFireController | Transform, EnemyFireController |
| ScoreSystem | Transform, ScoreSystem |
| PowerUpSystem | Transform, PowerUpSystem |

### HUD hierarchy detail

| GameObject | Components |
|---|---|
| UIManager | Transform, UIDocument, UIManager |

## Scripts

| Script | Path | Attached To |
|---|---|---|
| GameManager | Assets/Scripts/GameManager.cs | GameManager (Bootstrap) |
| SceneLoader | Assets/Scripts/SceneLoader.cs | SceneLoader (Bootstrap) |
| InputManager | Assets/Scripts/InputManager.cs | InputManager (Bootstrap) |
| AudioManager | Assets/Scripts/AudioManager.cs | AudioManager (Bootstrap) |
| PlayerShipContext | Assets/Scripts/PlayerShipContext.cs | Player (GameLogic) |
| PlayerShipStat | Assets/Scripts/PlayerShipStat.cs | Player (GameLogic) |
| BasicWeapon | Assets/Scripts/BasicWeapon.cs | Player (GameLogic) |
| Weapon | Assets/Scripts/Weapon.cs | (abstract base for BasicWeapon) |
| MoveAction | Assets/Scripts/MoveAction.cs | (plain C# BT leaf) |
| FireAction | Assets/Scripts/FireAction.cs | (plain C# BT leaf) |
| InvulnOverlayAction | Assets/Scripts/InvulnOverlayAction.cs | (plain C# BT leaf) |
| BTNode | Assets/Scripts/BT/BTNode.cs | (abstract BT base) |
| BTParallel | Assets/Scripts/BT/BTParallel.cs | (BT composite) |
| LevelManager | Assets/Scripts/LevelManager.cs | LevelManager (GameLogic) |
| EnemyFormation | Assets/Scripts/EnemyFormation.cs | EnemyFormation (GameLogic) |
| EnemyFireController | Assets/Scripts/EnemyFireController.cs | EnemyFireController (GameLogic) |
| Enemy | Assets/Scripts/Enemy.cs | Enemy prefab |
| Projectile | Assets/Scripts/Projectile.cs | PlayerBullet, EnemyBullet prefabs |
| ScoreSystem | Assets/Scripts/ScoreSystem.cs | ScoreSystem (GameLogic) |
| PowerUpSystem | Assets/Scripts/PowerUpSystem.cs | PowerUpSystem (GameLogic) |
| PowerUpData | Assets/Scripts/PowerUpData.cs | (ScriptableObject) |
| UIManager | Assets/Scripts/UIManager.cs | UIManager (HUD) |
| JuiceManager | Assets/Scripts/JuiceManager.cs | JuiceManager (Bootstrap) |
| IDamageable | Assets/Scripts/IDamageable.cs | (interface) |
| Team | Assets/Scripts/Team.cs | (enum) |
| LevelData | Assets/Scripts/LevelData.cs | (ScriptableObject) |

## Prefabs

| Prefab | Path | Key Components |
|---|---|---|
| Enemy | Assets/Prefabs/Enemy.prefab | SpriteRenderer, BoxCollider2D, Enemy |
| EnemyBullet | Assets/Prefabs/EnemyBullet.prefab | SpriteRenderer, BoxCollider2D, Projectile |
| PlayerBullet | Assets/Prefabs/PlayerBullet.prefab | SpriteRenderer, BoxCollider2D, Projectile |
| KillBurst | Assets/Prefabs/KillBurst.prefab | ParticleSystem (15 particles, yellow/white, 0.15s) |
| HitBurst | Assets/Prefabs/HitBurst.prefab | ParticleSystem (25 particles, red/orange, 0.3s) |
| ClearBurst | Assets/Prefabs/ClearBurst.prefab | ParticleSystem (60 particles, multi-color, 0.8s) |

## Audio Clips

| Clip | Path |
|---|---|
| sfx_enemy_kill | Assets/Audio/sfx_enemy_kill.wav |
| sfx_player_hit | Assets/Audio/sfx_player_hit.wav |
| sfx_player_death | Assets/Audio/sfx_player_death.wav |
| sfx_level_clear | Assets/Audio/sfx_level_clear.wav |
| sfx_level_start | Assets/Audio/sfx_level_start.wav |
| sfx_powerup | Assets/Audio/sfx_powerup.wav |
| music_gameplay | Assets/Audio/music_gameplay.mp3 |
| music_game_over | Assets/Audio/music_game_over.mp3 |
| music_victory | Assets/Audio/music_victory.mp3 |

## UI Assets

| Asset | Path | Used By |
|---|---|---|
| HUD.uxml | Assets/UI/HUD.uxml | UIManager (HUD scene) |
| HUDPanelSettings | Assets/UI/HUDPanelSettings.asset | UIDocument on UIManager |

## Level ScriptableObjects

| Asset | Path |
|---|---|
| Level1 | Assets/Levels/Level1.asset |
| Level2 | Assets/Levels/Level2.asset |
| Level3 | Assets/Levels/Level3.asset |
| Level4 | Assets/Levels/Level4.asset |
| Level5 | Assets/Levels/Level5.asset |
| Level6 | Assets/Levels/Level6.asset |

## PowerUp ScriptableObjects

| Asset | Path |
|---|---|
| bullet_speed | Assets/PowerUps/bullet_speed.asset |
| extra_life | Assets/PowerUps/extra_life.asset |
| multi_shot | Assets/PowerUps/multi_shot.asset |
| power_shot | Assets/PowerUps/power_shot.asset |
| rapid_fire | Assets/PowerUps/rapid_fire.asset |
| swift | Assets/PowerUps/swift.asset |
