Step-skill: writes systems-design.md, listing every game system with its tier and dependencies. Fourth step in the pre-production sequence.

---

## Agent

`claude`

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/preproduction/game-vision.md` | Read | Derive systems from the intended player experience |
| `.claude/docs/preproduction/design-decisions.md` | Read | Confirm scope before listing systems |
| `.claude/template-docs/preproduction/systems-design.md` | Read | Required structure |
| `.claude/docs/preproduction/systems-design.md` | Read (if exists) + Write | Output doc (for new project bootstrapping only) |
| `.claude/docs/PIPELINE.md` | Read + Write | Tick item on completion |
| `.claude/rules-for-skill/rule-what-to-do-get-block-by-previous-step.md` | Read | When to call /regress instead of patching |
| `.claude/rules-for-skill/rule-pipeline-progression-update.md` | Read | When and how to tick PIPELINE.md |

---

## Entry Condition

`game-vision.md` AND `design-decisions.md` must both exist. If either is missing, call `/regress` on the missing item before proceeding.

---

## Steps

1. Read all input docs in the order listed above
2. Read `systems-design.md` if it exists — note systems already defined
3. Derive the complete system list — every distinct system the game needs, tiered:
   - **Tier 1 — Foundation:** game state, scene management, input handling
   - **Tier 2 — Core Loop:** gameplay mechanics that make the game playable end-to-end
   - **Tier 3 — Supporting:** scoring, UI, audio, power-ups, extras
4. For each system record: name, one-line responsibility, tier, and dependencies (which systems it requires to function)
5. Present the system list to the user and ask for confirmation or changes before writing
6. Write `.claude/docs/preproduction/systems-design.md` following the template structure
7. Update PIPELINE.md: tick `- [x] Fill out systems-design.md`

---

## Exit Condition

`systems-design.md` exists with every system listed, tiered, and dependency-mapped. Every system that will appear in PIPELINE.md Phase 2 is present here. PIPELINE.md item ticked.

---

## Constraints

- Every system must have a tier assignment — no untiered systems
- Every system that appears in Phase 2 of PIPELINE.md must be listed here first
