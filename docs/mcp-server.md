# MCP Server

Expose your fleet dispatch tools to AI assistants like Claude Desktop, Cursor, Windsurf, and other MCP-compatible clients via the [Model Context Protocol](https://modelcontextprotocol.io). Available on plans with the **MCP Server** feature enabled.

## Overview

The MCP server lets your team connect AI tools directly to fleet data. Ask Claude to check available trucks, analyze load assignments, or dispatch trips — all through natural conversation, with your live fleet data.

The server exposes the same dispatch tools used by the built-in AI agent, so any tool available to the autonomous dispatcher is also available via MCP.

## Available Tools

| Tool | Type | Description |
|------|------|-------------|
| `get_unassigned_loads` | Read | All Draft loads not assigned to any trip |
| `get_available_trucks` | Read | Available trucks with driver HOS data and fleet summary |
| `get_driver_hos_status` | Read | Detailed HOS status for a specific driver |
| `check_hos_feasibility` | Read | Can a driver complete a trip given HOS remaining? |
| `batch_check_hos_feasibility` | Read | Batch HOS feasibility for multiple driver-load pairs |
| `calculate_distance` | Read | Driving distance between two geographic points |
| `calculate_assignment_metrics` | Read | Revenue/mile and deadhead analysis for truck-load pairs |
| `search_loadboard` | Read | Search DAT/Truckstop/123Loadboard for available loads |
| `assign_load_to_truck` | Write | Assign a load to a truck |
| `create_trip` | Write | Create a trip from assigned loads |
| `dispatch_trip` | Write | Transition trip to Dispatched status |
| `book_loadboard_load` | Write | Book a load from a load board |

Write tools include a confirmation prompt — the AI assistant will explain what it's about to do and ask for your approval before executing.

## Setup

### 1. Create an API Key

In the TMS Portal, go to **Settings > API Keys** and click **Create API Key**. Give it a descriptive name (e.g., "Claude Desktop"). The key is shown once — copy it immediately.

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

Any MCP client that supports Streamable HTTP can connect. The endpoint also supports legacy SSE connections automatically.

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

| Action | Description |
|--------|-------------|
| **Create** | Generate a new key. The plaintext key is shown once — store it securely. |
| **List** | View all keys with name, prefix, creation date, last used date, and status. |
| **Revoke** | Permanently deactivate a key. Revoked keys cannot be reactivated. |

Key format: `logsx_{tenantId}_{random}` — the tenant ID is embedded so the server can route to the correct database without additional headers.

## Security

- **Authentication**: API keys are hashed with SHA-256 before storage. Only the key prefix is stored for display.
- **Tenant isolation**: Each key is scoped to a single tenant's database. Cross-tenant access is impossible.
- **Rate limiting**: 100 requests per minute per API key.
- **Feature gating**: The MCP Server feature must be enabled on the tenant's subscription plan.
- **Write confirmation**: Write tools instruct the AI to explain and confirm before executing.

## Architecture

The MCP server reuses the same tool infrastructure as the built-in AI dispatch agent:

```text
MCP Client (Claude Desktop, Cursor, etc.)
  │
  POST /mcp (Authorization: Bearer logsx_...)
  │
  ├── ApiKeyAuthenticationHandler
  │     Parse tenant ID from key → resolve tenant → validate hash
  │
  └── DispatchMcpTool (one per tool definition)
        ├── Feature gate check (MCP Server + Load Board)
        └── IDispatchToolExecutor.ExecuteToolAsync()
              └── IDispatchTool implementation (same as AI agent)
```

Tool definitions (names, descriptions, schemas) are maintained in a single registry (`DispatchToolRegistry`) shared by both the AI dispatch agent and the MCP server. Adding a new tool to the registry automatically makes it available in both systems.

### Project Structure

```text
src/Presentation/Logistics.McpServer/
├── Registrar.cs                              # DI + MCP SDK + auth + rate limit
├── DispatchMcpTool.cs                        # McpServerTool subclass wrapping DispatchToolDefinition
└── Authentication/
    ├── ApiKeyDefaults.cs                     # Scheme constants
    └── ApiKeyAuthenticationHandler.cs        # API key validation + tenant resolution
```
