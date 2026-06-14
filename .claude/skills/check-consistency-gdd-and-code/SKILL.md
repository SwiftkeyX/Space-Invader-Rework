Standalone GDD health-check: audits every `.cs` script in `Assets/Scripts/` against its GDD on demand — no PR required. Run it any time you suspect GDDs have fallen behind the code (e.g. after an architecture pass where `/code`'s GDD gate was bypassed). Identifies divergences and resolves them one by one (update GDD or flag for `/code`). Never modifies `.cs` files.

---

## Agent

`claude`

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `Assets/Scripts/*.cs` | Read | Ground truth — current code state |
| `.claude/docs/production/gdd/*.md` | Read + Write (surgical) | The contract each script is audited against |
| `.claude/docs/preproduction/best-practices.md` | Read | Project-critical patterns used to detect violations |
| `.claude/docs/preproduction/architecture.md` | Read | Communication-patterns contract and per-system SRP |

---

## Entry Condition

Invocable at any time — no open PR required. If `Assets/Scripts/` contains no `.cs` files → report "No scripts found." and exit.

---

## Step 1 — Enumerate scripts

List all `.cs` files under `Assets/Scripts/`. Strip path and extension to get `<Name>` for each.

---

## Step 2 — Map scripts to GDDs

For each `<Name>`, look for `.claude/docs/production/gdd/<Name>.md`.

- **Matched** → collect the pair.
- **Unmatched** → log "No GDD for `<Name>.cs` — skipped." and move on. Creating a missing GDD is not this skill's job; use `/write-gdd`.

If no pairs matched → report and exit.

---

## Step 3 — Read pairs

For each matched pair: read the full `.cs` file AND the full GDD.

Also read `architecture.md` and `best-practices.md` **once** as shared context for all pairs.

---

## Step 4 — Identify divergences per pair

Compare the current code against the GDD. Look for:

| Divergence type | Example |
|---|---|
| Public method in code not in GDD | `public void SpawnDiver()` exists but GDD doesn't document it |
| Method specified in GDD absent from code | GDD says `ResetGrid()` exists; it no longer does |
| Event subscription mismatch | GDD says listens to `OnPlayerDead`; code subscribes to `OnGameStateChanged` |
| Cross-system dependency not in GDD Interactions table | New `AudioManager.Instance` call not listed |
| Tuning value divergence | `BaseMarchSpeed` constant differs from GDD's Tuning Knobs entry |
| Core Rules bullet violated | GDD rule says X; code does Y |
| Architecture-contract violation | Cross-system call not permitted by `architecture.md` communication table |

**Ignore:** private internals, variable renames with no contract change, comments, formatting.

If zero divergences for a pair → output "✓ `<Name>.md` — no divergences." and continue.

---

## Step 5 — Present and resolve each divergence

For each divergence:

```
--- DIVERGENCE: <SystemName> ---
Code:       <what the code currently does>
GDD:        <what the GDD currently says>
Assessment: <1–2 sentence judgment — improvement, regression, or neutral mismatch?>
```

Then ask:

> **Update GDD to match code?** (`yes` / `no` / `skip-all`)
>
> - `yes` — surgically edit only the affected GDD section; continue
> - `no` — note this as a code issue for `/code` to fix; continue
> - `skip-all` — skip remaining divergences; exit with summary of unresolved items

**Never skip past a divergence without an explicit user response.**

---

## Step 6 — Apply resolutions

**For each `yes` (update GDD):**
- Edit only the affected section (Interactions table row, Core Rules bullet, Tuning Knobs value, public API list).
- Do not rewrite unrelated sections.
- Update the `Last Updated` date at the top of the GDD.

**For each `no` (code needs fixing):**
- Do NOT touch the `.cs` file.
- Output: "Code fix needed in `<Name>.cs` — use `/code` to address: `<one-sentence description>`."

---

## Step 7 — Report summary

```
GDD Health Check — <date>
| GDD | Divergences | GDD updated | Code fix needed | Skipped |
|-----|-------------|-------------|-----------------|---------|
| PlayerShip.md | 3 | 2 | 1 | 0 |
| EnemyFormation.md | 0 | — | — | — |
```

If any items were skipped (`skip-all`), list them and remind the user to re-run after addressing them.

---

## Exit Condition

All matched pairs processed (or `skip-all` reached). Summary table reported. GDD updates saved. Code-fix items listed for `/code` follow-up.

---

## Constraints

- **Never modify `.cs` files** — code fixes belong to `/code`, not this skill
- Surgical GDD edits only — never rewrite an entire GDD
- Never call coplay MCP tools
- Never tick `PIPELINE.md`
- Never edit `.unity` scenes
- Never proceed past a divergence block without an explicit user answer
- This skill is a cleanup tool, not a substitute for the `/code` GDD gate — it catches drift after the fact; it does not excuse bypassing `/read-gdd` going forward
