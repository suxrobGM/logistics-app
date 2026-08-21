# MCP Server

Expose your fleet dispatch tools to AI assistants like Claude Desktop, Cursor, Windsurf, and any other [Model Context Protocol](https://modelcontextprotocol.io) client. Available on plans where the MCP Server feature is enabled.

## Overview

The MCP server connects an AI tool to live fleet data. Ask Claude what trucks are available, look at load assignments, or kick off a dispatch - all in chat, against your real database.

The server exposes the same tools used by the built-in AI dispatch agent. If the agent can do it, an MCP client can do it too.

## Available Tools

| Tool                           | Type  | Description                                               |
| ------------------------------ | ----- | --------------------------------------------------------- |
| `get_unassigned_loads`         | Read  | All Draft loads not assigned to any trip                  |
| `get_available_trucks`         | Read  | Available trucks with driver HOS data and fleet summary   |
| `get_driver_hos_status`        | Read  | Detailed HOS status for a specific driver                 |
| `check_hos_feasibility`        | Read  | Can a driver complete a trip given HOS remaining?         |
| `batch_check_hos_feasibility`  | Read  | Batch HOS feasibility for multiple driver-load pairs      |
| `calculate_distance`           | Read  | Driving distance between two geographic points            |
| `calculate_assignment_metrics` | Read  | Revenue/mile and deadhead analysis for truck-load pairs   |
| `get_container_status`         | Read  | ISO 6346 lookup: status, terminal, seal, B/L, linked load |
| `get_terminal_info`            | Read  | UN/LOCODE lookup: name, type, country, street address     |
| `search_loadboard`             | Read  | Search DAT/Truckstop/123Loadboard for available loads     |
| `search_loads`                 | Read  | Search loads by status, type, customer, or date range     |
| `get_load`                     | Read  | One load: status, cost, customer, invoice state           |
| `search_customers`             | Read  | Look up customers by name                                 |
| `get_invoices`                 | Read  | List load invoices by load/customer/status/date           |
| `get_invoice`                  | Read  | One invoice: totals, payments, send history               |
| `search_expenses`              | Read  | List expenses by type/status/truck/date                   |
| `get_expense_stats`            | Read  | Expense rollups by category, monthly trend, top trucks    |
| `get_upcoming_maintenance`     | Read  | Trucks with maintenance due (date-based schedules)        |
| `assign_load_to_truck`         | Write | Assign a load to a truck                                  |
| `create_trip`                  | Write | Create a trip from assigned loads                         |
| `dispatch_trip`                | Write | Transition trip to Dispatched status                      |
| `book_loadboard_load`          | Write | Book a load from a load board                             |
| `create_load_invoice`          | Write | Create an unpaid invoice for a load                       |
| `send_invoice`                 | Write | Email an invoice with a payment link                      |
| `create_payment_link`          | Write | Mint a public payment link for an invoice                 |

Write tools come with a confirmation prompt. The AI explains what it's about to do and waits for you to approve before it runs.

## Setup

### 1. Create an API Key

In the TMS Portal, go to **Settings > API Keys** and click **Create API Key**. Give it a descriptive name (e.g., "Claude Desktop"). The key is shown once - copy it immediately.

### 2. Connect Your AI Tool

#### Quick Start (Claude Code)

The repo includes a pre-configured [.mcp.json](../.mcp.json) file. Just replace `<your-api-key>` with your key:

```json
{
  "mcpServers": {
    "logisticsx": {
      "type": "http",
      "url": "http://localhost:7000/mcp",
      "headers": {
        "Authorization": "Bearer <your-api-key>"
      }
    }
  }
}
```

Claude Code auto-discovers this file when you open the project.

#### Claude Desktop

Add to your Claude Desktop config (`claude_desktop_config.json`):

```json
{
  "mcpServers": {
    "logisticsx": {
      "url": "https://api.logisticsx.app/mcp",
      "headers": {
        "Authorization": "Bearer <your-api-key>"
      }
    }
  }
}
```

#### Cursor

In Cursor settings, add an MCP server:

- **Name**: logisticsx
- **Type**: Streamable HTTP
- **URL**: `https://api.logisticsx.app/mcp`
- **Headers**: `Authorization: Bearer <your-api-key>`

#### Windsurf

Add to your Windsurf MCP config (`mcp_config.json`):

```json
{
  "mcpServers": {
    "logisticsx": {
      "serverUrl": "https://api.logisticsx.app/mcp",
      "headers": {
        "Authorization": "Bearer <your-api-key>"
      }
    }
  }
}
```

#### Any MCP Client (Generic)

The server uses **Streamable HTTP** transport at:

```text
POST https://api.logisticsx.app/mcp
Authorization: Bearer <your-api-key>
```

Any MCP client that speaks Streamable HTTP can connect. Legacy SSE clients are handled automatically.

### 3. Local Development

When running locally, the MCP endpoint is available at:

```text
http://localhost:7000/mcp
```

## Example Conversations

Once connected, you can ask your AI assistant questions like:

- "What loads are unassigned right now?"
- "Show me available trucks and their HOS status"
- "Which truck is the best fit for load #1234? Consider distance, deadhead, and HOS"
- "Assign load #1234 to truck #T-100 and create a trip"
- "Search the load board for freight near Chicago, IL"
- "Check if driver John Smith has enough HOS hours for a 500km trip"

## API Key Management

API keys are managed per tenant in the TMS Portal under **Settings > API Keys**.

| Action     | Description                                                                 |
| ---------- | --------------------------------------------------------------------------- |
| **Create** | Generate a new key. The plaintext key is shown once - store it securely.    |
| **List**   | View all keys with name, prefix, creation date, last used date, and status. |
| **Revoke** | Permanently deactivate a key. Revoked keys cannot be reactivated.           |

Key format: `logsx_{tenantId}_{random}` - the tenant ID is embedded so the server can route to the correct database without additional headers.

## Security

- **Authentication**: API keys are hashed with SHA-256 before storage. Only the key prefix is stored for display.
- **Tenant isolation**: Each key is scoped to a single tenant's database. Cross-tenant access is impossible.
- **Rate limiting**: 100 requests per minute per API key.
- **No AI quota**: MCP calls consume no platform LLM tokens (the caller brings their own model), so the weekly AI quota does not apply.
- **Feature gating**: The MCP Server feature must be enabled on the tenant's subscription plan. Without it the endpoint answers 403 rather than exposing a working handshake.
- **Per-tenant catalogue**: `tools/list` returns only the tools the tenant's features allow, so a client never sees a tool it cannot call.
- **Write confirmation**: A write called over MCP executes immediately - there is no dispatcher approval step behind an API key - and its description says so, instructing the client to confirm with its user first.
- **Writes are opt-in**: a tool reaches MCP only by naming `AgentSurfaces.Mcp`. The dispatch writes (`assign_load_to_truck`, `create_trip`, `dispatch_trip`) do. The ones that attribute work to a person (`book_loadboard_load`), email a third party (`send_invoice`, `propose_counter_offer`) or move money (`create_payment_link`) do not, and `tools/call` refuses them by name even though the catalogue never listed them.

## Architecture

The MCP server reuses the tool infrastructure of the built-in AI dispatch agent:

```text
MCP Client (Claude Desktop, Cursor, etc.)
  │
  POST /mcp (Authorization: Bearer logsx_...)
  │
  ├── ApiKeyAuthenticationHandler
  │     Parse tenant ID from key → resolve tenant → validate hash
  │
  ├── McpFeatureGate (endpoint filter)
  │     MCP Server feature enabled for this tenant?
  │
  └── McpToolSurface
        ├── tools/list → IAgentToolRegistry.GetMcpTools(enabled features)
        └── tools/call → IAgentToolRegistry.McpDenialReason() → IAgentToolExecutor.ExecuteToolAsync()
              └── the same tool class the AI agent runs
```

Tool definitions - names, descriptions, schemas - are declared on the tool classes themselves and discovered by `AgentToolCatalog`, which the AI dispatch agent, the copilot and the MCP server all read through `IAgentToolRegistry`. Add a tool and it shows up on every surface.

### Project Structure

```text
src/Presentation/Logistics.McpServer/
├── Registrar.cs                              # DI + MCP SDK + auth + rate limit
├── McpToolSurface.cs                         # tools/list + tools/call over the shared registry
├── McpFeatureGate.cs                         # endpoint filter for the MCP Server feature
└── Authentication/
    ├── ApiKeyDefaults.cs                     # Scheme constants
    └── ApiKeyAuthenticationHandler.cs        # API key validation + tenant resolution
```
