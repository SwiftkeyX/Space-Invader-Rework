# Known Issues — Archive

*(bugs moved here once confirmed fixed)*

---

### #2 — [UI] Restart button unresponsive on game-over screen after player death

| Field | Value |
|---|---|
| **ID** | #2 |
| **Severity** | Blocker |
| **System** | UI / GameManager restart flow |
| **Reported** | 2026-06-14 |
| **Status** | Closed |
| **Fixed in** | 2026-06-14 — `GameManager.HandleSceneLoaded`: changed label check from `"HUD"` to `"Gameplay"` to match what `SceneLoader` actually fires |

**Root cause:** `GameManager.HandleSceneLoaded` guarded with `if (label != "HUD") return;` but `SceneLoader.LoadGameplay()` fires `OnSceneLoaded` with label `"Gameplay"`. `_uiManager` was never assigned, so `UIManager.OnRestartRequested` had no subscriber and the button did nothing.  
**Fix:** One-line change in `GameManager.cs` line 83: `"HUD"` → `"Gameplay"`.

---

### #1 — Power-up pick screen doesn't pause the game

| Field | Value |
|---|---|
| **ID** | #1 |
| **Severity** | High — player takes damage and can die during card selection |
| **System** | UIManager / PowerUpSystem |
| **Reported** | 2026-06-13 |
| **Status** | Closed |
| **Fixed in** | 2026-06-13 — `UIManager.HandlePowerUpOffered()` sets `Time.timeScale = 0f`; `OnCardSelected()` and `HandleRunStarted()` restore it to `1f` |

**Root cause:** `HandlePowerUpOffered()` set the pick panel visible but never paused time.  
**Fix:** Added `Time.timeScale = 0f` on pick screen entry and `Time.timeScale = 1f` on card selection and run start.
