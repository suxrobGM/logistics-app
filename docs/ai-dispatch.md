# AI-Powered Dispatch

Autonomous and semi-autonomous load-to-truck dispatch powered by the Claude API. Available on the **Enterprise** plan.

## Overview

The agentic dispatcher analyzes your fleet state — unassigned loads, available trucks, driver HOS status, and load board opportunities — then optimizes assignments to maximize fleet utilization while ensuring compliance.

## Operating Modes

### Human-in-the-Loop (Default)

The agent analyzes the fleet and creates **suggestions** that dispatchers review in the TMS portal. Each suggestion includes the agent's reasoning. Dispatchers approve or reject individually or in bulk.

### Autonomous (Experimental)

The agent executes assignments immediately without human approval. Recommended only after validating the agent's suggestions in human-in-the-loop mode. Marked with an experimental badge in the UI.

## How It Works

```text
1. Gather fleet state (loads, trucks, drivers, HOS)
2. Send context + tools to Claude API
3. Agent reasons about optimal assignments
4. Agent calls tools (assign load, create trip, etc.)
   - Human mode: tools create suggestions
   - Autonomous mode: tools execute immediately
5. Agent searches load boards for capacity gaps
6. Session completes with summary
```

## Agent Tools

| Tool | Type | Description |
|------|------|-------------|
| `get_fleet_overview` | Read | Fleet summary: truck count, loads, active trips, violations |
| `get_unassigned_loads` | Read | All Draft loads not in any trip |
| `get_available_trucks` | Read | Available trucks with driver HOS data |
| `get_driver_hos_status` | Read | Detailed HOS for a specific driver |
| `check_hos_feasibility` | Read | Can a driver complete a trip given HOS remaining? |
| `calculate_distance` | Read | Driving distance between two points |
| `optimize_trip_stops` | Read | Optimize stop ordering for multi-load trips |
| `search_load_board` | Read | Search DAT/Truckstop/123Loadboard for opportunities |
| `assign_load_to_truck` | Write | Assign a load to a truck |
| `create_trip` | Write | Create a trip from assigned loads |
| `dispatch_trip` | Write | Transition trip to Dispatched status |
| `book_load_board_load` | Write | Book a load from a load board |

Write tools create suggestions in Human-in-the-Loop mode and execute immediately in Autonomous mode.

## API Endpoints

All endpoints require `Permission.Dispatch.View` or `Permission.Dispatch.Manage` and the `AgenticDispatch` feature to be enabled.

```text
POST   /dispatch/agent/run                         Trigger on-demand agent run
POST   /dispatch/agent/cancel/{sessionId}           Cancel a running session
GET    /dispatch/agent/sessions                     List sessions (paged)
GET    /dispatch/agent/sessions/{sessionId}         Session detail with decisions
GET    /dispatch/agent/pending                      All pending decisions
POST   /dispatch/agent/decisions/{id}/approve       Approve a suggestion
POST   /dispatch/agent/decisions/{id}/reject        Reject a suggestion
```

## Audit Trail

Every agent run creates a **DispatchSession** with:

- Mode (HumanInTheLoop / Autonomous)
- Who triggered it (user or background job)
- Start/end timestamps
- Total tokens consumed
- Agent's summary

Each decision within a session is a **DispatchDecision** with:

- Tool called and input parameters
- Agent's reasoning
- Status (Suggested / Approved / Rejected / Executed / Failed)
- Related entity IDs (load, truck, trip)
- Who approved/rejected and when

## Configuration

### Environment Variables

| Variable | Description |
|----------|-------------|
| `Claude__ApiKey` | Anthropic API key |
| `Claude__Model` | Model to use (default: `claude-sonnet-4-6`) |
| `Claude__MaxTokens` | Max tokens per response (default: 4096) |

### Feature Gating

The feature is gated behind `TenantFeature.AgenticDispatch`, available on the Enterprise plan only. Enable via the admin portal's feature management or by adding a `PlanFeature` entry.

## Background Job

`DispatchAgentJob` runs every 15 minutes via Hangfire. For each tenant it:

1. Checks if `AgenticDispatch` feature is enabled
2. Skips if a session is already running
3. Skips if no unassigned loads exist
4. Runs the agent in Autonomous mode

## Architecture

```text
src/Infrastructure/Logistics.Infrastructure.AI/
├── Registrar.cs                    # DI registration
├── Options/ClaudeOptions.cs        # Configuration
├── Services/
│   ├── ClaudeDispatchAgentService  # Agent loop orchestration
│   ├── DispatchToolExecutor        # Maps tool calls to MediatR
│   └── DispatchToolRegistry        # Tool definitions (MCP-ready)
└── Prompts/
    └── DispatchSystemPrompt        # Dynamic system prompt builder
```

The `IDispatchToolRegistry` defines tools once in a format compatible with both the Claude API and future MCP server expansion. Tool definitions include name, description, and JSON Schema — the same schema format used by MCP.

## Future Roadmap

- **MCP Server**: Expose dispatch tools via Model Context Protocol for use from Claude Desktop/Code
- **Telegram Bot**: Driver and dispatcher interaction via Telegram (accept/reject loads, status updates, fleet summaries)
- **Learning from overrides**: Track dispatcher corrections to improve future suggestions
