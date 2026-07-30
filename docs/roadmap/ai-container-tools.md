# Container & Terminal AI Tools

- **Status**: Done
- **Priority**: P1 - quick win; the system prompt currently apologizes that these tools "are not yet exposed" while intermodal is one of our differentiators
- **Effort**: S
- **Category**: AI differentiation / intermodal

## Why

`AIDispatchSystemPrompt` tells the agent to treat `Container` / `OriginTerminal` / `DestinationTerminal`
as "informational metadata." An intermodal-aware agent (only ContainerTrucks, terminal hours, box
availability) is table stakes for the [drayage vertical](drayage-vertical.md) and cheap to ship now.

## What to build

- `GetContainerStatusTool` (`get_container_status`): ISO 6346 lookup → status, current terminal, seal, B/L, linked load. Read tool via the `add-dispatch-tool` skill.
- `GetTerminalInfoTool` (`get_terminal_info`): UN/LOCODE lookup → name, type (port/rail/depot), address for deadhead calcs.
- Update the "Container & Terminal entities" section of `AIDispatchSystemPrompt` to describe the tools and intermodal assignment rules (ContainerTruck-only, factor terminal location into deadhead).
- Both tools appear on the MCP server automatically via `AgentToolRegistry`.
- Later (with drayage vertical): demurrage-deadline awareness in assignment priority.

## Acceptance

Given an unassigned intermodal load, the agent reasons about the container's terminal location in its deadhead comparison and cites container status in its suggestion reasoning.

## Notes

2026-07-25 - shipped. `GetContainerStatusTool` / `GetTerminalInfoTool` accept either the natural key
(ISO 6346 number, UN/LOCODE) or the GUID, and the prompt's "not yet exposed" paragraph is replaced with
intermodal assignment rules.

Two things that were not in the original plan but had to be done:

- **`get_unassigned_loads` now emits `container_number`, `container_iso_type` and the origin/destination
  terminals.** `LoadDto` already carried them; the tool just did not project them. Without that the
  agent has no way to know a load has a container, so it never calls `get_container_status` and the
  acceptance criterion stays unreachable.
- **`Terminal` has no coordinates** - `Address` is a plain value object with no lat/lng, so
  `get_terminal_info` cannot feed `calculate_distance`. The tool description, the prompt and the MCP
  instructions all say so explicitly, and the deadhead anchor stays the load's `origin_lat`/`origin_lng`.
  A test asserts the tool emits no coordinate fields. Giving terminals coordinates is the natural
  follow-up for the drayage vertical.

Case-insensitive lookups use `ToUpper() ==`, not `string.Equals(.., StringComparison.*)` - EF Core
cannot translate that overload and an IDE quick-fix silently turns the query into a runtime failure.
Both call sites carry a comment saying so.
