namespace Logistics.Infrastructure.AI.Llm.Contracts;

/// <summary>
/// Extended-thinking options for providers/models that support it; ignored by those that don't.
/// </summary>
internal record LlmThinkingOptions(int BudgetTokens);
