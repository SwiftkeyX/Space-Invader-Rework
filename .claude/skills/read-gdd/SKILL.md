**[INTERNAL — reached only via `/code` or `/fix-bug`. Do not invoke directly.]**

Atomic skill — single purpose: read the design docs for a system and report whether the current spec already covers an intended change (yes / no / partial). Read-only — it writes nothing and touches no code. It is the spec-check gate at the start of every code task; `/code` and `/fix-bug` call it first. "Spec" here is not only the GDD — it includes `design-decisions.md` and `game-vision.md` where the change touches a core mechanic or the pillars.

---

## Agent

`claude`

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/production/gdd/<Name>.md` | Read | The contract for the system being changed — primary target |
| `.claude/docs/preproduction/design-decisions.md` | Read | Core-mechanic constraints the change must respect |
| `.claude/docs/preproduction/game-vision.md` | Read | The three pillars the change must align with |
| `.claude/docs/preproduction/best-practices.md` | Read | Project-critical patterns the implementation must follow |

---

## Entry Condition

The user (or a calling skill) describes, in plain language, a change about to be made to a system.

---

## Steps

1. **Identify the target system(s)** the change will touch. Map each to its GDD at `.claude/docs/production/gdd/<Name>.md`.
   - If the change is a brand-new system with no GDD yet → report "No GDD exists for `<Name>` — author one with `/write-gdd` (or `/design-system` in Phase 2) before coding."
2. **Read the contract** — the full GDD for each target system, plus `design-decisions.md`, `game-vision.md`, and `best-practices.md` for the constraints that bound it.
3. **Emit a verdict** for each target system:
   - **YES** — the GDD already describes the intended change correctly → state "GDD is the spec — implement to match" and list the governing sections (SRP, relevant Detailed Design / Interactions / Tuning Knobs rows, Core Rules bullets).
   - **NO / PARTIAL** — the GDD is wrong, incomplete, or the change alters the design (behavior, mechanic, tuning value, cross-system dependency, or responsibility) → state what is missing/wrong and recommend running `/write-gdd` to correct the owning doc before coding.
4. **State the implementation contract** — the GDD sections and best-practices constraints the implementer must follow.

---

## Exit Condition

A verdict (YES / NO / PARTIAL) and the implementation contract have been reported. No doc or code was written.

---

## Constraints

- Read-only — never write or edit any doc, `.cs` file, or `.unity` scene, and never call coplay MCP tools.
- Do not run `/write-gdd` yourself — recommend it; the caller (`/code`, `/fix-bug`, or the user) decides.
- Do not tick PIPELINE.md.
