# Build Notes

> Unity version, target platforms, build steps, and release checklist.

## Project Info

| Field | Value |
|---|---|
| **Unity version** | *(e.g. 6000.0.47f1)* |
| **Render pipeline** | *(e.g. URP, Built-in)* |
| **Target platform(s)** | *(e.g. Windows, WebGL, Android)* |
| **Min OS / browser** | *(e.g. Windows 10, Chrome 120)* |

## Build Steps

1. Open Build Settings (`File > Build Settings`)
2. Confirm target platform matches above
3. Run `check_compile_errors` (coplay) — fix all errors before building
4. Switch platform if needed (`Switch Platform`)
5. Click `Build` — output to `Builds/<platform>/`

## Release Checklist

- [ ] All compile errors resolved
- [ ] Play Mode smoke test passed (main loop, win, lose)
- [ ] Performance pass done (stable frame rate, no GC spikes)
- [ ] Build runs on target device / browser
- [ ] Version number bumped in Player Settings
- [ ] Changelog entry added
