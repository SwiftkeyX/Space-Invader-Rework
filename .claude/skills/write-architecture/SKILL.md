Step-skill: writes architecture.md, mapping every system to its script, responsibilities, and communication patterns. Fifth step in the pre-production sequence.

---

## Agent

`claude`

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/preproduction/systems-design.md` | Read | System list, tiers, and dependencies |
| `.claude/template-docs/preproduction/architecture.md` | Read | Required structure and communication contract format |
| `.claude/docs/preproduction/architecture.md` | Read (if exists) + Write | Output doc (for new project bootstrapping only — existing projects use GDDs as SoT) |
| `.claude/docs/PIPELINE.md` | Read + Write | Tick item on completion |
| `.claude/rules-for-skill/rule-what-to-do-get-block-by-previous-step.md` | Read | When to call /regress instead of patching |
| `.claude/rules-for-skill/rule-pipeline-progression-update.md` | Read | When and how to tick PIPELINE.md |

---

## Entry Condition

`systems-design.md` must exist. If missing, call `/regress "Fill out systems-design.md" "required before architecture"`.

---

## Steps

1. Read `systems-design.md` and the architecture template
2. Read `architecture.md` if it exists — note sections already filled
3. For each system in `systems-design.md`, define:
   - Script filename (`SystemName.cs`)
   - Single-sentence responsibility
   - Communication pattern: events fired, events listened to, direct references permitted
4. Define the inter-system communication contract:
   - Which systems may call which directly (tight coupling, explicitly allowed)
   - Which must communicate via events only (loose coupling)
5. Write `.claude/docs/preproduction/architecture.md` following the template structure
6. Update PIPELINE.md: tick `- [x] Fill out architecture.md`

---

## Exit Condition

`architecture.md` exists with every system from `systems-design.md` mapped to a script, responsibility, and communication pattern. No "TBD" entries. PIPELINE.md item ticked.

---

## Constraints

- Every system in `systems-design.md` must appear in `architecture.md` — no gaps
- Communication patterns must be explicit — no ambiguous entries
