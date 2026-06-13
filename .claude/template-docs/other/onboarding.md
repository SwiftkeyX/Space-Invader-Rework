# Onboarding

> How to get the project running from a fresh clone.

## Prerequisites

| Requirement | Version |
|---|---|
| Unity Hub | latest |
| Unity Editor | *(see `.claude/docs/beta/build-notes.md`)* |
| Claude Code CLI | latest |
| coplay MCP | installed and connected |

## Setup Steps

1. Clone the repo
2. Open Unity Hub → `Add project from disk` → select this folder
3. Open with the Unity version listed in `.claude/docs/beta/build-notes.md`
4. Wait for asset import to complete
5. Open the starting scene: `Assets/Scenes/MainMenu.unity`
6. Press Play to verify the project runs

## Claude Code Setup

1. `cd` into this directory
2. Run `claude` to start a session
3. Claude will read `CLAUDE.md` automatically on first message
4. Run `/check-pipeline-stage` to see current project status

## First Run Checks

- [ ] Project opens without errors in Unity Console
- [ ] No compile errors (`check_compile_errors` via coplay)
- [ ] Play Mode enters and exits cleanly
