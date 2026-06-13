# Game Vision

## Concept

**Working title:** Space Invader Rework (bullet-hell roguelike)

**One sentence:** A chaotic, explosive reimagining of Space Invaders where you fight escalating invader formations across a short, steep run, picking up weapon mods and power-ups between waves to survive bullet-hell patterns.

## Feel Pillars

> **Chaotic · Explosive · Fun**

These three words are the north star for every feel decision. They are the verification source for the Phase 3 **feel-tuning** gate.

- **Chaotic** — the screen fills with enemies, bullets, and effects. The player is always reacting, never idle. Controlled overwhelm.
- **Explosive** — every kill pops. Heavy juice: screen shake, particles, hit-stop, loud punchy SFX. Destroying a formation feels like a fireworks finale.
- **Fun** — readable despite the chaos, fast to restart, generous power-ups. The roguelike layer makes each run feel different and keeps "one more run" pull.

## Intended Player Experience

Each session the player should feel:

- **Pressure that builds** — start manageable, end frantic. The player ends a run sweating but grinning.
- **Power fantasy on a timer** — power-ups and weapon mods make you briefly feel unstoppable, then the difficulty catches up.
- **Quick stakes** — a full run is short (a single sitting). Death is cheap; the restart loop is instant. Failure invites another attempt, not frustration.
- **Spectacle reward** — clearing a formation or a level delivers a satisfying explosive payoff.

A session = one run: ~10–15 minutes for a full clear, much shorter on an early death.

## Difficulty Curve *(mandatory — Phase 3 difficulty-tuning gate source)*

**Shape:** Short & steep. **6 levels**, each noticeably harder than the last. Full clear ≈ 10–15 minutes. The curve escalates on four axes:

| Level | Enemy HP (per base unit) | Threat speed (formation march + bullet speed) | Fire rate (enemy shots) | Pacing target |
|---|---|---|---|---|
| 1 | 1× (1 HP) | 1.0× march, 1.0× bullet | Sparse — single aimed shots | Onboarding. Learn movement + shooting. ~60–90s. |
| 2 | 1× | 1.15× march, 1.1× bullet | +1 shooter active | Introduce a second bullet stream. ~75s. |
| 3 | 2× (tankier front row) | 1.3× march, 1.25× bullet | Short bursts begin | First real pressure spike. Power-up reliably offered. ~90s. |
| 4 | 2× | 1.5× march, 1.4× bullet | Overlapping streams | Bullet-hell density crosses "chaotic" threshold. ~90s. |
| 5 | 3× | 1.7× march, 1.6× bullet | Dense patterns + spread | Near-overwhelming. Demands a built-up loadout. ~100s. |
| 6 | 4× (mini-boss formation) | 2.0× march, 1.8× bullet | Peak pattern density | Climax. Explosive payoff on clear = run win. ~120s. |

**Scaling patterns (explicit):**

- **Level count:** 6 fixed levels per run.
- **HP scaling:** stepped, not linear — HP roughly doubles at levels 3, 5, and 6, with front-row enemies tankier than back rows to shape the kill order. Base unit = 1 HP at level 1.
- **Threat-speed ramp:** both formation march speed and enemy bullet speed ramp from 1.0× (L1) to ~2.0× march / ~1.8× bullet (L6), accelerating most sharply at levels 4–6. (This is the "ball/threat speed ramp" for this genre — there is no ball; the moving threats are the marching formation and enemy projectiles.)
- **Fire-rate / density ramp:** number of simultaneous enemy shooters and shot patterns increases each level, crossing into true bullet-hell density by level 4.
- **Pacing targets:** each level ~60–120s; total run ~10–15 min; instant restart on death (< 2s back into action). Roguelike power-up/mod choice offered between levels (guaranteed by level 3) to give the player tools to match the spike.

## Target Platform & Audience

- **Platform:** PC (desktop build). **Keyboard + mouse** primary control scheme.
- **Audience:** Arcade fans — players who enjoy fast, juicy, score-and-survival arcade games and short replayable runs.
- **Session length:** short (single-sitting runs), high replayability via the roguelike mod layer.
