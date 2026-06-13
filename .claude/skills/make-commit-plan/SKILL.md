Review all pending changes, group them into logical atomic commits, present the plan for approval, then stage and commit each group using Conventional Commits format.

---

## Agent

`claude`

---

## Step 0 — Branch check (hard block)

Run `git branch --show-current`. If it is `main` (or the default branch), **STOP — do not commit:**

> "You're on `main`. Git hygiene: commits don't land on `main`. Run `/start-branch` first, or tell me a task description and I'll create `<type>/<desc>` now."

Do not proceed to Step 1 until the current branch is a non-`main` feature branch. (See `.claude/rules/git-hygiene.md`.)

---

## Step 1 — Audit current state

Run `git status` and `git diff --stat` to list all modified and untracked files.

If there is nothing to commit → report "Nothing to commit." and stop.

---

## Step 2 — Read each change

For each **modified** file: run `git diff <file>` to understand the nature and scope of the change.

For each **untracked** file: read the file directly to understand its content.

---

## Step 3 — Group into atomic commits

Apply these rules in order to assign every file to a commit group:

1. **Same system/feature** — files that implement or extend the same gameplay system belong together (e.g., `BallController.cs` + `BallConfig.cs` + `Ball.prefab`)
2. **Scripts travel with their scene/prefab** — a `.cs` change and the `.unity` or `.prefab` that wires it belong in the same commit
3. **Docs are isolated** — `.md` changes go in their own commit unless the file is the GDD/spec directly describing the feature being committed
4. **Config/settings are isolated** — `CLAUDE.md`, `.claude/settings.json`, `.claude/skills/`, `ProjectSettings/` changes = a separate `chore:` commit
5. **Split when ambiguous** — prefer smaller atomic commits over bundling; when grouping is genuinely unclear, ask the user before proceeding

---

## Step 4 — Draft commit messages

Format: `type(scope): short imperative description`

| Type | When to use |
|---|---|
| `feat` | New gameplay behavior, new system, new mechanic |
| `fix` | Bug correction |
| `refactor` | Restructure without behavior change |
| `chore` | Tooling, config, workflow, settings |
| `docs` | Documentation only |

Scope = system or area in lowercase (e.g., `ball`, `paddle`, `ui`, `audio`, `config`, `workflow`)

Examples: `feat(ball): add velocity cap and bounce angle`, `chore(workflow): add smart commit skill`, `docs(gdd): update brick system spec`

---

## Step 5 — Present the commit plan (draft loop)

Before touching anything, display a table:

```
| # | Commit message                              | Files                              |
|---|--------------------------------------------|------------------------------------|
| 1 | feat(ball): add velocity cap               | BallController.cs, BallConfig.cs   |
| 2 | chore(workflow): add git commit rules      | CLAUDE.md, .claude/settings.json   |
```

Then ask:

> **Proceed with this plan?**
> Reply: `yes` / `edit N <describe change>` / `cancel`

This is a **draft loop** — accepted replies and what happens:

| Reply | What happens |
|---|---|
| `yes` | Proceed to Step 6 |
| `edit N <describe change>` | Apply the edit to the plan, re-show the full updated table, ask again — **never proceed to Step 6 without a new explicit `yes`** |
| `cancel` | Stop. Report "Cancelled — nothing committed." |

**If the user's feedback is about a code or content problem** (not just commit grouping or message wording), do NOT fix it silently or skip it. Ask:

> "That's a code issue, not a commit-plan issue. Do you want to:
> **a) Fix it now** — I'll make the change, then restart the commit plan from Step 1
> **b) Commit as-is** — commit what we have now; log the issue and fix it separately"

Wait for the user's answer before doing anything.

**Never proceed to Step 6 without an explicit `yes` on the most recent version of the plan.**

---

## Step 6 — Execute each commit in sequence

For each group, in order:

1. **Compile check** — if the group contains any `.cs` files, run `check_compile_errors`. If errors exist, abort the entire sequence and report which errors must be fixed first.
2. **Stage explicitly** — `git add <file1> <file2> ...` using exact file paths, never `git add .` or `git add -A`
3. **Commit** — `git commit -m "message"` using a heredoc for multi-line messages
4. **Confirm** — output: `✓ Committed: feat(ball): add velocity cap`

---

## Step 7 — Final summary

Run `git log --oneline -5` and display the output so the user can verify what landed.

---

## Step 8 — Offer to push, then auto-open the PR

The commits are on a feature branch (guaranteed by Step 0). Offer to push:

> "Commits are on `<branch>`. Push to `origin`? (`yes` / `no`)"

On `yes`: `git push -u origin <branch>`. Once the push succeeds, **automatically run `/open-pr`** — do not make the user run it as a separate step. Then hand off:

> "Pushed and PR opened. Review it on GitHub; when you're done, tell me and I'll run `/reconcile-gdd`."

On `no`: stop; leave the commits local (no PR).

---

## Constraints

- **Never** use `git add .` / `git add -A` / `git commit -a` / `git commit --amend`
- **Never** commit on `main` — Step 0 hard-blocks it; the user must be on a feature branch
- **Push only via Step 8** (offer + explicit `yes`) — never silently, never force-push
- **Abort** the entire sequence if compile errors are found — report the errors clearly
- **Ask the user** when grouping is ambiguous — never guess
- **One commit at a time** — complete Step 6 fully for each group before moving to the next
