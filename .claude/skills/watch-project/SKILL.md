Scheduled project health monitor. Runs on a loop using ScheduleWakeup to check for compile errors, open bugs, and performance regressions between sessions. Reports only what changed since the last check.

---

## Agent

`claude`

---

## Step 1 — Compile state

Call `check_compile_errors`. If errors exist, report each one and stop — do not proceed to further steps until the project compiles.

## Step 2 — Open bug count

Read `.claude/docs/beta/known-issues.md`. Count real rows in the Open table (ignore the `*(no issues yet)*` placeholder). Report: "Open bugs: N"

## Step 3 — GC health

Call `get_worst_gc_frames`. Report the worst GC allocation found. Flag as a regression if any steady-state frame shows non-zero alloc.

## Step 4 — Delta report

Compare the findings above to the previous invocation's report in this conversation. Report only what changed:

- Compile errors: new errors added, or "cleared"
- Bug count: `+N added` or `−N fixed`, or "unchanged"
- GC: new regression found, or "clean"

If nothing changed: "No regressions since last check."

Always end with the current Phase 3 PIPELINE.md status (how many items remain unchecked).

## Step 5 — Reschedule

Call ScheduleWakeup with:
- `delaySeconds`: 1200
- `prompt`: `/watch-project`
- `reason`: "periodic project health check"

This keeps the loop alive across sessions. To stop the loop, simply do not approve the next ScheduleWakeup call.
