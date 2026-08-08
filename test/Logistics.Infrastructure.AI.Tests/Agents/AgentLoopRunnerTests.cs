using Logistics.Application.Abstractions.Agents;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Infrastructure.AI.Agents;
using System.Net;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Infrastructure.AI.Llm;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AIDispatch;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Agents;

public class AgentLoopRunnerTests
{
    private readonly ILlmProvider provider = Substitute.For<ILlmProvider>();
    private readonly IAgentToolExecutor toolExecutor = Substitute.For<IAgentToolExecutor>();
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<AgentSession, Guid> sessionRepo =
        Substitute.For<ITenantRepository<AgentSession, Guid>>();
    private readonly AgentLoopRunner sut;

    /// <summary>
    /// The loop re-reads the session's status from the database each iteration to honour a cancel
    /// issued on another instance. Point that query at <paramref name="rows"/>.
    /// </summary>
    private void SessionInDatabase(params AgentSession[] rows) =>
        sessionRepo.Query().Returns(rows.ToList().BuildMock());

    public AgentLoopRunnerTests()
    {
        tenantUow.Repository<AgentDecision>()
            .Returns(Substitute.For<ITenantRepository<AgentDecision, Guid>>());
        tenantUow.Repository<AgentSession>().Returns(sessionRepo);
        SessionInDatabase();
        tenantUow.GetCurrentTenant().Returns(new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Tenant",
            ConnectionString = "test-connection",
            BillingEmail = "test@test.com",
            CompanyAddress = new() { Line1 = "1 Main", City = "Dallas", State = "TX", ZipCode = "75201", Country = "US" }
        });

        var processor = new AgentDecisionProcessor(
            toolExecutor, new AgentToolRegistry(), tenantUow,
            Substitute.For<IAIDispatchBroadcastService>(),
            NullLogger<AgentDecisionProcessor>.Instance);
        sut = new AgentLoopRunner(processor, tenantUow, NullLogger<AgentLoopRunner>.Instance);
    }

    private static AgentSession Session() => new()
    {
        StartedAt = DateTime.UtcNow,
        ModelUsed = "claude-haiku-4-5"
    };

    private LlmConversation Conversation() => new(
        provider, "system prompt", [LlmMessage.FromUser("go")], [], "claude-haiku-4-5", 1000,
        ReasoningEffort.None);

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

        await sut.RunAsync(session, Conversation(), new ToolCallContext(), null, CancellationToken.None);

        await provider.Received(1).SendAsync(Arg.Any<LlmRequest>(), Arg.Any<CancellationToken>());
        Assert.Equal("All done.", session.Summary);
        Assert.Equal(150, session.TotalTokensUsed);
        Assert.Equal(LlmPricing.Calculate("claude-haiku-4-5", 100, 50), session.EstimatedCostUsd);
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
        await sut.RunAsync(session, conversation, new ToolCallContext(),
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

        await sut.RunAsync(session, Conversation(), new ToolCallContext(), null, CancellationToken.None);

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
            sut.RunAsync(Session(), Conversation(), new ToolCallContext(),
                null, CancellationToken.None));

        // One initial attempt plus MaxRetries backoff attempts.
        Assert.Equal(4, calls);

        // The tenant must be told about the rate limit, not sent to check the API key.
        var sanitized = LlmErrorSanitizer.ForSession(ex);
        Assert.Contains("Rate limited", sanitized);
        Assert.DoesNotContain("API key", sanitized);
    }

    [Fact]
    public void ForSession_GenuineAuthFailure_StillReportsKeyProblem()
    {
        var sanitized = LlmErrorSanitizer.ForSession(
            new HttpRequestException("401 Unauthorized", null, HttpStatusCode.Unauthorized));

        Assert.Equal("API authentication error. Check the LLM API key configuration.", sanitized);
    }

    [Fact]
    public void ForToolCall_AuthShapedFailure_RedactsWithoutBlamingTheLlmKey()
    {
        // A tool talks to the database and to vendor APIs, so an auth failure here is not the LLM
        // credentials - saying so would send the operator to the wrong system entirely.
        var sanitized = LlmErrorSanitizer.ForToolCall(
            new InvalidOperationException("unauthorized: Host=db-prod-01;Password=hunter2"));

        Assert.DoesNotContain("hunter2", sanitized);
        Assert.DoesNotContain("db-prod-01", sanitized);
        Assert.DoesNotContain("API key", sanitized);
    }

    [Fact]
    public void ForToolCall_OrdinaryFailure_KeepsTheMessage()
    {
        var sanitized = LlmErrorSanitizer.ForToolCall(
            new InvalidOperationException("Trip is not in Draft status"));

        Assert.Equal("Trip is not in Draft status", sanitized);
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
            sut.RunAsync(session, Conversation(), new ToolCallContext(),
                null, CancellationToken.None));

        Assert.Equal(220, session.TotalTokensUsed);

        // Cost is recorded even though the run failed - the audit trail promises both, and a
        // budget that only counted successes would make a failing prompt free to retry.
        Assert.True(session.EstimatedCostUsd > 0);
    }

    [Fact]
    public async Task Run_SessionCancelledOnAnotherInstance_StopsWithoutCallingTheProvider()
    {
        var session = Session();
        // What a cancel from a different API instance looks like to this worker: its own token is
        // untouched (the registry is process-local) but the row already says Cancelled.
        var rowAsPersisted = Session();
        rowAsPersisted.Id = session.Id;
        rowAsPersisted.Cancel();
        SessionInDatabase(rowAsPersisted);

        provider.SendAsync(Arg.Any<LlmRequest>(), Arg.Any<CancellationToken>())
            .Returns(TextResponse("should never run"));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sut.RunAsync(session, Conversation(), new ToolCallContext(),
                null, CancellationToken.None));

        await provider.DidNotReceive().SendAsync(Arg.Any<LlmRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Run_SessionStillRunningInDatabase_Proceeds()
    {
        var session = Session();
        var rowAsPersisted = Session();
        rowAsPersisted.Id = session.Id;
        SessionInDatabase(rowAsPersisted);

        provider.SendAsync(Arg.Any<LlmRequest>(), Arg.Any<CancellationToken>())
            .Returns(TextResponse("All done."));

        await sut.RunAsync(session, Conversation(), new ToolCallContext(),
            null, CancellationToken.None);

        await provider.Received(1).SendAsync(Arg.Any<LlmRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Run_Cancelled_StillRecordsCost()
    {
        var session = Session();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sut.RunAsync(session, Conversation(), new ToolCallContext(), null, cts.Token));

        // The finally block ran: a cancel before any provider call burned nothing, so $0 exactly.
        Assert.Equal(0m, session.EstimatedCostUsd);
    }

    [Fact]
    public async Task Run_Cancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sut.RunAsync(Session(), Conversation(), new ToolCallContext(),
                null, cts.Token));
    }
}
