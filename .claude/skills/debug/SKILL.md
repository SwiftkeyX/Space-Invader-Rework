Debug router: single entry point when you find a bug. Asks one question and routes to the right skill.

## Agent

`claude`

## Steps

1. Ask the user:
   > "What do you need?
   > **a) Just log it** — you're mid-session and want to capture it without stopping → /log-bug
   > **b) Fix it right now** — you know exactly what's broken → /fix-bug
   > **c) Fix all open bugs** — Phase 3, work through everything in known-issues.md → /fix-all-bugs"

2. Route to the chosen skill.

## Constraints

- Do not attempt any fix or logging before the user answers
- /regress is NOT part of this flow — it is a pipeline management tool, not a debug tool
