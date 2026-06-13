# [System / Mechanic Name]

<!-- Copy this file, rename it to match your system (e.g. player-movement.md, enemy-ai.md). -->
<!-- Fill every section before implementation begins. Agents read this before touching the system. -->

> **Status**: Draft | In Review | Approved | Implemented
> **Last Updated**: [Date]
> **Implements Pillar**: [Which design pillar from game-vision.md this supports]

## Summary

[2–3 sentences: what this system is, what it does for the player, and why it exists. Write for fast scanning — an agent reading 10 GDDs uses this to decide whether to read further.]

> **Quick reference** — Layer: `Foundation | Core | Feature | Presentation` · Priority: `MVP | Vertical Slice | Alpha | Full Vision` · Key deps: `[system names or "None"]`

---

## Overview

[One paragraph for someone unfamiliar with the project. What is this, what does the player do, and why does it exist in this game?]

## Player Fantasy

[What should the player FEEL when engaging with this system? What emotional or power fantasy does it serve? This guides all detail decisions below.]

---

## Detailed Design

### Core Rules

[Precise, unambiguous rules. A programmer should be able to implement this section without asking questions. Use numbered rules for sequential processes, bullets for properties.]

1. *(Rule 1)*
2. *(Rule 2)*

### States and Transitions

[Document every state and every valid transition. If this system has no states, remove this section.]

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| *(Idle)* | *(default)* | *(input received)* | *(describe)* |

### Interactions with Other Systems

[How does this system interact with others? For each interaction specify: what data flows in, what flows out, who is responsible for what.]

| System | Interaction |
|---|---|
| *(GameManager)* | *(describe)* |

---

## Formulas

[Every formula used by this system. Remove this section if none.]

### [Formula Name]

```
result = base_value * modifier
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `base_value` | float | *(e.g. 1–100)* | *(ScriptableObject / Inspector)* | *(describe)* |
| `modifier` | float | *(e.g. 0.5–2.0)* | *(calculated)* | *(describe)* |

**Expected output range**: [min] to [max]
**Edge cases**: *(e.g. clamp to 0 to prevent negative results)*

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| *(e.g. health reaches exactly 0)* | *(trigger death, not negative health)* | *(describe why)* |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| *(GameManager)* | This depends on it | *(e.g. reads game state enum)* |
| *(UIManager)* | It depends on this | *(e.g. subscribes to health changed event)* |

> Nature options: `Data dependency` · `State trigger` · `Rule dependency` · `Ownership handoff`

---

## Tuning Knobs

[Every value that a designer should be able to adjust for balancing. All must be exposed via Inspector or ScriptableObject — no magic numbers.]

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| *(e.g. MoveSpeed)* | *(5.0)* | *(2–10)* | *(faster, harder to control)* | *(slower, easier to control)* |

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| *(e.g. player hit)* | *(flash red)* | *(hurt SFX)* | *(MVP / Alpha / Polish)* |

---

## Game Feel

### Feel Reference

[Name a specific game and mechanic that captures the target feel. Be precise — cite the exact mechanic, not just the game.]

> "Should feel like *(Game X)*'s *(specific mechanic)* — *(what quality you're borrowing)*. NOT *(anti-reference)*."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| *(Primary action)* | *(e.g. 50ms)* | *(e.g. 3 frames)* |

### Animation Feel Targets

| Animation | Startup Frames | Active Frames | Recovery Frames | Feel Goal |
|---|---|---|---|---|
| *(e.g. attack)* | | | | *(e.g. snappy, low commitment)* |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| *(e.g. hit-stop)* | *(e.g. 80ms)* | *(e.g. freeze both objects on contact)* |

### Weight and Responsiveness

- **Weight**: *(Heavy and deliberate / Light and reactive?)*
- **Player control**: *(Can the player course-correct mid-action, or is it committed?)*
- **Snap quality**: *(Crisp and binary / Smooth and analog?)*
- **Failure texture**: *(When the player fails, does it feel fair? Can they read why?)*

### Feel Acceptance Criteria

- [ ] *(e.g. "Combat feels impactful — playtesters comment on weight unprompted")*
- [ ] *(e.g. "No reviewer uses the words 'floaty' or 'unresponsive'")*

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| *(e.g. current health)* | *(HUD top-left)* | *(on change)* | *(always visible)* |

---

## Cross-References

[Every dependency on another system's specific mechanic, value, or rule. Declare it here even if mentioned in prose above.]

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| *(e.g. "damage feeds score")* | `docs/gdd/score.md` | `combo_multiplier` output | Data dependency |

---

## Acceptance Criteria

[Testable criteria confirming this system works as designed.]

- [ ] *(Specific, measurable, testable criterion)*
- [ ] *(Another criterion)*
- [ ] Performance: system update completes within *(X)*ms per frame
- [ ] No hardcoded values in implementation — all tuning knobs exposed via Inspector

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| *(e.g. Should dash have iframes?)* | *(designer)* | *(date)* | *(pending)* |
