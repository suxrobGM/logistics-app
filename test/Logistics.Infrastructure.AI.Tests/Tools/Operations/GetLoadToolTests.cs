using Logistics.Infrastructure.AI.Tools.Operations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using Logistics.Mediator;
using Logistics.Domain.Persistence;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Operations;

public class GetLoadToolTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly GetLoadTool _sut;

    public GetLoadToolTests()
    {
        var tenantUow = Substitute.For<ITenantUnitOfWork>();
        tenantUow.GetCurrentTenant().Returns(TestTenants.Create());
        _sut = new GetLoadTool(_mediator, tenantUow);
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
        Assert.Equal(1800m, root.GetProperty("linehaul").GetDecimal());
        Assert.Equal("ap@acme.com", root.GetProperty("customer_email").GetString());
        Assert.False(root.GetProperty("has_invoice").GetBoolean());
        Assert.True(root.TryGetProperty("distance_mi", out _));
    }

    /// <summary>The linehaul is only part of the price; the agent quotes total_charges.</summary>
    [Fact]
    public async Task Execute_LoadWithCharges_TotalsLinehaulFuelAndAccessorials()
    {
        var load = CopilotToolTestData.CreateLoad(deliveryCost: 1800m);
        load.FuelSurcharge = 220m;
        load.Accessorials =
        [
            new LoadAccessorialDto { Name = "Detention", Rate = 75m, Quantity = 2, Total = 150m },
            new LoadAccessorialDto { Name = "Lumper", Rate = 80m, Total = 80m }
        ];
        load.PalletCount = 6;
        load.FreightClass = Logistics.Domain.Primitives.Enums.FreightClass.Class70;
        _mediator.Send(Arg.Any<GetLoadByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<LoadDto>.Ok(load));

        var result = await _sut.ExecuteAsync(
            new JsonObject { ["load_id"] = load.Id.ToString() }, CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        Assert.Equal(220m, root.GetProperty("fuel_surcharge").GetDecimal());
        Assert.Equal(2, root.GetProperty("accessorials").GetArrayLength());
        Assert.Equal(2250m, root.GetProperty("total_charges").GetDecimal());
        Assert.StartsWith("class 70, 6 pallets", root.GetProperty("freight").GetString());
    }
}
