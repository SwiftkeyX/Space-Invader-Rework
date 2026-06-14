# Known Issues — Archive

*(bugs moved here once confirmed fixed)*

---

### #3 — [Juice] Camera drifts after screen shake

| Field | Value |
|---|---|
| **ID** | #3 |
| **Severity** | Medium |
| **System** | JuiceManager / Camera |
| **Reported** | 2026-06-14 |
| **Status** | Closed |
| **Fixed in** | 2026-06-14 — `JuiceManager.cs`: capture `_basePos` once in `Awake`; remove per-frame `_basePos` overwrite; restore `_cam.transform.position = _basePos` when shake ends |

**Root cause:** `_basePos` was reassigned from `_cam.transform.position` every frame in `Update()`. After each shake applied a random offset, the next frame read the already-offset position as the new base, accumulating drift permanently.  
**Fix:** `_basePos` set once on `Awake`. Shake offsets are always applied relative to this fixed rest position. On shake end, camera is explicitly restored to `_basePos`.

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
