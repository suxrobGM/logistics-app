using System.Net;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Models;
using Logistics.Infrastructure.AI.Providers;
using Logistics.Infrastructure.AI.Services;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AIDispatch;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Services;

public class AgentLoopRunnerTests
{
    private readonly ILlmProvider provider = Substitute.For<ILlmProvider>();
    private readonly IAIDispatchToolExecutor toolExecutor = Substitute.For<IAIDispatchToolExecutor>();
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly AgentLoopRunner sut;

    public AgentLoopRunnerTests()
    {
        tenantUow.Repository<AIDispatchDecision>()
            .Returns(Substitute.For<ITenantRepository<AIDispatchDecision, Guid>>());
        tenantUow.GetCurrentTenant().Returns(new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Tenant",
            ConnectionString = "test-connection",
            BillingEmail = "test@test.com",
            CompanyAddress = new() { Line1 = "1 Main", City = "Dallas", State = "TX", ZipCode = "75201", Country = "US" }
        });

        var processor = new AIDispatchDecisionProcessor(
            toolExecutor, new AIDispatchToolRegistry(), tenantUow,
            Substitute.For<IAIDispatchBroadcastService>(),
            NullLogger<AIDispatchDecisionProcessor>.Instance);
        sut = new AgentLoopRunner(processor, NullLogger<AgentLoopRunner>.Instance);
    }

    private static AIDispatchSession Session() => new()
    {
        Mode = AIDispatchMode.Autonomous,
        StartedAt = DateTime.UtcNow,
        ModelUsed = "claude-haiku-4-5"
    };

    private LlmConversation Conversation() => new(
        provider, "system prompt", [LlmMessage.FromUser("go")], [], "claude-haiku-4-5", 1000, null);

    private static LlmResponse TextResponse(string text, int inputTokens = 100, int outputTokens = 50) => new()
    {
        AssistantMessage = new LlmMessage(LlmRole.Assistant, [new LlmTextBlock(text)]),
        TextContent = text,
        StopReason = "end_turn",
        ToolCalls = [],
        Usage = new LlmTokenUsage(inputTokens, outputTokens)
    };

    private static LlmResponse ToolCallResponse(string toolName)
    {
        var toolUse = new LlmToolUseBlock(Guid.NewGuid().ToString(), toolName, new JsonObject());
        return new LlmResponse
        {
            AssistantMessage = new LlmMessage(LlmRole.Assistant, [toolUse]),
            TextContent = null,
            StopReason = "tool_use",
            ToolCalls = [toolUse],
            Usage = new LlmTokenUsage(200, 20)
        };
    }

    [Fact]
    public async Task Run_EndTurnResponse_StopsAfterOneIterationAndSetsCost()
    {
        var session = Session();
        provider.SendAsync(Arg.Any<LlmRequest>(), Arg.Any<CancellationToken>())
            .Returns(TextResponse("All done."));

        await sut.RunAsync(session, Conversation(), new ToolCallContext(session.Mode), null, CancellationToken.None);

        await provider.Received(1).SendAsync(Arg.Any<LlmRequest>(), Arg.Any<CancellationToken>());
        Assert.Equal("All done.", session.Summary);
        Assert.Equal(150, session.TotalTokensUsed);
        Assert.Equal(LlmPricing.GetMultiplier("claude-haiku-4-5"), session.RequestCost);
    }

    [Fact]
    public async Task Run_ToolCallThenEndTurn_ProcessesToolAndAppendsResults()
    {
        var session = Session();
        var conversation = Conversation();
        toolExecutor.ExecuteToolAsync("get_available_trucks", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{}");
        provider.SendAsync(Arg.Any<LlmRequest>(), Arg.Any<CancellationToken>())
            .Returns(ToolCallResponse("get_available_trucks"), TextResponse("Done."));

        var iterations = 0;
        await sut.RunAsync(session, conversation, new ToolCallContext(session.Mode),
            () => { iterations++; return Task.CompletedTask; }, CancellationToken.None);

        await provider.Received(2).SendAsync(Arg.Any<LlmRequest>(), Arg.Any<CancellationToken>());
        Assert.Equal(1, iterations);
        Assert.Equal(1, session.DecisionCount);
        // user, assistant(tool_use), user(tool_result), assistant(text)
        Assert.Equal(4, conversation.Messages.Count);
        Assert.Equal(370, session.TotalTokensUsed);
    }

    [Fact]
    public async Task Run_RateLimited_RetriesThenSucceeds()
    {
        var session = Session();
        var calls = 0;
        provider.SendAsync(Arg.Any<LlmRequest>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                calls++;
                if (calls == 1)
                    throw new HttpRequestException("rate limited", null, HttpStatusCode.TooManyRequests);
                return TextResponse("Recovered.");
            });

        await sut.RunAsync(session, Conversation(), new ToolCallContext(session.Mode), null, CancellationToken.None);

        Assert.Equal(2, calls);
        Assert.Equal("Recovered.", session.Summary);
    }

    [Fact]
    public async Task Run_RateLimitedEveryAttempt_StopsAfterFourCallsAndThrows()
    {
        var calls = 0;
        provider.SendAsync(Arg.Any<LlmRequest>(), Arg.Any<CancellationToken>())
            .Returns<LlmResponse>(_ =>
            {
                calls++;
                throw new HttpRequestException("rate limited", null, HttpStatusCode.TooManyRequests);
            });

        var ex = await Assert.ThrowsAsync<LlmRateLimitedException>(() =>
            sut.RunAsync(Session(), Conversation(), new ToolCallContext(AIDispatchMode.Autonomous),
                null, CancellationToken.None));

        // One initial attempt plus MaxRetries backoff attempts.
        Assert.Equal(4, calls);

        // The tenant must be told about the rate limit, not sent to check the API key.
        var sanitized = AgentLoopRunner.SanitizeErrorMessage(ex);
        Assert.Contains("Rate limited", sanitized);
        Assert.DoesNotContain("API key", sanitized);
    }

    [Fact]
    public void SanitizeErrorMessage_GenuineAuthFailure_StillReportsKeyProblem()
    {
        var sanitized = AgentLoopRunner.SanitizeErrorMessage(
            new HttpRequestException("401 Unauthorized", null, HttpStatusCode.Unauthorized));

        Assert.Equal("API authentication error. Check the LLM API key configuration.", sanitized);
    }

    [Fact]
    public async Task Run_ProviderThrowsMidRun_StillAccumulatesTokensAlreadySpent()
    {
        var session = Session();
        var calls = 0;
        toolExecutor.ExecuteToolAsync("get_available_trucks", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{}");
        provider.SendAsync(Arg.Any<LlmRequest>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                calls++;
                if (calls == 1)
                    return ToolCallResponse("get_available_trucks");
                throw new InvalidOperationException("provider exploded");
            });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.RunAsync(session, Conversation(), new ToolCallContext(session.Mode),
                null, CancellationToken.None));

        Assert.Equal(220, session.TotalTokensUsed);

        // Cost is recorded even though the run failed - the audit trail promises both, and quota
        // that only counts successes would make a failing prompt free to retry.
        Assert.Equal(LlmPricing.GetMultiplier("claude-haiku-4-5"), session.RequestCost);
        Assert.True(session.EstimatedCostUsd > 0);
    }

    [Fact]
    public async Task Run_Cancelled_StillRecordsCost()
    {
        var session = Session();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sut.RunAsync(session, Conversation(), new ToolCallContext(session.Mode), null, cts.Token));

        Assert.Equal(LlmPricing.GetMultiplier("claude-haiku-4-5"), session.RequestCost);
    }

    [Fact]
    public async Task Run_Cancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sut.RunAsync(Session(), Conversation(), new ToolCallContext(AIDispatchMode.Autonomous),
                null, cts.Token));
    }
}
