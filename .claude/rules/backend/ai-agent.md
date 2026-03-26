# AI Dispatch Agent Conventions

## Project Structure

All AI agent code lives in `src/Infrastructure/Logistics.Infrastructure.AI/`:
- `Services/` — Agent loop, tool executor, tool registry
- `Tools/` — Individual tool implementations (future)
- `Prompts/` — System prompt builders
- `Options/` — Configuration (ClaudeOptions)

## Adding a New Agent Tool

1. Add the tool definition in `DispatchToolRegistry.cs` with name, description, and JSON schema
2. Add the handler case in `DispatchToolExecutor.ExecuteToolAsync()` switch
3. If it's a write tool, add its name to `ClaudeDispatchAgentService.WriteTools` HashSet
4. Tool names use `snake_case` (Claude API convention)
5. Tool schemas follow JSON Schema format (compatible with MCP)

## Tool Classification

- **Read tools**: Always execute immediately in both modes. Used for querying fleet state.
- **Write tools**: In HumanInTheLoop mode → create `Suggested` decisions. In Autonomous mode → execute immediately.

## Agent Loop Pattern

The agent loop in `ClaudeDispatchAgentService` follows: send message → receive tool_use → record decision → execute or suggest → send tool_result → repeat until end_turn.

Max 25 iterations per session to prevent runaway token usage.

## Anthropic SDK Usage

- Client: `new AnthropicClient(apiKey)` — do NOT use DI for the client itself
- System prompt: `System = [new SystemMessage(text)]` on `MessageParameters`
- Tools: `new Function(name, description, jsonSchemaNode)` cast to `Tool`
- Tool results: `ToolResultContent` with `ToolUseId` matching the request
- Disambiguation: `using Tool = Anthropic.SDK.Common.Tool` (conflicts with Messaging namespace)

## Configuration

Claude config is in `appsettings.json` under `"Claude"` section. API key is passed via environment variable `Claude__ApiKey` in all deployment targets (Aspire, Docker, .env).
