Audit all `.md` files in this project for broken path references and stale doc-index content, then report findings and offer to fix them.

---

## Agent

`claude`

---

## Step 1 — Discover all .md files

Use Glob with pattern `**/*.md` from the project root. This is your working set.

---

## Step 2 — Extract path references from each file

Read each .md file. Scan every line for these patterns:

**Pattern A — backtick paths**
Any `` `path` `` where the path contains `/` or ends in `.md`, `.unity`, `.cs`, `.prefab`, `.asset`, `.uxml`, `.uss`, or `.inputactions`. Extract the path inside the backticks.

**Pattern B — markdown links**
Any `[text](path)` where path does not start with `http`. Strip any `#anchor` suffix before using the path.

**Pattern C — bare prose paths**
Segments starting with `docs/`, `.claude/`, or `Assets/` that are not already captured by Pattern A or B.

**Skip entirely:**
- Paths inside `*(e.g. ...)` italic spans — these are template examples, not live references
- Paths inside `<!-- ... -->` HTML comments
- Bare filenames with no `/` and no known extension (cannot resolve)

For each extracted path record: source file, line number, raw path string, pattern type.

---

## Step 3 — Resolve each path to an absolute path

- Path starts with `docs/`, `.claude/`, or `Assets/` → project-root-relative
- Path starts with `../` or is a bare filename → relative to the containing file's directory
- Path ends with `/` → directory reference (handle separately in Step 4)

---

## Step 4 — Verify existence

For each resolved path:
- File path: Glob the exact path. No match → **BROKEN**
- Directory path: Glob `<path>*`. No results → **BROKEN DIRECTORY**

Collect all broken references (source file, line number, raw text, resolved path).

---

## Step 5 — Stale content audit

Check four structural sections that catalog files and drift after reorganizations:

**5a. `.claude/docs/index.md` entry list**
- Read `.claude/docs/index.md` and extract every path reference (Pattern A)
- Verify each listed path exists on disk
- Run `Glob .claude/docs/**/*.md` and `Glob .claude/docs/*.md` — flag any doc file NOT referenced in `index.md` as a gap (newly added file not yet indexed)

**5b. `README.md` file tables**
- For each table row, verify the path in column 1 exists
- Check each table's directory scope for unlisted files

**5c. `.claude/docs/PIPELINE.md` checked tasks only**
- For `- [x]` items that reference a file path: verify the file exists. A checked task referencing a missing file means the file was moved after completion.
- Do NOT flag `- [ ]` items — future tasks referencing files not yet created is expected and correct.

**5d. `.claude/template-docs/other/onboarding.md`**
- Verify all `docs/` paths and `Assets/` paths in the Setup Steps section exist

---

## Step 6 — Report findings

Output a structured report. Skip any section that has zero findings.

### Broken Path References

Group by source file:

```
FILE: .claude/template-docs/other/onboarding.md
  Line 18  `docs/build-notes.md`  → [FILE NOT FOUND]
            Likely intended: docs/beta/build-notes.md
```

When a broken path has exactly one Glob match by filename anywhere in the project, add a "Likely intended:" line.

### Stale Content Gaps

```
CLAUDE.md doc index — files on disk not listed:
  docs/other/changelog.md

README.md — rows for files that no longer exist:
  (none)
```

### Agent Files (informational — not auto-fixable)

List any broken paths found in `.claude/agents/*.md` files. Flag them but note they require manual review.

### Summary

```
Broken references: N  (M auto-fixable, K manual)
Stale gaps:        N
Agent flags:       N
Files scanned:     N
```

---

## Step 7 — Offer fixes

After the report, ask:

> **Fix broken references?** Auto-fixable = exactly one filename match on disk exists.
> Reply: `yes` / `no` / `list` (show substitutions before applying)

Then ask separately:

> **Update CLAUDE.md index and README.md tables** to match current disk state?
> Reply: `yes` / `no`

---

## Step 8 — Apply reference fixes (if user said yes)

For each auto-fixable broken reference:
1. Re-read the source file
2. Edit: replace only the broken path string with the corrected one
3. Preserve surrounding syntax — keep backticks, keep `[text](...)` wrapper
4. Confirm: "Fixed `<old>` → `<new>` in <file> line N"

Do not touch paths flagged MANUAL (zero matches or multiple matches).

---

## Step 9 — Update index sections (if user said yes)

**`.claude/docs/index.md` entry list:**
1. Read `.claude/docs/index.md`
2. Build corrected entry list: keep existing valid entries, remove entries for missing files, add entries for new files (extract description from the file's first `#` heading line; add a "Consult when:" trigger line)
3. Edit to replace only the affected entry — leave all surrounding content untouched

**README.md tables:**
1. Read README.md
2. For each table: remove rows for missing files, add rows for new files using the same heading-extraction approach
3. Edit each affected table block individually

---

## Constraints

- **Never edit `docs/PIPELINE.md`** — it is a phase tracker; editing task descriptions requires human judgment
- **Never auto-edit `.claude/agents/*.md`** — agent instructions are behavioral contracts; report only
- **Never create files**
- **Resolve relative paths from the containing file's directory**, not the project root
- Change only the broken path string — nothing else on the line
