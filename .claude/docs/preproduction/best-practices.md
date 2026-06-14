# Best Practices

<!-- Two sections: project-critical patterns (fill in), then Unity 6 current patterns (pre-filled, factual). -->
<!-- Agents read this before writing any code. Project-critical patterns override everything else. -->

---

## Project-Critical Patterns

<!-- Game-specific rules that override all other guidance. -->
<!-- Add a rule here whenever: -->
<!--   - A timing-sensitive system has a non-obvious constraint -->
<!--   - A state machine invariant must never be violated -->
<!--   - A bug was caused by a pattern and must never recur -->
<!--   - An architectural decision was made that differs from the Unity 6 defaults below -->

These rules are **hard constraints**, not suggestions. They override the Unity 6 patterns below where they conflict.

### Single singleton

**Rule**: `GameManager` is the only singleton in the project. Every other system communicates through the events declared in `architecture.md`.

**Why**: One global owner of run state keeps coupling loose and testable; scattered singletons create hidden cross-system dependencies that break the scene split.

**Instead**: Subscribe to `GameManager` events. Never `Find` / `FindObjectOfType` and never query another system's state directly across tiers.

### The communication contract is frozen

**Rule**: Only the From→To methods in the `architecture.md` "Communication Patterns" table are permitted. Adding a new edge requires updating `architecture.md` first.

**Why**: The architecture doc is the authoritative wiring map. Ad-hoc calls silently rot it and reintroduce the `Find`-chain coupling it exists to prevent.

### Pool everything hot — zero GC in steady state

**Rule**: Bullets, enemies, and particles are pooled. No per-frame `new`, no LINQ in `Update`, no boxing in gameplay hot paths.

**Why**: Budget is 8.3 ms @ 120 fps with zero steady-state GC (`technical-preferences.md`). Bullet-hell density means hundreds of live projectiles; a per-frame allocation spikes the frame and the zero-GC budget is broken.

**Instead**: Pre-warm object pools; acquire/release instead of Instantiate/Destroy during play.

### Scene loads through SceneLoader only

**Rule**: Never call `SceneManager.LoadScene` / `LoadSceneMode.Single` directly, and never use `DontDestroyOnLoad`. Persistent objects live in `Bootstrap.unity`.

**Why**: The Bootstrap / MainMenu / GameLogic / HUD split depends on additive, controlled loading. Direct loads and `DontDestroyOnLoad` break the split and duplicate persistent objects.

### Pause/hit-stop-safe timers

**Rule**: Any gameplay, i-frame, or power-up-duration timer uses `Time.unscaledDeltaTime` / `WaitForSecondsRealtime`.

**Why**: Hit-stop (D6) sets `Time.timeScale = 0`. Scaled time freezes during hit-stop, silently stalling i-frame and power-up timers and causing chain-death or stuck-effect bugs.

### Behavior Tree leaves must read unscaled time directly

**Rule**: BT leaf nodes (`BTNode.Tick(float dt)`) that manage i-frame or power-up timers must call `Time.unscaledDeltaTime` themselves — never use the passed `dt`.

**Why**: `PlayerShipContext.Update()` passes `Time.deltaTime` to `_btRoot.Tick(dt)`. During hit-stop (`timeScale = 0`) `dt` is 0, freezing any leaf that uses it for a timer. The Pause/hit-stop-safe rule still applies; it just must be satisfied inside the leaf, not via the passed argument.

**How to apply**: `InvulnOverlayAction` is the reference implementation — it ignores `dt` and decrements `InvulnTimer` with `Time.unscaledDeltaTime` directly.

### Horizontal-only player movement (invariant)

**Rule**: The player ship moves on the X axis only. Never add a vertical or dodge-dash axis.

**Why**: D1/N1 — horizontal-only is the core "this is still Space Invaders" identity. Free 2-axis movement was explicitly rejected.

### I-frame invariant

**Rule**: A hit that lands during the invulnerability window deals zero damage and costs no life.

**Why**: D2 — i-frames exist to prevent multi-hit chain-deaths in dense bullet patterns. A hit registering during i-frames defeats the mechanic and makes the curve feel unfair.

