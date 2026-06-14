Orientation skill — run this FIRST, before starting anything, to understand the big picture of the whole agentic development workflow so you don't get lost. It is a reference: it explains the lifecycle, names every skill and when to reach for it, and points you to your next step. It runs nothing itself.

---

## Agent

`claude`

---

## What to print

Walk the user through the full lifecycle below. Keep it readable — this is a map, not a wall of text. At the end, tell them their immediate next move is `/check-pipeline-stage`.

---

## The big picture

### 0. Orientation — know where you are
- `/explain-workflow` — this skill: the whole picture.
- `/check-pipeline-stage` — run at the start of every session. It reads `PIPELINE.md`, tells you which phase you're in, what's done, and **which orchestrator to run next**.

The project moves through three phases. Each phase has one **orchestrator** skill that drives the work; you mostly answer its questions and approve gates.

### 1. Phase 1 — Pre-production → `/preproduction-task`
Auto-runs, in order: `/write-game-vision` → `/write-design-decisions` → `/write-technical-preferences` → `/write-systems-design` → `/write-architecture` → `/write-best-practices`. You answer each skill's questions. Output: a complete set of design docs.

### 2. Phase 2 — Production → `/production-task`
- **Sub-phase A** auto-runs `/design-system` for each system — you review and approve each GDD.
- **Sub-phase B** auto-runs `/code-system` to build each system in tier order, pausing at a **test gate after each tier** so you can play-test before moving on.
Output: a testable core game. Then a **go/no-go**: *continue*, or *ditch* the game if the idea didn't pan out.

### 2½. Phase 2 → 3 bridge — Architecture pass → `/architecture-pass`
On `continue`, refactor the code architecture to match the GDDs **before** polish. It audits the architecture (`technical-director`), fixes issues via `/code`, then commits → PR → you review → `/reconcile-gdd`, looping until the audit is clean. **Phase 3 is locked until this is done.**

### 3. Phase 3 — Beta → `/beta-task`
Auto-runs: `/juice-pass` → `/tune-pass` (feel, then difficulty) → `/fix-all-bugs` → `/performance-pass` → `/release-pass`. This is where the game gets its polish and ships.

### 4. Your hands-on role (mostly in beta, but valid anytime)
Now you have a game to play. Your job is to test it and iterate:

- **Found a bug?** (a console error, OR just something that feels off) → `/debug`. It asks one question and routes you: log it (`/log-bug`), fix it now (`/fix-bug`), or fix everything in one go (`/fix-all-bugs`). Use `/log-bug` liberally while testing — that log is your treasure list later.
- **Want to add, change, or remove a feature** — including changing the original idea ("power-ups are boring, let's swap them", "one weapon is dull, let's allow several")? → `/code`. It reads the design docs first (`/read-gdd`), and if the idea really should change it updates the docs (`/write-gdd`, with a good-enough reason and your approval) before applying the change in Unity (`/edit-unity`). A docs-only change skips the Unity step.
- **Architecture cleanup** once the game works → that's the **`/architecture-pass`** (the Phase 2 → 3 bridge): a guided audit-fix-PR-reconcile loop. For a one-off refactor outside that pass, run it through `/code`, or hand the whole job to a stronger model (Opus).

### 5. Saving your work (anytime you've made changes)
`/start-branch` (a feature branch — never `main`) → make your changes via `/code` or `/debug` → `/make-commit-plan` (commits your work and **auto-opens the PR**) → you review the PR on GitHub → tell Claude you're done → `/reconcile-gdd` (checks code against the docs, resolves differences, merges, deletes the branch). Then back to `/start-branch` for the next task.

### 6. Philosophy — it's a prototype
You're building a prototype to find out if the game is fun. Test it honestly. If the idea doesn't work, you can stop at any point — that's what real studios do: prototype, then decide whether to continue. There's no reason to polish the architecture of a game you're going to drop.

---

## Next step

End with: "Your next move: run `/check-pipeline-stage` to see where you are and which orchestrator to run."

---

## Constraints

- Reference only — run no other skill, make no edits, touch no git.
- Only name skills that exist. The user never invokes internal skills directly (workers like `/read-gdd`, `/write-gdd`, `/edit-unity`, and the bug/step skills run via their entry skill or orchestrator).
