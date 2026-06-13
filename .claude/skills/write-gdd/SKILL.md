**[INTERNAL — reached only via `/code` or `/fix-bug`. Do not invoke directly.]**

Atomic skill — single purpose: surgically update a design doc (GDD / design-decisions / game-vision / best-practices) so it reflects a changed design. Writes docs only; never touches code. Called by `/code` and `/fix-bug` when `/read-gdd` finds the spec wrong. A doc is never changed on a whim — the change must carry a good-enough reason and the user's approval first.

---

## Agent

`claude`

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/preproduction/game-vision.md` | Read | Verify the change aligns with the three pillars |
| `.claude/docs/preproduction/design-decisions.md` | Read / Write if mechanic change | Fundamental mechanic constraints |
| `.claude/docs/preproduction/best-practices.md` | Read / Write if coding pattern changes | Project-critical code patterns |
| `.claude/docs/production/gdd/<System>.md` | Read / Write | Per-system GDD: SRP, communication patterns, effects, values — primary target for most changes |
| `.claude/rules/doc-conventions.md` | Read | Doc-ownership routing table — which doc owns which change |

---

## Entry Condition

A design change is described in plain language, e.g.:
- "Replace SpeedBoost with SlowBall"
- "Drop chance should be 30% not 55%"
- "Ball should not bounce off the side walls"

---

## Steps

1. Read `game-vision.md` and the doc-ownership routing table in `doc-conventions.md`
2. Identify which doc(s) own the data being changed using the routing table
3. Read those target docs
4. Verify the proposed change does not contradict the three game pillars. If it does, flag the conflict to the user before proceeding
4b. **Justify and get approval** — present the change together with a one-line reason for *why the doc must change* (a real design rationale, not merely "the code does X now"). Get the user's explicit approval. A doc is the contract — do not edit it on a casual request. The higher the doc (GDD < design-decisions < game-vision), the stronger the reason required
5. Edit the target doc(s) — update tables, tuning knobs, acceptance criteria, or constraint prose. Be surgical: change only what the request covers
   5b. **If a communication pattern changed** (new event, new method call, or changed dependency between systems): update the "Interactions with Other Systems" section in every affected GDD — both the calling and the receiving system. Scene lifecycle rule → update `SceneLoader.md`. Singleton rule → update `GameManager.md`.
6. Report what was changed and in which doc(s).

---

## Exit Condition

All target docs updated. The doc now reflects the intended design. No code was written.

---

## Constraints

- Never write or edit any `.cs` file
- Never edit `.unity` scenes
- Never call coplay MCP tools
- Never tick PIPELINE.md
- One change at a time — if the request spans multiple unrelated changes, handle them sequentially and confirm between each
