using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Reporting.Reports.Queries;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.AI.Tools.Financial;
using Logistics.Mediator;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Financial;

public class GetProfitabilityToolTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly GetProfitabilityTool _sut;

    public GetProfitabilityToolTests()
    {
        _tenantUow.GetCurrentTenant().Returns(TestTenant.Create());
        _sut = new GetProfitabilityTool(_mediator, _tenantUow);
    }

    private async Task<JsonElement> Run(JsonObject input) =>
        JsonDocument.Parse(await _sut.ExecuteAsync(input, CancellationToken.None)).RootElement;

    [Fact]
    public async Task Execute_MissingGroupBy_ReturnsError()
    {
        var root = await Run([]);

        Assert.True(root.TryGetProperty("error", out _));
    }

    [Fact]
    public async Task Execute_ByLane_ReturnsLanesWithTotals()
    {
        _mediator.Send(Arg.Any<ProfitabilityReportQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProfitabilityReportDto>.Ok(new ProfitabilityReportDto
            {
                Summary = new ProfitabilitySummaryDto { LoadCount = 2, NetProfit = -150m },
                Lanes =
                [
                    new LaneProfitDto { Lane = "TX → CA", LoadCount = 2, Margin = -300m, MarginPerDistance = -0.5m, WorstCustomer = "Acme" }
                ]
            }));

        var root = await Run(new JsonObject { ["group_by"] = "lane" });

        Assert.Equal(-150m, root.GetProperty("totals").GetProperty("net_profit").GetDecimal());
        var lane = root.GetProperty("rows")[0];
        Assert.Equal("TX → CA", lane.GetProperty("lane").GetString());
        Assert.Equal(-0.5m, lane.GetProperty("margin_per_mi").GetDecimal());
        Assert.Equal("Acme", lane.GetProperty("worst_customer").GetString());
    }
}
