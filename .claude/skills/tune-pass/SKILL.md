Step-skill: guided iterative tuning loop for feel and difficulty passes. Proposes one value change at a time, runs the game, waits for feedback, and loops until done. Called twice by /beta-task — once for feel, once for difficulty.

---

## Agent

`systems-designer`

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/project-snapshot-index.md` | Read | Current scene hierarchy and scripts — locate exposed fields before auditing values |
| `.claude/docs/preproduction/game-vision.md` | Read | Feel pillars and Difficulty Curve spec — the target for both passes |
| `.claude/docs/PIPELINE.md` | Read + Write | Identify which pass is active; tick item on completion |
| Inspector / ScriptableObject assets | Write | Only output — no script files are modified |
| `.claude/rules-for-skill/rule-read-write-unity.md` | Read | Compile check, play/stop, save, snapshot — Unity editor workflow |
| `.claude/rules-for-skill/rule-what-to-do-get-block-by-previous-step.md` | Read | When to call /regress instead of patching |
| `.claude/rules-for-skill/rule-pipeline-progression-update.md` | Read | When and how to tick PIPELINE.md |

---

## Entry Condition

Phase 3 is active. `game-vision.md` must exist with both feel pillars and a Difficulty Curve section filled.

---

## Step 1 — Identify tuning target

Read `.claude/docs/PIPELINE.md` to determine whether this is a **feel tuning** or **difficulty tuning** pass (whichever is the first unchecked item).

Read `.claude/docs/preproduction/game-vision.md`:
- For **feel tuning**: review the feel pillars (e.g. Chaotic · Explosive · Fun) and intended player experience
- For **difficulty tuning**: review the Difficulty Curve section (level count, HP scaling, speed ramp, pacing targets)

Report which pass is active and quote the relevant section from `game-vision.md`.

---

## Step 2 — Audit current values

Search `Assets/Scripts/` for exposed numeric fields relevant to the tuning target:

- **Feel tuning**: ball speed, ball acceleration, paddle speed, bounce multipliers, hit-stop duration, screen shake intensity, particle counts, power-up timing
- **Difficulty tuning**: brick HP values per level, ball speed progression, power-up drop rates, level layout density

List the top 5 most impactful parameters with their current values and file/field path.

---

## Step 3 — Propose one change

Pick the single parameter most likely to move the needle toward the target. State:

1. Parameter name + path (e.g. `BallController.cs → _initialSpeed`)
2. Current value → proposed value
3. Rationale: which pillar or curve target this addresses and why this direction

Wait for the user to approve before applying. Do not apply multiple changes at once.

---

## Step 4 — Apply and observe

Apply the approved change via `set_property` or direct file edit.

Call `play_game`. Tell the user exactly what to observe — one concrete, testable question (e.g. "Does the ball feel snappier on the first bounce?"). Call `stop_game` when the user is ready to evaluate.

---

## Step 5 — Evaluate and loop

Ask: **"Keep this change? (yes / no / tweak / done)"**

- **yes** — accept the value; return to Step 3 for the next parameter
- **no** — revert the change; return to Step 3 with a different parameter
- **tweak** — stay on the same parameter; propose a refined value in Step 3
- **done** — the tuning pass is complete

On **done**: confirm with the user that the feel or difficulty matches the intent in `game-vision.md`. If confirmed, tick the corresponding PIPELINE.md item. If not, continue from Step 3.

---

## Exit Condition / Gate

User explicitly confirms the pass matches `game-vision.md`. PIPELINE.md item ticked.

---

## Constraints

- One change per iteration — never batch multiple parameter edits between play tests
- Always state what to observe before calling `play_game`
- Never tick PIPELINE.md until the user explicitly confirms the gate passes
