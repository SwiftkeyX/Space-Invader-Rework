Production-exit orchestrator: once the game is built and proven worth continuing, refactors the code architecture to match the GDDs through the PR-review loop, looping until a technical-director audit is clean. The Phase 2 → Phase 3 bridge. Handed off to by `/production-task`; also user-invokable.

---

## Agent

Orchestrator (`claude`) — routes to `/code`, `/make-commit-plan`, `/reconcile-gdd`, and spawns the `technical-director` audit agent. Writes no Unity state itself.

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/PIPELINE.md` | Read + Write | Verify entry condition; tick the Architecture pass item on a clean audit |
| `.claude/docs/production/gdd/*.md` | Read | The architecture contract each script is audited against |
| `.claude/docs/preproduction/best-practices.md` | Read | Project-critical patterns the audit enforces |
| `.claude/docs/other/architecture-overview.md` | Read | Coupling/SRP summary — refreshed via `/read-architecture` as audit input |
| `.claude/rules/git-hygiene.md` | Read | Branch + push rules for the refactor |

---

## Entry Condition

Milestone 3 = `[x]` in PIPELINE.md (all Phase 2 systems built and tested). If not met, stop:
"Architecture pass is for a content-complete game — finish Phase 2 (all systems + test gates, Milestone 3) first."

> The go/no-go ("continue or ditch the game?") is asked by `/production-task` at the end of Phase 2 — reaching this skill means the user already chose **continue**. If invoked directly, confirm the game is worth continuing before starting.

---

## Steps

**Step 1 — Branch**

Ensure work is on a feature branch. If on `main`, run `/start-branch` to create `refactor/architecture-pass` (or a more specific `refactor/<area>`). Never refactor on `main` (git-hygiene).

---

**Step 2 — Audit**

1. Run `/read-architecture` to refresh `.claude/docs/other/architecture-overview.md` (SRP + coupling per system, derived from the GDDs).
2. Spawn the **`technical-director`** agent for a full architecture audit: scripts vs their GDDs, `best-practices.md`, and the scene rules in `unity-editor.md`.
3. Collect a **prioritized list** of architecture issues — e.g. SRP violations (two responsibilities in one script), illegal cross-system coupling, `DontDestroyOnLoad`/direct `SceneManager.LoadScene` usage, singletons outside Bootstrap.

---

**Step 3 — Present findings**

Show the prioritized issue list to the user and agree on what to fix **this round** (one PR's worth — keep it atomic). 

If the audit reports **no issues**, skip to Step 6 — the architecture is already clean.

---

**Step 4 — Fix**

Fix the agreed issues via `/code` for each change. `/code` enforces the GDD gate (`/read-gdd` → `/write-gdd` if the design itself must change → `/edit-unity`). Stay within the agreed scope.

---

**Step 5 — Commit, review, reconcile**

1. Run `/make-commit-plan` — it groups the changes, commits, pushes (on your `yes`), and auto-opens the PR via `/open-pr`.
2. Hand off: "Refactor PR opened. Review it on GitHub; tell me when you're done."
3. When the user says they have finished reviewing, run `/reconcile-gdd` — it diffs the changed `.cs` against their GDDs plus your PR comments, resolves each divergence with you (update GDD vs fix code), then merges and deletes the branch on your explicit `yes`.

---

**Step 6 — Loop or finish**

Re-run the audit (Step 2).

- **Issues remain** → loop from Step 1 on a fresh branch for the next atomic refactor.
- **Audit clean and the last PR merged** → tick `🏛️ Architecture pass` in PIPELINE.md (`- [ ]` → `- [x]`) and report:

  > "Architecture pass complete — audit clean, code matches the GDDs. Phase 2 is fully done. Next: `/beta-task` for Phase 3 polish."

---

## Exit Condition

`technical-director` audit reports clean, all refactor PRs merged, `🏛️ Architecture pass` = `[x]` in PIPELINE.md, and the working branch is back on `main`.

---

## Constraints

- Never tick the Architecture pass before the audit comes back clean — a single remaining high-priority issue blocks the tick.
- Never refactor on `main` — Step 1 hard-requires a feature branch.
- Never skip the GDD gate when fixing — all changes go through `/code`.
- Merging and resolving divergences is delegated to `/reconcile-gdd`; never merge with unresolved divergences.
- One atomic PR per loop iteration — do not bundle unrelated refactors into one PR.