### Difficulty values live in ScriptableObjects

**Rule**: Per-level HP, threat speed, fire-rate, and pacing values live in `LevelData` ScriptableObjects — never hardcoded in scripts.

**Why**: The Phase 3 difficulty-tuning pass must iterate values without recompiling, and the 6-level curve (D5 / `game-vision.md`) is the primary tuning surface.

### UI is UI Toolkit

**Rule**: HUD, menus, the power-up pick screen, and game-over UI are built with UI Toolkit (`.uxml` + `.uss`), not UGUI Canvas.

**Why**: Project decision — UI Toolkit is the Unity 6 production-ready path and keeps UI markup/style separate from logic. The `ui-programmer` routing in `technical-preferences.md` reflects this.

### Keep Update() thin — extract helpers

**Rule**: No `Update()` method should exceed ~15 lines. Logic that belongs together (march step, edge check, fire handling, state tick) moves into private helpers; `Update()` is a readable dispatch list.

**Why**: Long `Update()` methods mix multiple concerns in one scan path, making it hard to read, test, or extend any single concern. Enforced during the architecture pass (2026-06-14) after EnemyFormation and PlayerShip were refactored to this shape.

**How to apply**: If `Update()` is growing, extract named helpers immediately (`MarchStep()`, `HandleFire()`, `FlashIfInvuln()`). Each helper handles one concern.

### IDamageable for hittable objects

**Rule**: Any object that can be hit by a `Projectile` implements `IDamageable` (`Team Team { get; }` + `void TakeDamage(int damage)`). `Projectile.OnTriggerEnter2D` calls `GetComponentInParent<IDamageable>()` and team-filters — never `GetComponentInParent<Enemy>()` or `GetComponentInParent<PlayerShipContext>()`.

**Why**: The original if-else team check in Projectile was an Open/Closed violation — adding a third damageable type required editing Projectile. The interface eliminates that. Enforced during the architecture pass (2026-06-14).

**How to apply**: Implement `IDamageable` on any new damageable entity. Never add a new branch to Projectile's collision handler.

### SRP: MonoBehaviour coordinator + plain C# state objects

**Rule**: A MonoBehaviour that owns significant state should be split into a thin **coordinator** (MonoBehaviour, lifecycle + wiring) and one or more **plain C# class** objects for state and stats (`PlayerShipState`, `PlayerShipStat`). Abstract MonoBehaviours (e.g. `Weapon`) handle sub-behaviours with their own Unity lifecycle.

**Why**: Lumping movement + firing + i-frames + stat mutation into one MonoBehaviour violates SRP and makes every concern harder to extend. Plain C# objects are lighter than components and testable without a scene. Enforced during the architecture pass (2026-06-14) when `PlayerShip.cs` was split into `PlayerShipContext`, `PlayerShipState`, `PlayerShipStat`, and `Weapon` / `BasicWeapon`.

**How to apply**: When a MonoBehaviour starts accumulating unrelated fields, extract them into a plain C# class and instantiate it in the MonoBehaviour's `Awake()`.

---

## Unity 6 LTS — Current Patterns

**Last verified:** 2026-05-30

> These patterns differ from older Unity versions that may appear in LLM training data.
> Follow these. Do not revert to the legacy column.

### Input

| Use This | Not This | Why |
|---|---|---|
| `UnityEngine.InputSystem` package | `Input.GetKey()` / `Input.GetAxis()` | Rebindable, cross-platform, event-driven |

```csharp
// ✅ Correct
controls.Gameplay.Jump.performed += ctx => Jump();

// ❌ Legacy
if (Input.GetKeyDown(KeyCode.Space)) Jump();
```

Read input directly from device when you don't need rebinding:

```csharp
Vector2 mousePos = Mouse.current.position.ReadValue();
if (Mouse.current.leftButton.wasPressedThisFrame) { /* action */ }
```

---

### UI

