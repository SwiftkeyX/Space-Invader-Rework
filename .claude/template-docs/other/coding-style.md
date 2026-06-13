# Coding Style

> HOW to write code in this project — naming, structure, and anti-patterns.
> For WHAT scripts exist and how they communicate, see per-system GDDs at `.claude/docs/production/gdd/`.
> For Unity 6 pattern choices (Input System, UI Toolkit, etc.), see `best-practices.md`.
> For tech-stack level forbidden patterns (banned libraries, subsystems), see `.claude/docs/preproduction/technical-preferences.md`.

## Naming Conventions

| Symbol | Convention | Example |
|---|---|---|
| Classes | PascalCase | `PlayerController` |
| Public fields / properties | PascalCase | `MoveSpeed` |
| Private fields | `_camelCase` | `_moveSpeed` |
| Methods | PascalCase | `TakeDamage()` |
| Events | `OnEventName` | `OnPlayerDied` |
| Constants | `ALL_CAPS` | `MAX_HEALTH` |
| Files | Match class name | `PlayerController.cs` |

## Rules

- One script per responsibility — no monolithic classes
- No `Find`, `FindObjectOfType`, or `GameObject.Find` at runtime — wire references in the Inspector or via events
- GameManager is the only permitted singleton
- No `#region` blocks — if a file needs them, split it
- All `Start` / `Awake` logic must be null-safe; log a warning if a required reference is missing
- No magic numbers — define named constants or expose to Inspector

## Anti-Patterns

| Pattern | Instead |
|---|---|
| `GetComponent` in `Update()` | Cache in `Awake()` or `Start()` |
| Polling in `Update` for one-time events | Use event callbacks |
| Coroutines for simple timed delays | Use `Invoke` or events |
| Hard-coded data in MonoBehaviours | `ScriptableObject` assets |
