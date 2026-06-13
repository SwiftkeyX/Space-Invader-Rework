# Game Vision Document

> Full creative vision — what this game is. Genre, pillars, mechanics, art/audio direction. Fill every section before development begins. Claude reads this to stay aligned with your vision throughout the project.

## Overview

| Field | Value |
|---|---|
| **Title** | *(working title)* |
| **Genre** | *(e.g. 2D platformer, top-down shooter, puzzle)* |
| **Platform** | *(e.g. PC, WebGL, mobile)* |
| **Target audience** | *(e.g. casual, hardcore, kids)* |
| **Estimated scope** | *(e.g. game jam prototype, vertical slice, full release)* |

## Design Pillars

Three qualities that every decision must serve. If a feature doesn't serve at least one pillar, cut it.

1. *(e.g. "Fast — every action feels snappy and responsive")*
2. *(e.g. "Readable — the player always knows what's happening")*
3. *(e.g. "Surprising — encounters feel emergent, not scripted")*

## Core Loop

Describe the moment-to-moment and session loop:

```
[Action] → [Feedback] → [Reward] → [repeat]
```

*(e.g. Shoot enemies → earn coins → upgrade weapon → face harder enemies → repeat)*

## Mechanics

### Player

| Mechanic | Description |
|---|---|
| Movement | *(e.g. WASD + mouse aim, top-down)* |
| Primary action | *(e.g. left-click to shoot)* |
| Secondary action | *(e.g. right-click to dash)* |
| Resource | *(e.g. health, ammo, mana)* |

### Enemies / Obstacles

| Type | Behavior |
|---|---|
| *(Enemy A)* | *(describe AI behavior)* |
| *(Enemy B)* | *(describe AI behavior)* |

### Progression

*(How does the player get stronger or the game get harder? e.g. wave scaling, unlocks, level-ups)*

## Win / Lose Conditions

| Condition | Trigger |
|---|---|
| **Win** | *(e.g. survive 10 waves, reach the exit, score 10,000 points)* |
| **Lose** | *(e.g. health reaches 0, time runs out)* |

## Levels / Scenes

| Scene | Purpose |
|---|---|
| `MainMenu` | Title screen, start / settings / quit |
| `Game` | Core gameplay |
| *(add scenes)* | |

## Art Direction

| Aspect | Direction |
|---|---|
| **Style** | *(e.g. pixel art, low-poly 3D, flat vector)* |
| **Palette** | *(e.g. limited 16-color, vibrant neons, muted earth tones)* |
| **Camera** | *(e.g. orthographic 2D, perspective 3D, fixed isometric)* |
| **Resolution / aspect** | *(e.g. 1920×1080, 16:9 locked)* |

## Audio Direction

| Aspect | Direction |
|---|---|
| **Music style** | *(e.g. chiptune, orchestral, lo-fi ambient)* |
| **SFX style** | *(e.g. punchy arcade, realistic, minimal)* |
| **Key audio moments** | *(e.g. hit feedback, level-up fanfare, death sting)* |

## Out of Scope

List features explicitly excluded to prevent scope creep:

- *(e.g. multiplayer)*
- *(e.g. procedural generation)*
- *(e.g. controller support)*
