using Logistics.Infrastructure.AI.Tools.Operations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using Logistics.Mediator;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Operations;

public class GetLoadToolTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly GetLoadTool _sut;

    public GetLoadToolTests()
    {
        _sut = new GetLoadTool(_mediator);
    }

    [Fact]
    public async Task Execute_MissingLoadId_ReturnsError()
    {
        var result = await _sut.ExecuteAsync(new JsonObject(), CancellationToken.None);

        Assert.Contains("load_id", result);
        await _mediator.DidNotReceiveWithAnyArgs().Send<Result<LoadDto>>(default!, default);
    }

    [Fact]
    public async Task Execute_ValidInput_ReturnsDeliveryCostAndInvoiceState()
    {
        var customer = new CustomerDto { Id = Guid.NewGuid(), Name = "Acme", Email = "ap@acme.com" };
        var load = CopilotToolTestData.CreateLoad(deliveryCost: 1800m, customer: customer);
        _mediator.Send(Arg.Any<GetLoadByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<LoadDto>.Ok(load));

        var result = await _sut.ExecuteAsync(
            new JsonObject { ["load_id"] = load.Id.ToString() }, CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        Assert.Equal(1800m, root.GetProperty("delivery_cost").GetDecimal());
        Assert.Equal("ap@acme.com", root.GetProperty("customer_email").GetString());
        Assert.False(root.GetProperty("has_invoice").GetBoolean());
    }
}
