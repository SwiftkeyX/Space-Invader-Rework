# Known Issues — Archive

*(bugs moved here once confirmed fixed)*

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
