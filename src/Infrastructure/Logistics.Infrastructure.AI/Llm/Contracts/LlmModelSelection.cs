using Logistics.Application.Abstractions.AI;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Models;

/// <summary>The resolved model, its provider, and that provider's configuration.</summary>
internal sealed record LlmModelSelection(string Model, LlmProvider Provider, LlmProviderOptions ProviderConfig);
