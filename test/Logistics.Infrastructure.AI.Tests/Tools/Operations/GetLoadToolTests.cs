using Logistics.Infrastructure.AI.Tools.Operations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using MediatR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Operations;

public class GetLoadToolTests
{
    private readonly IMediator mediator = Substitute.For<IMediator>();
    private readonly GetLoadTool sut;

    public GetLoadToolTests()
    {
        sut = new GetLoadTool(mediator);
    }

    [Fact]
    public async Task Execute_MissingLoadId_ReturnsError()
    {
        var result = await sut.ExecuteAsync(new JsonObject(), CancellationToken.None);

        Assert.Contains("load_id", result);
        await mediator.DidNotReceiveWithAnyArgs().Send<Result<LoadDto>>(default!, default);
    }

    [Fact]
    public async Task Execute_ValidInput_ReturnsDeliveryCostAndInvoiceState()
    {
        var customer = new CustomerDto { Id = Guid.NewGuid(), Name = "Acme", Email = "ap@acme.com" };
        var load = CopilotToolTestData.CreateLoad(deliveryCost: 1800m, customer: customer);
        mediator.Send(Arg.Any<GetLoadByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<LoadDto>.Ok(load));

        var result = await sut.ExecuteAsync(
            new JsonObject { ["load_id"] = load.Id.ToString() }, CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        Assert.Equal(1800m, root.GetProperty("delivery_cost").GetDecimal());
        Assert.Equal("ap@acme.com", root.GetProperty("customer_email").GetString());
        Assert.False(root.GetProperty("has_invoice").GetBoolean());
    }
}
