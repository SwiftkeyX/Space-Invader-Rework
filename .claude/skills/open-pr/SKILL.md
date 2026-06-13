Opens a PR for the current feature branch using the bot identity. Auto-invoked by `/make-commit-plan` immediately after it pushes the branch — not normally run by hand. Never pushes — that is `/make-commit-plan`'s job.

---

## Agent

`claude`

---

## Steps

**Step 1 — Branch check**

Run `git branch --show-current`. If it is `main`, stop:

> "You're on `main` — there's nothing to PR from here. Switch to a feature branch first."

---

**Step 2 — Duplicate PR check**

Run `gh pr list --head <branch> --state open --json number`. If a PR already exists, stop:

> "PR #<N> is already open for this branch. Review it on GitHub, then tell me when you're done."

---

**Step 3 — Build PR title and body**

- **Title**: take the first commit message on the branch (`git log main..HEAD --oneline | tail -1 | cut -c9-`).
- **Body**: list all commits since `main` as bullets:
  ```
  git log main..HEAD --oneline
  ```
  Format:
  ```
  ## Summary
  - <commit 1>
  - <commit 2>

  🤖 Opened by Claude via /open-pr
  ```

---

**Step 4 — Open the PR**

```
GH_TOKEN=$(cat .gh-token 2>/dev/null) gh pr create \
  --title "<title>" \
  --body "<body>"
```

Uses bot token from `.gh-token` if it exists; falls back to default `gh` credentials if not.

---

**Step 5 — Hand off to user**

Print the PR URL, then:

> "PR opened: <url>
> Review it on GitHub — leave inline comments on any line you want flagged. When you're done reviewing, tell me and I'll run `/reconcile-gdd`."

---

## Exit Condition

PR is open and URL is printed. User has been told to review and report back.

---

## Constraints

- Never open a PR on `main`
- Never open a duplicate PR
- Never push — push belongs to `/make-commit-plan`
- Never merge — merge belongs to `/reconcile-gdd`
