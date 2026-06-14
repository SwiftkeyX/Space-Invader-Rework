# Workflow

End-to-end process for every code task, from branch creation to merge.

## Lifecycle

```
/start-branch  →  /code  (loop until done)  →  /make-commit-plan  →  push (with your OK) → auto /open-pr
   →  YOU review + comment in the PR
   →  /reconcile-gdd  →  resolve flags  →  merge PR  →  delete branch  →  back to /start-branch
```

`/code` is the single entry skill for a code change; loop it for each change until ready to commit. Internally it sequences `/read-gdd → /write-gdd` (if the spec is wrong) `→ /edit-unity`. To fix a known bug instead, use `/debug`. See the `explain-workflow` skill for the full big-picture narrative.

| Step | Who | Skill |
|---|---|---|
| Branch off `main` before any work | Claude | `/start-branch` |
| Build / change something (gates the GDD, then edits Unity) | Claude | `/code` (loops `/read-gdd` · `/write-gdd` · `/edit-unity`) |
| Fix a known bug | Claude | `/debug` |
| Commit to the branch | Claude | `/make-commit-plan` |
| Push the branch (your confirmation) | Claude | `/make-commit-plan` (final step) |
| Open the PR | Claude | auto-run by `/make-commit-plan` after push |
| Review the PR, comment inline | **You** | GitHub |
| Second-pass code-vs-GDD check + resolve | Claude | `/reconcile-gdd` |
| Merge + clean up | Claude | `/reconcile-gdd` (final step, your confirmation) |

## GDD gates

**Before any code task → the `/read-gdd` gate (run automatically by `/code`)**

Before writing or modifying any `.cs` file, `/code` runs `/read-gdd` for the system you are about to touch. It reads the GDD, and if the GDD is wrong or your change alters the design, `/code` runs `/write-gdd` to update the doc first so you start from a spec that matches intent. If no GDD exists for a new system, author one with `/write-gdd` (or `/design-system` in Phase 2) before coding. Never write to Unity without this gate (see `unity-editor.md`).

**At PR time → push auto-opens the PR, then `/reconcile-gdd`**

1. `/make-commit-plan` pushes and automatically opens the PR via `/open-pr`.
2. You review it on GitHub, leaving inline comments/flags directly in the PR.
3. When you are done reviewing, tell Claude — it runs `/reconcile-gdd`, which reads the diff and your comments, diffs the changed `.cs` against their GDDs, and consults you on each divergence: update the GDD (code was better) or fix the code (GDD was right).
4. Claude applies all resolutions, then offers to merge and delete the branch (explicit `yes` required).

**Tool roles:** `/write-gdd` updates a doc when the design changes. `/reconcile-gdd` syncs the doc to the code at PR time. Never edit a GDD ad-hoc — route through these.

## Phase orchestration

For implementation tasks, use the phase skill that matches the current pipeline stage:

| Phase | Skill | When |
|---|---|---|
| Phase 1 | `/preproduction-task` | Fill out design and technical docs |
| Phase 2 | `/production-task` | Design GDD and implement a system |
| Phase 2 → 3 bridge | `/architecture-pass` | After the game is built & tested, refactor the architecture to match the GDDs |
| Phase 3 | `/beta-task` | Feel tuning, bug pass, performance pass, release |

### Production-exit architecture pass

When `/production-task` reaches Milestone 3 (game built and tested), it asks a **go/no-go**: *continue* or *ditch the game* if the idea didn't pan out. On `continue` it hands off to **`/architecture-pass`** — the Phase 2 → Phase 3 bridge. That skill is just the **named application of the PR-review lifecycle loop** drawn at the top of this file: it audits the architecture (`technical-director`), fixes issues via `/code`, commits via `/make-commit-plan`, you review the PR, then `/reconcile-gdd` syncs code and GDDs and merges — looping until a `technical-director` audit is clean. **Phase 3 (`/beta-task`) is locked until the Architecture pass is `[x]`.**

For one-off doc lookups, consult `.claude/docs/index.md`.  
For git branch and push rules, see `.claude/rules/git-hygiene.md`.
