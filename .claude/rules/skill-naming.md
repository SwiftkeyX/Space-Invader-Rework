# Skill Naming

Every skill folder under `.claude/skills/<name>/` follows a naming convention so the name signals the skill's role. Names are lowercase kebab-case. When adding a skill, place it in the matching family and follow that family's pattern.

## Families

| Family | Pattern | Members | Who invokes |
|---|---|---|---|
| Entry routers | bare verb | `code`, `debug` | User |
| Lifecycle | `verb-noun` | `start-branch`, `make-commit-plan`, `open-pr`, `reconcile-gdd` | User |
| Orchestrators | `<phase>-task` | `preproduction-task`, `production-task`, `beta-task` | User |
| Preproduction steps | `write-<doc>` | `write-game-vision`, `write-design-decisions`, `write-technical-preferences`, `write-systems-design`, `write-architecture`, `write-best-practices` | Internal (via `preproduction-task`) |
| Production steps | `<verb>-system` | `design-system`, `code-system` | Internal (via `production-task`) |
| Production-exit pass | `<noun>-pass` | `architecture-pass` | User (handed off by `production-task`) |
| Beta steps | `<x>-pass` (+ `ship` concept) | `juice-pass`, `tune-pass`, `performance-pass`, `release-pass` | Internal (via `beta-task`) |
| Workers | `verb-noun` | `read-gdd`, `write-gdd`, `edit-unity`, `fix-bug`, `log-bug`, `fix-all-bugs` | Internal (via `code` / `debug` / `fix-bug`) |
| Utilities | `verb-noun` | `explain-workflow`, `check-pipeline-stage`, `read-architecture`, `sync-template`, `setup-gh-bot`, `clean-docs` | User |
| Automation | `verb` / `verb-noun` | `regress`, `watch-project` | Internal / scheduled |

## Rules

- **Lowercase kebab-case only.** No camelCase, no underscores, no `task`/`session` filler words.
- **Default to `verb-noun`** for any action skill (worker, utility, lifecycle). The verb says what it does; the noun says to what.
- **Use the family suffix** when a skill joins an existing family: `-task` (orchestrator), `-system` (production step), `-pass` (a *pass* — the beta steps, plus the single production-exit `architecture-pass`), `write-<doc>` (preproduction step). `-pass` signals an iterative refinement pass; it is no longer beta-exclusive.
- **Bare single-word names** are reserved for the two entry routers (`code`, `debug`) and a few self-evident actions (`regress`).
- **No verbose phrases.** A name longer than three kebab segments is a smell — shorten it (e.g. `fix-all-bugs`, not `open-session-for-fix-all-bug`).
- **When you rename a skill, update every reference in the same change** — other SKILL.md files, `.claude/rules/`, `CLAUDE.md`, and any doc index. A dangling skill reference is a broken structure (see `doc-conventions.md`).
