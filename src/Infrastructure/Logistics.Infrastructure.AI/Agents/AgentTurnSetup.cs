using Logistics.Infrastructure.AI.Llm.Contracts;

namespace Logistics.Infrastructure.AI.Agents;

/// <summary>
/// What an <see cref="IAgentSurface"/> resolves before <see cref="AgentLoopRunner"/> can run: the
/// LLM conversation (system prompt, tool catalogue, replayed transcript) and how tool calls in it
/// are processed.
/// </summary>
internal sealed record AgentTurnSetup(LlmConversation Conversation, ToolCallContext ToolContext);
