# Architecture Contract

> The authoritative record of how scripts in this project communicate. Fill this in as systems are implemented. Claude reads this before touching any existing script.
>
> For system responsibilities, dependencies, and tier assignments, see `.claude/docs/preproduction/systems-design.md`.
> For coding conventions and anti-patterns, see `coding-style.md`.

## Communication Patterns

Define how scripts talk to each other. Establish this once and enforce it.

| From | To | Method | Notes |
|---|---|---|---|
| *(System A)* | *(System B)* | *(C# event / Direct method call / etc.)* | *(any notes)* |

**Rule**: only the communication methods listed above are permitted. No ad-hoc `Find`/`FindObjectOfType` chains.
