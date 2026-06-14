Second-pass GDD check on an open PR. After the PR is open and the user has reviewed it (leaving inline comments), this skill reads the PR via `gh` — the diff AND the user's review comments — diffs the changed `.cs` against their GDDs, resolves all divergences, then prompts the user to merge on GitHub (bot cannot merge). Run it when the user says they have finished reviewing.

---

## Agent

`claude`

---

## Entry Condition

A PR must be open for the current branch. If `gh pr view` finds no open PR → output "No open PR for this branch. Run `/open-pr` first, then review it and tell me when you're done." and exit.

Before any `gh` or `git push` call, refresh the bot token:

```
python .claude/scripts/refresh-gh-token.py
```

Report: `✓ Bot token refreshed.`

---

## Step 1 — Load the open PR via `gh`

All `gh` calls use the bot token:

1. Detect the PR for the current branch: `GH_TOKEN=$(cat .gh-token 2>/dev/null) gh pr view --json number,url,title,headRefName,baseRefName`
2. Get the code diff: `GH_TOKEN=$(cat .gh-token 2>/dev/null) gh pr diff <number>` — keep only `.cs` files. If no `.cs` files in the diff → skip to Step 7 (report) and Step 8 (merge).
3. Pull the user's review feedback:
   - PR-level + review comments: `GH_TOKEN=$(cat .gh-token 2>/dev/null) gh pr view <number> --comments`
   - Inline (file/line) review comments: `GH_TOKEN=$(cat .gh-token 2>/dev/null) gh api repos/{owner}/{repo}/pulls/{number}/comments --paginate` — record each as `path : line : body`.
   Index these by file so each changed script carries the user's comments on it.

---

## Step 2 — Map scripts to GDDs

For each changed `.cs` file strip path and extension: `Assets/Scripts/EnemyFormation.cs` → `EnemyFormation`.

Look for `.claude/docs/production/gdd/<Name>.md`. If none exists → log "No GDD for `<Name>.cs` — skipped" and move on.

Collect all matched GDD paths. If none matched → skip to Step 7 (report) and Step 8 (merge).

---

## Step 3 — Read diffs, GDDs, and the user's comments

For each matched pair:

1. The PR diff for that file (from Step 1).
2. The full GDD from `.claude/docs/production/gdd/<Name>.md`.
3. Any of the user's inline/review comments touching that file (from Step 1).

---

## Step 4 — Identify divergences

Compare the code diff against the GDD. Look for:

| Divergence type | Example |
|---|---|
| New public method not in GDD | `public void SpawnDiver()` added but not documented |
| Removed method the GDD specifies | GDD says `ResetGrid()` exists; it was deleted |
| Changed event subscription | GDD says subscribes to `OnPlayerDead`; code now uses `OnGameStateChanged` |
| New cross-system dependency | New `AudioManager.Instance` call not in the Interactions table |
| Changed tuning value | `BaseMarchSpeed` constant changed; GDD lists a different default |
| Behavior contradicting a Core Rules bullet | GDD rule says X; code does Y |

**Weight the user's PR comments** when judging each divergence:
- A user comment **flagging** the relevant line (e.g. "this is wrong", "shouldn't call this here") → lean toward **flag code for fix**.
- A user comment **endorsing** the code (e.g. "this is better", "keep this") → lean toward **update GDD**.

**Ignore:** internal private details, variable renames with no contract change, comments, formatting.

If zero divergences for a GDD → "✓ `<Name>.md` — no divergences." and move on.

---

## Step 5 — Present and resolve each divergence

For each divergence:

```
--- DIVERGENCE: <SystemName> ---
Code:         <what the code now does>
GDD:          <what the GDD currently says>
Your PR note: <the user's inline comment on this, if any — else "none">
Assessment:   <honest 1–2 sentence judgment — improvement, regression, or neutral?>
```

Then ask:

> **Update GDD to match code?** (`yes` / `no` / `skip-all`)
>
> - `yes` — surgically update the relevant GDD section; continue
> - `no` — fix the code to match the GDD; continue
> - `skip-all` — skip remaining; exit with summary of unresolved items

**Never skip past a divergence without an explicit user response.**

---

## Step 6 — Apply resolutions

**For each divergence answered `yes` (update GDD):**
- Edit only the affected GDD section (Interactions table row, a Core Rules bullet, a Tuning Knobs value).
- Do not rewrite unrelated sections.
- Update the `Last Updated` date at the top of the GDD to today's date.

**For each divergence answered `no` (fix code):**
- Apply the targeted fix directly to the `.cs` file to match what the GDD specifies.
- Run `check_compile_errors` — fix any errors before continuing.
- Commit the fix with bot author:
  ```
  git commit --author="space-invader-rework-bot[bot] <4041458+space-invader-rework-bot[bot]@users.noreply.github.com>" -m "fix(<scope>): reconcile-gdd correction"
  ```
- Push using bot token:
  ```bash
  BOT_TOKEN=$(cat .gh-token 2>/dev/null)
  git -c "http.https://github.com/.extraheader=Authorization: Basic $(printf 'x-access-token:%s' "$BOT_TOKEN" | base64 -w 0)" push
  ```

---

## Step 7 — Report summary

```
PR #<number> — <title>
| GDD | Divergences | GDD updated | Code fixed |
|-----|-------------|-------------|------------|
| GameManager.md | 2 | 1 | 1 |
| EnemyFormation.md | 0 | — | — |
```

If any divergences were skipped (`skip-all`), list them and stop — do not proceed to Step 8.

---

## Step 8 — Merge and clean up

All divergences resolved. Ask:

> "All clear — merge PR #<N> and delete the branch? (`yes` / `no`)"

On `yes`:
1. Output the PR URL and instruct the user:
   > "All clear. Merge PR #<N> on GitHub, then delete the branch. Once merged, tell me and I'll sync `main`."
2. **Wait for the user to confirm the merge is done.**
3. Then run:
   ```
   git switch main
   git pull
   ```
4. Report: "`main` is up to date. Next task starts with `/start-branch`."

On `no`: stop; tell the user to merge manually when ready.

> **Note:** PR merge is a human-only action — the bot cannot merge PRs. The user must merge on GitHub directly.

---

## Constraints

- Never edit `.unity` scenes
- Never call coplay MCP tools
- Never merge while any divergences remain unresolved (skipped items block merge)
- Surgical edits only — never rewrite an entire GDD
- Never proceed past a divergence block without an explicit user answer
- Merge is an outward-facing action — always ask for explicit `yes` before running it
