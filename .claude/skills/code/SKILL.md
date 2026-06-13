Entry skill for building or changing something in Unity — the sibling of `/debug` (which is for fixing what's broken). This is the ONE skill the user invokes for a code task, looped until ready to commit. It does no work itself; it sequences the atomic workers: `/read-gdd` → `/write-gdd` (only if the spec is wrong) → `/edit-unity`.

---

## Agent

Entry skill — routes to worker skills. See each worker's own agent assignment.

---

## Entry Condition

The user describes, in plain language, a change to build or modify (a new feature, a behavior change, a tuning change). For fixing a known bug, use `/debug` instead.

---

## Steps

**Step 0 — Branch check**
Run `git branch --show-current`. If on `main`, run `/start-branch` first so work lands on a feature branch (per `git-hygiene.md`), then continue.

**Step 1 — Read the design docs (`/read-gdd`)**
Run `/read-gdd` for the target system. "Spec" is not only the GDD — it includes `design-decisions.md` and `game-vision.md` where the change touches a core mechanic or the pillars. It returns a verdict:
- **YES** (the docs already cover the change) → go to Step 3.
- **NO / PARTIAL** (a doc is wrong, missing, or the change alters the design) → go to Step 2.

**Step 2 — Fix the docs (`/write-gdd`)**
Run `/write-gdd` to correct the owning doc(s). It requires a good-enough reason and the user's approval before editing — a doc is the contract, never changed on a whim. The doc now reflects the intended design. Then continue.

**Step 3 — Make the change (`/edit-unity`) — skip if doc-only**
If the task involves code or scene changes, run `/edit-unity` to apply them through the safety checklist (snapshot → change → compile → play-test → save → snapshot). **If the change was doc-only (Step 2 fixed a doc and there is nothing to build), skip this step** and go straight to Step 4.

**Step 4 — Loop or commit**
Ask the user: another change, or commit?
- Another change → return to Step 1.
- Done → "Ready to commit. Run `/make-commit-plan` — it will commit and open the PR. After you review the PR, tell me you're done and I'll run `/reconcile-gdd`."

---

## Exit Condition

The user's described change(s) are in the scene/scripts, compiling, saved, and the snapshot is current. The user is directed to `/make-commit-plan` when ready.

---

## Constraints

- This skill performs no edits itself — it only sequences `/read-gdd`, `/write-gdd`, and `/edit-unity`.
- Never write to Unity without `/read-gdd` first — the gate is mandatory (see `unity-editor.md`).
- One change at a time through the loop — do not bundle unrelated changes into a single `/edit-unity` pass.
- Do not commit or push — that is `/make-commit-plan`'s job.
