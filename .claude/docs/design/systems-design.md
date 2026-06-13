# Systems Design

> List every system in the game, its single responsibility, what it depends on, and which tier it belongs to. Fill this out as part of Phase 1 before writing any code.

## Systems Table

| System | Responsibility | Depends On | Tier |
|---|---|---|---|
| GameManager | Owns game state enum and core game data (singleton) | — | 1 |
| SceneLoader | Handles all scene transitions | GameManager | 1 |
| InputHandler | Reads raw input and exposes it to other systems | — | 1 |
| *(add systems)* | | | |

## Tier Definitions

| Tier | Label | Must work before… |
|---|---|---|
| 1 | Foundation | Any gameplay can be tested |
| 2 | Core Loop | Win/lose is reachable |
| 3 | Supporting | Content is complete |
