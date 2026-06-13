Step-skill: writes game-vision.md, the foundational creative vision doc. First step in the pre-production sequence.

---

## Agent

`claude`

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/preproduction/game-vision.md` | Read (if exists) + Write | Output doc — read first to resume partial work |
| `.claude/docs/PIPELINE.md` | Read + Write | Tick item on completion |
| `.claude/rules-for-skill/rule-what-to-do-get-block-by-previous-step.md` | Read | When to call /regress instead of patching |
| `.claude/rules-for-skill/rule-pipeline-progression-update.md` | Read | When and how to tick PIPELINE.md |

---

## Entry Condition

Phase 1 is active. No prior doc required — this is the first step.

---

## Steps

1. Read `.claude/docs/preproduction/game-vision.md` if it exists — note any sections already filled
2. Ask the user for:
   - Game name and one-sentence concept
   - Core feel pillars (e.g. "Chaotic · Explosive · Fun")
   - Intended player experience — what should the player feel each session?
   - Difficulty curve intent — level count, escalation pattern, pacing
   - Target platform and audience
3. Write `.claude/docs/preproduction/game-vision.md` covering all of the above. The **Difficulty Curve** section is mandatory — it is the verification source for the Phase 3 difficulty tuning gate
4. Update PIPELINE.md: tick `- [x] Fill out game-vision.md`

---

## Exit Condition

`game-vision.md` exists with all sections filled and no placeholders. Difficulty Curve section is present. PIPELINE.md item ticked.

---

## Constraints

- Never invent feel pillars or difficulty intent — ask the user
- The Difficulty Curve section must include: level count, HP scaling pattern, ball speed ramp, and pacing targets
