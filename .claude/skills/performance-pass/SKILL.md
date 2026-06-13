Step-skill: profiles the game and optimizes scripts until GC allocs and frame times are within budget.

---

## Agent

`gameplay-programmer`

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/project-snapshot-index.md` | Read | Current scene hierarchy and scripts — full inventory of scripts to identify hotspot candidates |
| `.claude/docs/preproduction/technical-preferences.md` | Read | Performance budgets: target frame rate, GC alloc limit, draw call budget |
| Affected `Assets/Scripts/*.cs` | Read + Write | Scripts being optimized |
| `.claude/docs/PIPELINE.md` | Read + Write | Tick item when budgets are met |
| `.claude/rules-for-skill/rule-read-write-unity.md` | Read | Compile check, play/stop, save, snapshot — Unity editor workflow |
| `.claude/rules-for-skill/rule-what-to-do-get-block-by-previous-step.md` | Read | When to call /regress instead of patching |
| `.claude/rules-for-skill/rule-pipeline-progression-update.md` | Read | When and how to tick PIPELINE.md |

---

## Entry Condition

Phase 3 is active. `technical-preferences.md` must exist with defined budget values.

---

## Steps

1. Read `technical-preferences.md` — record exact budget values (ms per frame, GC alloc target, draw call limit)
2. Call `get_worst_gc_frames` — identify the worst GC allocation frames
3. Call `get_worst_cpu_frames` — identify the worst CPU frames
4. For each offending script (worst offender first):
   a. Read the script
   b. Fix the hotspot: cache components in Awake, eliminate per-frame allocations, reduce draw calls as needed
   c. Call `check_compile_errors`
   d. Re-run `get_worst_gc_frames` and `get_worst_cpu_frames` to verify improvement
5. Repeat until all steady-state frames show zero GC alloc and all frames are within the ms budget
6. Tick PIPELINE.md: `- [x] Performance pass`

---

## Exit Condition / Gate

`get_worst_gc_frames` shows zero alloc in steady-state gameplay. All frames within the budget defined in `technical-preferences.md`. PIPELINE.md item ticked.

---

## Constraints

- Never tick before re-profiling — verify with tools, not by code inspection
