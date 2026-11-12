using System.Text.Json.Nodes;
using Logistics.Application.Modules.Operations.Loads.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Tools.Operations;
using Logistics.Mediator;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Operations;

public class SendCustomerUpdateToolTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly SendCustomerUpdateTool _sut;

    public SendCustomerUpdateToolTests()
    {
        _sut = new SendCustomerUpdateTool(_mediator);
    }

    [Fact]
    public async Task Execute_MissingMessage_ReturnsError()
    {
        var result = await _sut.ExecuteAsync(
            new JsonObject { ["load_id"] = Guid.NewGuid().ToString() }, CancellationToken.None);

        Assert.Contains("error", result);
        await _mediator.DidNotReceiveWithAnyArgs().Send(default(SendCustomerDelayNoticeCommand)!, default);
    }

    [Fact]
    public async Task Execute_SendsTheDelayNotice()
    {
        var loadId = Guid.NewGuid();
        _mediator.Send(Arg.Any<SendCustomerDelayNoticeCommand>(), Arg.Any<CancellationToken>()).Returns(Result.Ok());

        var result = await _sut.ExecuteAsync(new JsonObject
        {
            ["load_id"] = loadId.ToString(),
            ["message"] = "A road closure near Tucson moves delivery to about 4 PM."
        }, CancellationToken.None);

        Assert.Contains("\"success\":true", result);
        await _mediator.Received(1).Send(
            Arg.Is<SendCustomerDelayNoticeCommand>(c => c.LoadId == loadId && c.Message.Contains("Tucson")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Definition_IsNeverAutoApproveEligible()
    {
        Assert.Equal(AgentDecisionType.SendCustomerUpdate, SendCustomerUpdateTool.Definition.DecisionType);
        Assert.DoesNotContain(AgentDecisionType.SendCustomerUpdate, AIDispatchPolicy.AutoApproveEligibleTypes);
    }
}
