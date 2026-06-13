Create and switch to a feature branch before starting any work. Enforces git hygiene: work never happens on `main`. Run this at the very start of a task — before `/code`. Carries any in-progress changes onto the new branch (also the way to rescue work already done on `main`).

---

## Agent

`claude`

---

## Entry Condition

The user is about to start a change (feature, fix, tuning, docs) and needs a branch. Works whether the tree is clean or already has uncommitted changes.

---

## Steps

1. **Check the current branch** — `git branch --show-current`.
   - **Already on a non-`main` feature branch** → ask: "Already on `<branch>` — reuse it, or start a new branch?" Reuse → report and stop. New → continue.
   - **On `main`** → continue.

2. **Pick a name** — propose `<type>/<short-kebab-desc>` derived from the intended work, where `<type>` ∈ `feat | fix | chore | docs | refactor` (see `git-hygiene.md`). If the user hasn't described the task, ask for a one-line description first. Confirm the proposed name with the user before creating.

3. **Create + switch:**
   - **Clean tree** → branch from the latest `main`: `git fetch origin`, then `git switch -c <name> origin/main`.
   - **Uncommitted changes present** → `git switch -c <name>` from current `HEAD` so the work-in-progress travels onto the new branch (do NOT stash or discard). This is the rescue path for work already sitting on `main`.
   - Surface a dirty tree to the user before switching so nothing is a surprise.

4. **Report** — "On branch `<name>` (off `main`). Proceed with `/code`, then `/make-commit-plan`."

---

## Exit Condition

`git branch --show-current` shows a non-`main` feature branch, and any prior uncommitted work is present on it.

---

## Constraints

- **Never push** here — pushing happens at the end of `/make-commit-plan`, with confirmation.
- **Never branch off a non-`main` base** unless the user explicitly asks.
- **Never stash, reset, or discard** uncommitted changes — carry them onto the branch.
- Never create a branch whose name omits the `<type>/` prefix.
