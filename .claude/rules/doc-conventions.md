# Doc Conventions

Rules for reading, creating, and routing changes to docs.

## Memory vs Rules

| Store as | Where | When to use |
|---|---|---|
| **Rule** | `.claude/rules/` or `CLAUDE.md` | Invariant constraints — architecture, coding standards, Claude behavior. Read on demand. |
| **Memory** | `memory/` (project or global) | Contextual facts that evolve. Recalled when relevant. |

## Template reading

**Use `.claude/template-docs/` as structural reference when creating project docs.** When creating or updating any doc in `.claude/docs/`, read the corresponding file in `.claude/template-docs/` first to follow the correct structure and format. Never write project-specific content into `.claude/template-docs/`.

## Broken references

**Report broken reference paths immediately.** Whenever you encounter a path reference in any `.md` file (e.g. `[text](path)`, inline code paths, or plain file paths) that does not resolve to an existing file or directory, stop and report it to the user before continuing. Include: the file containing the broken reference, the broken path, and what you expected to find there. Do not silently skip or work around broken paths.

## Doc-ownership routing

Which doc owns each type of change:

| Change type | Doc to edit |
|---|---|
| Item / power-up type, effect, duration | `.claude/docs/production/gdd/<system>.md` |
| Core mechanic constraint | `.claude/docs/preproduction/design-decisions.md` |
| System responsibility or dependencies | `.claude/docs/production/gdd/<system>.md` |
| Code pattern or best practice | `.claude/docs/preproduction/best-practices.md` |
| Architecture or communication rule | `.claude/docs/production/gdd/<system>.md` (Interactions section) · scene rules → `SceneLoader.md` · singleton rules → `GameManager.md` |

Never edit a GDD ad-hoc — route through `/write-gdd` (before code, normally via `/code`'s gate) or `/reconcile-gdd` (at PR time). See `.claude/rules/workflow.md` for the full GDD lifecycle.
