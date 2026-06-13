# Docs Folder Structure

**The doc tree has a fixed shape. Do not change it.** Inventing new folders is what broke path references across this project — this rule exists to stop that.

## The only permitted folders

Under **both** `.claude/docs/` and `.claude/template-docs/`, the only allowed top-level folders are:

| Folder | Holds | Phase |
|---|---|---|
| `preproduction/` | game-vision, design-decisions, systems-design, architecture, technical-preferences, best-practices | Phase 1 |
| `production/` | per-system GDDs under `production/gdd/` | Phase 2 |
| `beta/` | build-notes, known-issues, known-issues-archive | Phase 3 |
| `other/` | anything cross-phase, auto-generated, or vague (architecture-overview, changelog, ADRs under `other/adr/`) |  — |

## Rules

- **Never create a new top-level folder** under `.claude/docs/` or `.claude/template-docs/`. If a doc does not clearly belong to a development phase, put it in `other/` — never make a new folder for it.
- The **only** permitted subfolders are the ones already established: `production/gdd/` and `other/adr/`. Do not add others.
- `.claude/docs/` root holds exactly three control files — `index.md`, `PIPELINE.md`, `project-snapshot-index.md`. Do not add more files at the root.
- A template in `.claude/template-docs/<folder>/` must live in the **same** folder its output doc occupies under `.claude/docs/<folder>/` — keep the two trees mirrored.
- **When you add or move a doc, update the matching index in the same change** (`.claude/docs/index.md` or `.claude/template-docs/index.md`). A doc that isn't in its index is a broken structure.

## If a doc genuinely needs a new home

Do not create the folder. Stop and ask the user whether to (a) place it in `other/`, or (b) amend this rule to add a fifth folder. Changing the permitted set is a deliberate decision, not an ad-hoc one.
