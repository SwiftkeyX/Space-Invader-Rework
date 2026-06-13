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

*(No project-critical patterns yet — add as you discover them during development.)*

<!-- Example format:
### [Short rule name]

**Rule**: Never use `Time.deltaTime` for parry window timers.

**Why**: `Time.timeScale = 0` is used for hit-stop — it freezes `deltaTime`, silently stalling the timer.

**Instead**: Use `Time.unscaledDeltaTime` or `WaitForSecondsRealtime` for any timer that must survive `timeScale` changes.
-->

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