| Use This | Not This | Why |
|---|---|---|
| UI Toolkit (`.uxml` + `.uss`) | UGUI Canvas + `Text`/`Image` components | Production-ready in Unity 6, HTML/CSS workflow |

```csharp
// ✅ Correct
var root = GetComponent<UIDocument>().rootVisualElement;
root.Q<Button>("play-button").clicked += StartGame;

// ❌ Legacy
GetComponent<Button>().onClick.AddListener(StartGame);
```

---

### Asset Loading

| Use This | Not This | Why |
|---|---|---|
| `Addressables` | `Resources.Load` | Async, memory-efficient, supports remote delivery |

```csharp
// ✅ Correct
var handle = Addressables.InstantiateAsync(enemyKey);
var enemy = await handle.Task;
Addressables.ReleaseInstance(enemy); // release when done

// ❌ Legacy
var enemy = Resources.Load<GameObject>("Enemies/Basic");
```

---

### Tunable Data

| Use This | Not This | Why |
|---|---|---|
| `ScriptableObject` assets | Hardcoded values or config files | Inspector-editable, designer-friendly, no recompile |

```csharp
// ✅ Correct
[CreateAssetMenu(menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject {
    public float MoveSpeed;
    public int MaxHealth;
}
```

---

### Timers (when timeScale may change)

| Use This | Not This | Why |
|---|---|---|
| `Time.unscaledDeltaTime` / `WaitForSecondsRealtime` | `Time.deltaTime` / `WaitForSeconds` | `Time.timeScale = 0` (hit-stop, pause) freezes scaled time |

```csharp
// ✅ Survives pause / hit-stop
_timer += Time.unscaledDeltaTime;
yield return new WaitForSecondsRealtime(duration);

// ❌ Freezes when timeScale = 0
_timer += Time.deltaTime;
yield return new WaitForSeconds(duration);
```

---

### DOTS / ECS

| Use This | Not This | Why |
|---|---|---|
| `ISystem` (unmanaged) | `ComponentSystem` / `JobComponentSystem` | Burst-compatible, no managed heap allocation |
| `IJobEntity` | `IJobForEach` | Modern, cleaner query syntax |

```csharp
// ✅ Correct
public partial struct MovementSystem : ISystem {
    public void OnUpdate(ref SystemState state) {
        foreach (var (transform, speed) in
            SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveSpeed>>()) {
            transform.ValueRW.Position += speed.ValueRO.Value * SystemAPI.Time.DeltaTime;
        }
    }
}
```

---

### Rendering (URP custom passes)

| Use This | Not This | Why |
|---|---|---|
| `RenderGraph` API | `CommandBuffer.Execute()` | Required in Unity 6 URP; old API deprecated |

---

### Testing

| Use This | Not This | Why |
|---|---|---|
| NUnit + Unity Test Runner | Manual Play Mode testing only | Repeatable, automated, catches regressions |
| Edit Mode tests for pure logic | Play Mode tests for everything | Faster iteration; Play Mode tests are slower |

```csharp
// ✅ Edit Mode test — no scene needed
[Test]
public void DamageFormula_ReturnsCorrectValue() {
    Assert.AreEqual(75, DamageCalculator.Calculate(base: 100, reduction: 0.25f));
}

// ✅ Play Mode test — needs scene
[UnityTest]
public IEnumerator Player_TakesDamage_HealthDecreases() {
    var player = new GameObject().AddComponent<PlayerHealth>();
    player.TakeDamage(25);
    yield return null;
    Assert.AreEqual(75, player.Current);
}
```

---

### Summary Reference

| Feature | Use (2026) | Avoid (Legacy) |
|---|---|---|
| Input | Input System package | `Input` class |
| UI | UI Toolkit | UGUI Canvas |
| Assets | Addressables | `Resources` |
| Tunable data | `ScriptableObject` | Hardcoded constants |
| ECS | `ISystem` + `IJobEntity` | `ComponentSystem` |
| Rendering | URP + RenderGraph | Built-in pipeline |
| Timers (pause-safe) | `unscaledDeltaTime` | `deltaTime` |
| Testing | NUnit + Test Runner | Manual only |
