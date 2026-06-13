Step-skill: writes design-decisions.md, deriving key decisions from game-vision.md. Second step in the pre-production sequence.

---

## Agent

`claude`

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/preproduction/game-vision.md` | Read | Source — derive decisions from the vision |
| `.claude/docs/preproduction/design-decisions.md` | Read (if exists) + Write | Output doc |
| `.claude/docs/PIPELINE.md` | Read + Write | Tick item on completion |
| `.claude/rules-for-skill/rule-what-to-do-get-block-by-previous-step.md` | Read | When to call /regress instead of patching |
| `.claude/rules-for-skill/rule-pipeline-progression-update.md` | Read | When and how to tick PIPELINE.md |

---

## Entry Condition

`game-vision.md` must exist. If missing, call `/regress "Fill out game-vision.md" "required before design-decisions"`.

---

## Steps

1. Read `game-vision.md` in full
2. Read `design-decisions.md` if it exists — note sections already filled
3. Derive key design decisions from the vision:
   - Core mechanic constraints (what the game IS)
   - Explicit non-goals (what the game is NOT)
   - Non-obvious choices that need justification (why this approach over alternatives)
   - Scope boundaries (what features are in vs. out)
4. Ask the user to confirm or add decisions before writing
5. Write `.claude/docs/preproduction/design-decisions.md`
6. Update PIPELINE.md: tick `- [x] Fill out design-decisions.md`

---

## Exit Condition

`design-decisions.md` exists covering: core mechanic choices, scope constraints, and explicit non-goals. PIPELINE.md item ticked.

---

## Constraints

- Derive decisions from `game-vision.md` — do not invent decisions the vision does not support
