# PowerUpSystem

> **Status**: Draft
> **Last Updated**: 2026-06-13
> **Implements Pillar**: Fun — the roguelike layer and "power fantasy on a timer" that makes every run feel different.

## Summary

PowerUpSystem is the run's roguelike layer. Between levels it offers the player a choice of weapon mods / power-ups (guaranteed by level 3, D4); the chosen effect applies to PlayerShip for the rest of the run (run-scoped, D3). It fires `OnPowerUpOffered` (UIManager shows the pick screen) and `OnPowerUpChosen` (PlayerShip applies the effect). It owns the catalog and the offer/choice flow; it does not own the player's stats — it tells PlayerShip what to change.

> **Quick reference** — Layer: `Feature` · Priority: `Vertical Slice` · Key deps: `LevelManager`, `PlayerShip`, `GameManager`

---

## Overview

After clearing a level, the player picks an upgrade — faster fire, multi-shot, a damage boost, an extra life, etc. — that lasts the rest of the run. This is what makes runs diverge and delivers the "briefly unstoppable, then the difficulty catches up" fantasy. PowerUpSystem builds the offer (a small random set of choices from a catalog), presents it via the UI, applies the chosen effect to PlayerShip, and stacks effects across the run. Everything is run-scoped: a new run starts with no mods (D3, no meta-progression).

## Player Fantasy

The power spike. Each pick should feel like a meaningful build decision and an immediate boost — the player walks into the next level visibly stronger, briefly on top, before the curve catches up. Across runs, different pick paths should feel genuinely different (multi-shot build vs. damage build vs. survivability build).

---

## Detailed Design

### Core Rules

