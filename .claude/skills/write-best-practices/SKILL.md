Step-skill: writes best-practices.md, capturing project-critical coding patterns that override all defaults. Sixth and final step in the pre-production sequence.

---

## Agent

`claude`

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/preproduction/architecture.md` | Read | Communication patterns become hard rules (frozen artifact) |
| `.claude/template-docs/preproduction/best-practices.md` | Read | Required structure |
| `.claude/docs/preproduction/best-practices.md` | Read (if exists) + Write | Output doc |
| `.claude/docs/PIPELINE.md` | Read + Write | Tick item and Milestone 0 on completion |
| `.claude/rules-for-skill/rule-what-to-do-get-block-by-previous-step.md` | Read | When to call /regress instead of patching |
| `.claude/rules-for-skill/rule-pipeline-progression-update.md` | Read | When and how to tick PIPELINE.md |

---

## Entry Condition

`preproduction/architecture.md` must exist. If missing, call `/regress "Fill out preproduction/architecture.md" "required before best-practices"`.

---

## Steps

1. Read `architecture.md` and the best-practices template
2. Read `best-practices.md` if it exists — note patterns already defined
3. Derive project-critical rules from the architecture:
   - Communication rules (e.g. "GameManager must never call AudioManager directly — use events")
   - Unity-specific patterns (e.g. "Never use FindObjectOfType at runtime — cache in Awake")
   - Project invariants (e.g. "Ball must never stop moving")
   - Performance rules aligned with `technical-preferences.md` budgets
4. Ask the user if there are additional critical rules before writing
5. Write `.claude/docs/preproduction/best-practices.md` following the template structure — project-critical section must be prominent
6. Update PIPELINE.md:
   - Tick `- [x] Fill out best-practices.md`
   - If all 6 Phase 1 docs are now checked, tick `- [x] Milestone 0 — vision complete...`

---

## Exit Condition

`best-practices.md` exists with a project-critical patterns section. Milestone 0 ticked in PIPELINE.md.

---

## Constraints

- Every rule in the project-critical section is a hard constraint, not a suggestion
