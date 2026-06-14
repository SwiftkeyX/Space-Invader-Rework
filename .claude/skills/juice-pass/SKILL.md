Step-skill: adds all juice elements — screen shake, particles, hit-stop, SFX, music, UI animations. First step in Phase 3 beta.

---

## Agent

| Category | Agent |
|---|---|
| Screen shake, Particles, Hit-stop, UI animations | `gameplay-programmer` |
| SFX, Music | `audio-engineer` |

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/project-snapshot-index.md` | Read | Current scene hierarchy, scripts, and assets — understand what exists before adding juice |
| `.claude/docs/preproduction/game-vision.md` | Read | Feel pillars and intended player experience to guide juice direction |
| `.claude/docs/preproduction/technical-preferences.md` | Read | Performance budgets — juice must not exceed frame rate or GC limits |
| `.claude/docs/PIPELINE.md` | Read + Write | Tick item on completion |
| `.claude/rules-for-skill/rule-read-write-unity.md` | Read | Compile check, play/stop, save, snapshot — Unity editor workflow |
| `.claude/rules-for-skill/rule-what-to-do-get-block-by-previous-step.md` | Read | When to call /regress instead of patching |
| `.claude/rules-for-skill/rule-pipeline-progression-update.md` | Read | When and how to tick PIPELINE.md |

---

## Entry Condition

Milestone 3 = [x]. `game-vision.md` and `technical-preferences.md` must exist.

---

## Steps

1. Read `game-vision.md` — note the feel pillars and player experience description
2. Read `technical-preferences.md` — note the frame rate and GC alloc budget
3. Implement each juice category below in order. After each: call `check_compile_errors`, then `play_game` to verify, then `stop_game` before moving on:

   | Category | What to implement |
   |---|---|
   | Screen shake | On ball-brick impact and ball-paddle impact |
   | Particles | Brick destruction VFX; power-up pickup burst |
   | Hit-stop | Brief time-scale dip (≤ 0.1s) on high-impact hits |
   | SFX | Ball bounce, brick hit, brick destroy, power-up pickup, game over, level clear |
   | Music | Looping background track; intensity variant or layer change when appropriate |
   | UI animations | Score pop, lives-lost flash, level transition |

4. After all categories are in, run a full `play_game` feel pass — confirm the juice enhances the feel pillars, not fights them. Call `stop_game`.
5. Call `save_scene`
6. Tick PIPELINE.md: `- [x] Juice pass`

---

## Exit Condition

All 6 juice categories implemented and tested. Full play pass confirms feel alignment with `game-vision.md`. PIPELINE.md item ticked.

---

## Constraints

- Each category must compile-check and pass `play_game` before moving to the next
- Check `get_worst_gc_frames` after adding particles — must stay within budget
