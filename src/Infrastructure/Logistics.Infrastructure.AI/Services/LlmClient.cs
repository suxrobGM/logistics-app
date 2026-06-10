using Logistics.Application.Abstractions.Ai;
using Logistics.Infrastructure.AI.Models;
using Logistics.Infrastructure.AI.Options;
using Logistics.Infrastructure.AI.Providers;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// One-shot LLM entry point. Resolves the global model, sends a single tool-less request
/// (optionally with inline documents), and reports token usage and estimated cost.
/// </summary>
internal sealed class LlmClient(
    LlmModelResolver modelResolver,
    LlmProviderFactory providerFactory,
    IOptions<LlmOptions> options,
    ILogger<LlmClient> logger) : ILlmClient
{
    public async Task<Result<LlmCompletionResult>> CompleteAsync(
        LlmCompletionRequest request,
        CancellationToken ct = default)
    {
        var config = options.Value;
        var selection = await modelResolver.ResolveAsync(config, ct);

        if (string.IsNullOrWhiteSpace(selection.ProviderConfig.ApiKey))
            return Result<LlmCompletionResult>.Fail($"LLM API key for provider '{selection.Provider}' is not configured.");

        var content = new List<LlmContentBlock> { new LlmTextBlock(request.UserText) };
        foreach (var document in request.Documents)
        {
            content.Add(new LlmDocumentBlock(document.MediaType, Convert.ToBase64String(document.Data)));
        }

        var llmRequest = new LlmRequest
        {
            SystemPrompt = request.SystemPrompt,
            Messages = [new LlmMessage(LlmRole.User, content)],
            Tools = [],
            Model = selection.Model,
            MaxTokens = request.MaxTokens,
            Temperature = 0m
        };

        try
        {
            var provider = providerFactory.Create(selection.Provider);
            var response = await provider.SendAsync(llmRequest, ct);

            var usage = response.Usage;
            var cost = LlmPricing.Calculate(
                selection.Model, usage.InputTokens, usage.OutputTokens, usage.CacheReadTokens, usage.CacheCreationTokens);

            logger.LogInformation(
                "LLM completion: model {Model}, input {InputTokens} tok, output {OutputTokens} tok, est ${Cost:F4}",
                selection.Model, usage.InputTokens, usage.OutputTokens, cost);

            return Result<LlmCompletionResult>.Ok(new LlmCompletionResult(
                response.TextContent ?? string.Empty,
                selection.Model,
                usage.InputTokens,
                usage.OutputTokens,
                cost));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "LLM completion failed for model {Model}", selection.Model);
            return Result<LlmCompletionResult>.Fail($"LLM request failed: {ex.Message}");
        }
    }
}