1. **Catalog (ScriptableObjects)**: each power-up is a `PowerUpData` SO defining its id, name, description, icon, and effect parameters. The catalog is data-driven (no hardcoded effect list).
2. **Offer trigger**: subscribe to `LevelManager.OnLevelCleared`. On clear, build an **offer** — a small random selection (default 3) of distinct power-ups from the catalog — and fire `OnPowerUpOffered(offerSet)`. UIManager shows the pick screen.
3. **Guaranteed by level 3 (D4)**: an offer is reliably presented by level 3. *(Whether a power-up is offered after every level or only specific levels is an open tuning question — see Open Questions. Lean: offered after every cleared level 1–5; level 6 clear is the win, no offer.)*
4. **Choice**: the player picks one (via UIManager). PowerUpSystem records it and fires `OnPowerUpChosen(chosen)`. PlayerShip applies the effect.
5. **Run-scoped, stacking (D3)**: chosen effects persist for the rest of the run and stack (e.g. two fire-rate picks stack per the effect's stacking rule). On run start/reset, all power-up state clears — nothing persists between runs, no save (N3).
6. **Effect application boundary**: PowerUpSystem decides *what* was chosen; PlayerShip owns *how* the stat changes (it exposes the modifiable stat surface). PowerUpSystem passes the effect data; PlayerShip applies it. PowerUpSystem never reaches into PlayerShip internals beyond the agreed apply method/event.
7. **Between-level gating**: the next level does not start until a choice is made (coordinated with LevelManager — the offer is a blocking beat, D4). A skip/no-pick option is a design choice (see Open Questions).
8. PowerUpSystem uses no `Find`/`FindObjectOfType`; it communicates via the contract events and an agreed PlayerShip apply path.

### Proposed starter catalog (needs sign-off — see Open Questions)

| Id | Name | Effect | Stacking |
|---|---|---|---|
| `rapid_fire` | Rapid Fire | +fire-rate multiplier (e.g. +25%) | Additive, capped |
| `multi_shot` | Spread Shot | +1 simultaneous bullet (spread) | Additive, capped |
| `power_shot` | Power Shot | +1 projectile damage | Additive |
| `swift` | Thrusters | +move speed | Additive, capped |
| `extra_life` | Repair | +1 life (up to a max) | Additive, capped at life max |
| `pierce` | Piercing Rounds | bullets pierce 1 extra enemy | Additive (ties to Projectile pierce Open Q) |
| `bullet_speed` | Velocity | +player projectile speed | Additive |

> This catalog is a **proposal**, not approved. `design-decisions.md` explicitly defers the exact catalog to Phase 2. Effects map onto the PlayerShip stat surface (fire rate, multi-shot, damage, move speed, projectile speed) + GameManager lives (extra life).

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Idle` | During a level / run start | `OnLevelCleared` | No offer active |
| `Offering` | `OnLevelCleared` | Player picks | Offer set built + `OnPowerUpOffered` fired; awaits choice; next level gated |
| `Applied` | Player picks | Next `OnLevelCleared` | `OnPowerUpChosen` fired; effect applied; run continues |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| LevelManager | Subscribes to `OnLevelCleared` → builds offer; gates next level until chosen. |
| UIManager | `OnPowerUpOffered` → shows pick screen; UIManager reports the player's selection back. |
| PlayerShip | `OnPowerUpChosen` → applies run-scoped stat/weapon modifier. |
| GameManager | Run start resets all power-up state; `extra_life` pick adds a life (via GameManager). |

---

## Formulas

### Effect application (per effect type)

```
playerStat = baseStat (+/×) effectMagnitude, applied per chosen power-up, clamped to caps
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `effectMagnitude` | float/int | per `PowerUpData` | SO | The buff amount (e.g. +0.25 fire-rate mult) |
| caps | per stat | PlayerShip/SO | Prevent runaway stacking (e.g. min fire cooldown) | |

> Concrete magnitudes are authored in `PowerUpData` assets and tuned in Phase 3; PlayerShip enforces caps.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Catalog smaller than offer size | Offer all available distinct items | Don't duplicate or crash a 3-card offer. |
| `extra_life` picked at max lives | Offer it less / convert to alternate, or simply no-op the over-cap | Avoid a wasted/dead pick (tuning, Open Q). |
| Same effect picked twice | Stacks per its stacking rule, clamped to caps | Build-stacking is intended (D4 variety). |
| Run restart | All chosen effects cleared; PlayerShip reset to base | Run-scoped (D3). |
| Player picks during hit-stop / paused transition | Choice resolves normally; uses unscaled-safe UI flow | Pick screen is between levels, not in combat. |
| Level 6 cleared | No offer — that clear is the run win | No upgrade after the final level. |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| LevelManager | This depends on it | State trigger — `OnLevelCleared` triggers offer; gates next level |
| UIManager | Bidirectional | State trigger — `OnPowerUpOffered` out; selection in |
| PlayerShip | It depends on this | Rule dependency — applies effects via `OnPowerUpChosen` |
| GameManager | This depends on it | State trigger — run reset; `extra_life` adds a life |

> Nature options: `Data dependency` · `State trigger` · `Rule dependency` · `Ownership handoff`

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| Offer size | 3 | 2–4 | More choice each level | Fewer, more forced |
| Offered-on levels | 1–5 (every clear) | — | More power gained (easier) | Fewer upgrades (harder) |
| Per-effect magnitudes | per `PowerUpData` | — | Stronger buffs | Weaker |
| Stacking caps | per stat | — | More runaway power | Tighter ceiling |
| `extra_life` max | tied to lives cap | — | More survivability | Less |

> Catalog + magnitudes live in `PowerUpData` SOs; offer rules in a `PowerUpConfig` SO. Primary Phase 3 build-variety/balance surface. No magic numbers.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Offer shown | Pick screen with cards (icon/name/desc) — UI Toolkit | Offer-appears cue | Vertical Slice |
| Card hover/select | Card highlight | Hover/confirm SFX | Alpha |
| Power-up applied | Player buff VFX (glow) + HUD mod indicator | Power-up SFX | Alpha |

> UIManager owns the pick-screen markup (UI Toolkit, `best-practices.md`); PowerUpSystem supplies the offer data.

---

## Game Feel

### Feel Reference

> "Picks should feel like *Slay the Spire* / *Vampire Survivors* level-up choices — meaningful, build-defining, with an immediate power rush. NOT a trivial stat bump you forget you picked."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Card click → chosen + effect applied | ≤ 100 ms | — |

### Animation Feel Targets

| Animation | Startup | Active | Recovery | Feel Goal |
|---|---|---|---|---|
| Pick screen in/out | brief | choice | next wave | A reward beat, snappy not sluggish |
| Apply buff VFX | 0 | brief | — | Instant "I got stronger" |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Power-up chosen | brief | Buff VFX + SFX; player enters next level visibly stronger |

### Weight and Responsiveness

- **Weight**: Meaningful — each pick should feel consequential.
- **Player control**: Full — the player chooses; choice is committed for the run.
- **Snap quality**: Crisp selection, immediate effect.
- **Failure texture**: N/A (no failure), but a "bad" pick should still feel like a build path, not a trap.

### Feel Acceptance Criteria

- [ ] Picks feel build-defining; different paths produce noticeably different runs.
- [ ] The power spike after a pick is felt immediately in the next level.

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Offer cards (icon/name/desc) | Center pick screen (UIManager) | On `OnPowerUpOffered` | Between levels |
| Active mods | HUD indicator (UIManager) | On `OnPowerUpChosen` | When mods active |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| Offer trigger + gating | `LevelManager.md` | `OnLevelCleared`, next-level gate | State trigger |
| Pick screen + selection | `UIManager.md` | `OnPowerUpOffered`, selection callback | State trigger |
| Effect application | `PlayerShip.md` | stat surface, `OnPowerUpChosen` apply | Rule dependency |
| Extra life + run reset | `GameManager.md` | lives add, `OnRunStarted` reset | State trigger |
| Pierce effect | `Projectile.md` | pierce/`Hits` field (Open Q) | Rule dependency |
| Catalog deferral | `design-decisions.md` | D3/D4, catalog-deferred-to-Phase-2 | Rule dependency |

---

## Acceptance Criteria

- [ ] On `OnLevelCleared` (levels 1–5), an offer of N distinct power-ups is built and `OnPowerUpOffered` fires.
- [ ] An offer is guaranteed by level 3 (D4).
- [ ] The player's choice fires `OnPowerUpChosen`; PlayerShip applies the run-scoped effect.
- [ ] Effects stack within a run (capped) and fully reset on a new run (D3); no persistence (N3).
- [ ] The next level does not start until a choice is made.
- [ ] Level 6 clear produces no offer (it is the win).
- [ ] Catalog and magnitudes are `PowerUpData`/SO-driven; no hardcoded effect list.
- [ ] No `Find`/`FindObjectOfType`; communicates via the contract.
- [ ] Performance: offer building is a one-time between-level cost; no per-frame allocation.

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| **Approve the catalog.** Is the 7-item starter catalog above the v1 set? Which to cut/add? `design-decisions.md` defers this to Phase 2 — **needs a decision.** | designer | Before implementation | **Pending — primary sign-off item for this GDD.** |
| Offered after **every** level (1–5) or only at set levels (e.g. 3, 5)? D4 only guarantees "by level 3." | designer | Before implementation | Leaning: every clear 1–5 (more build variety, fits roguelike). |
| Is there a skip/reroll, or is picking mandatory? | designer | Before implementation | Leaning: mandatory pick, no reroll for v1 (simplest). |
| `extra_life` at max lives — suppress, convert, or no-op? | designer | Before implementation | Leaning: drop it from the offer when at max lives. |
| Does `pierce` ship (depends on Projectile pierce Open Q)? | designer / Claude | With Projectile decision | Pending — cut `pierce` if Projectile stays single-hit in v1. |
| Effect-apply mechanism: direct method on PlayerShip vs PlayerShip subscribing to `OnPowerUpChosen` (architecture lists the event). | Claude | Before implementation | Leaning: PlayerShip subscribes to `OnPowerUpChosen` (per architecture); avoid a new direct edge. |
