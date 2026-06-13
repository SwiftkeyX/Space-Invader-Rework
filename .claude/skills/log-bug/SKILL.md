**[INTERNAL — reached only via `/debug`. Do not invoke directly.]**

Lightweight bug logger: captures one bug immediately without derailing the current session. Writes a structured card to known-issues.md and hands control back. Does NOT fix the bug — fixing is done later via `/debug`.

---

## Agent

`claude`

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.claude/docs/beta/known-issues.md` | Read + Write | Append the new bug card to the Open section |

---

## Steps

**Step 1 — Gather info**

Ask the user in one message (do not ask field-by-field):

> "Describe the bug — fill in all 7 fields:
>
> 1. **Title** — one short sentence: what happens and where? (e.g. "[Ball] Falls through the floor on respawn")
> 2. **Environment** — Unity version, OS, scene name, Play Mode or Build
> 3. **Description** — one sentence on the player/user impact
> 4. **Steps to Reproduce** — numbered list; each step must be specific enough to follow without guessing
> 5. **Expected Result** — what should have happened
> 6. **Actual Result** — what actually happened (observable behaviour, not just "it broke")
> 7. **Evidence** — paste console errors or log lines; if none, write 'none observed'"

Wait for the user's response before proceeding.

**Step 1b — Validate each field**

Before writing anything, check every field against the quality bar below. If any field fails, ask the user to clarify **only the failing fields** in one follow-up message. Keep asking until all 7 fields pass.

| Field | Reject if… | Example follow-up |
|---|---|---|
| Title | Generic ("bug", "doesn't work", "issue") or missing where/what | "The title needs to say what happens and where — e.g. '[Ball] Passes through paddle on fast bounce'." |
| Environment | Missing Unity version, OS, scene name, or Play Mode vs Build | "Which scene and Unity version? Play Mode or a standalone build?" |
| Description | Just restates the title, or missing player/user impact | "Describe the player impact in one sentence — what can't the player do because of this?" |
| Steps to Reproduce | Not numbered, uses vague verbs ("click something"), or skips setup state | "Step N is too vague to follow. What exactly did you do, and what was the game state at that moment?" |
| Expected Result | Blank, or 'it should work' without specifics | "What specifically should have happened — what outcome were you expecting?" |
| Actual Result | Vague ("it broke", "it crashed"), missing observable behaviour | "Describe exactly what you saw — what did the game do?" |
| Evidence | Skipped without explanation | "Paste any console errors or Unity log output. If there were none, write 'none observed'." |

**Step 2 — Assign issue number**

Read `known-issues.md`. Count the real entries in Open, Fixed, and Won't Fix sections (ignore placeholder lines). The new issue number = total real entries + 1.

**Step 3 — Append card to Open section**

Add a new card to the Open section using this format:

```markdown
### #N — <Title>
- **Environment:** <value>
- **Description:** <value>
- **Steps to Reproduce:**
  1. <step>
  2. <step>
- **Expected:** <value>
- **Actual:** <value>
- **Evidence:** <value>
- **Area:** <infer from title/description>
- **Severity:** Blocker / Major / Minor / Cosmetic
```

Remove the placeholder `*(no open issues yet)*` line if it is still present.

**Step 4 — Confirm and resume**

Report: "Bug #N logged. Resume what you were doing — run `/debug` when ready to fix it."

---

## Constraints

- Never write the bug card until all 7 fields pass the quality bar in Step 1b
- Never attempt to fix the bug — log only
- Keep the Step 4 confirmation to one line
