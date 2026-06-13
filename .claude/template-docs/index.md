# Template Docs Index

> **Structure:** templates live under only four folders — `preproduction/ production/ beta/ other/` — mirroring `.claude/docs/`. Never add a new folder; see `.claude/rules/docs-structure.md`.

These files are structural templates for a new project. They contain no project-specific content.

## How to use
- **Copy and fill** — copy the file to the matching folder under `.claude/docs/`, then replace placeholder content with project-specific details
- **Reference only** — read the template for structural guidance; do not copy (already pre-filled or used as-is)

## Pre-production (`preproduction/`)
| File | Use | Purpose |
|---|---|---|
| `preproduction/systems-design.md` | Copy and fill | System tier table, responsibilities, and dependencies |
| `preproduction/architecture.md` | Copy and fill | Script table — one row per script, responsibility, dependencies |
| `preproduction/technical-preferences.md` | Copy and fill | Engine version, platform, performance budgets, test requirements |
| `preproduction/best-practices.md` | Copy and fill | Project-critical patterns (top section) + Unity 6 current patterns (pre-filled) |

## Production (`production/`)
| File | Use | Purpose |
|---|---|---|
| `production/gdd/_template.md` | Copy per system | Per-system GDD — copy once for each system, rename to `<system>.md` |

## Other (`other/`)
| File | Use | Purpose |
|---|---|---|
| `other/coding-style.md` | Reference only | Required code patterns and anti-patterns |
| `other/asset-conventions.md` | Reference only | Folder layout, naming rules, import settings |
| `other/onboarding.md` | Reference only | Onboarding guide for new contributors or agents |
