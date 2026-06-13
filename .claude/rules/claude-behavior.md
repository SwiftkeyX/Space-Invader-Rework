# Claude Behavior

## For debugging purpose State what docs, skill, agent you used at the start of every response
**State what you used at the start of every response.** Every response must open with an audit header — even short conversational answers. **The header block itself is never optional. Do not skip it even when all optional lines would be omitted.** Format:

```
Docs read: [filename or "none"] — [one-sentence reason]
Rules applied: [.claude/rules/ filenames active this turn, or "none"]
Rules-for-skill applied: [.claude/rules-for-skill/ filenames consulted this turn, or "none"]
Docs written: [filename] — [one-sentence reason]
Skills used: [skill names]
Agent assigned: [subagent_type — task description]
```

- **Docs read**: list every `.claude/docs/`, `CLAUDE.md`, or `memory/` file actually read this turn. Write `none` if you read nothing.
- **Rules applied**: list every `.claude/rules/` file that actively governed behavior this turn. Write `none` if no rule files were explicitly consulted beyond auto-loaded context.
- **Rules-for-skill applied**: list every `.claude/rules-for-skill/` file consulted this turn (typically only when running a skill that reads these). Write `none` otherwise.
- **Docs written**: list every `.claude/docs/`, `CLAUDE.md`, or `memory/` file actually written or edited this turn. Omit this line entirely if nothing was written.
- **Skills used**: list any skill or sub-agent invoked this turn. Omit this line entirely if nothing was invoked (avoids noisy `none`).
- **Agent assigned**: list any sub-agent spawned via the Agent tool this turn, including its type and a brief task description. Omit this line entirely if no agent was spawned.
- This header exists so omissions are visible: if a doc should have been read, its absence is the signal.

## Verify before changing position

When a user challenges a factual or technical claim:
1. Acknowledge the disagreement
2. Test or verify immediately (run the tool, check the schema, read the docs)
3. Update position based on evidence — not based on the user's confidence level

## Declare verification criteria before every implementation task

Before taking any action that changes code, scenes, files, or configuration, output a verification block:

```
Verification plan:
- What I'm doing: <one sentence — the specific change>
- PASS: <observable outcome that confirms the task worked>
- FAIL: <observable outcome that confirms it did not work or broke something>
```

After completing the task, close with a result statement:

```
Result: PASS — <what was observed>
```
or
```
Result: FAIL — <what was observed and what to try next>
```

**This rule fires for:**
- Any edit to a script, scene, config, or doc file
- Any skill run that produces changes (`/code`, `/debug`, `/production-task`, etc.)
- Any MCP tool call that modifies state (`set_property`, `add_component`, `execute_script`, etc.)

**This rule does NOT fire for:**
- Conversational answers (no changes made)
- Read-only research (Explore agents, doc reads, planning)
- `/log-bug` (logging only, no fix)

**If the task is ambiguous enough that a concrete PASS criterion cannot be stated, ask the user to define it before starting — do not invent one.**
