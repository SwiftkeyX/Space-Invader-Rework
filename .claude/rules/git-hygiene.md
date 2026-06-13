# Git Hygiene

**Never commit or push to `main`.** `main` is the default/protected branch — it only ever changes by merging a reviewed PR. All work happens on a feature branch. This is a hard rule, not a preference.

For the full step-by-step lifecycle and GDD gates, see `.claude/rules/workflow.md`.

## Hard rules

- **Check the current branch before every commit.** If on `main` (or the default branch), STOP and run `/start-branch` first. `/make-commit-plan` enforces this and will refuse to commit on `main`.
- **Branch naming:** `<type>/<short-kebab-desc>`, where `<type>` ∈ `feat | fix | chore | docs | refactor` (mirrors the commit types in `make-commit-plan`). Examples: `feat/powerup-magnet`, `fix/enemy-march-stall`, `chore/git-hygiene`.
- **Pushing is outward-facing** — only after explicit user confirmation. Never push silently.
- **Never** force-push, rebase shared history, or commit directly to `main`.

## After the PR merges

`/reconcile-gdd` handles this automatically on `yes`: `gh pr merge --delete-branch` → `git switch main` → `git pull`. Then the next task starts again at `/start-branch`.
