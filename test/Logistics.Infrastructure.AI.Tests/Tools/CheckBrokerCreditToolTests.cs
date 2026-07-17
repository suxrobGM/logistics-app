using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

public class CheckBrokerCreditToolTests
{
    private readonly IBrokerCreditService brokerCreditService = Substitute.For<IBrokerCreditService>();
    private readonly CheckBrokerCreditTool sut;

    public CheckBrokerCreditToolTests()
    {
        sut = new CheckBrokerCreditTool(brokerCreditService);
    }

    [Fact]
    public void Name_IsSnakeCase()
    {
        Assert.Equal("check_broker_credit", sut.Name);
    }

    [Fact]
    public async Task Execute_MissingMcNumber_ReturnsError()
    {
        var input = new JsonObject();

        var result = await sut.ExecuteAsync(input, CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        Assert.Contains("mc_number", root.GetProperty("error").GetString());
        await brokerCreditService.DidNotReceiveWithAnyArgs().GetBrokerCreditAsync(default);
    }

    [Fact]
    public async Task Execute_NoDataAvailable_ReturnsWarningNotError()
    {
        brokerCreditService.GetBrokerCreditAsync("MC999999", Arg.Any<CancellationToken>())
            .Returns((BrokerCreditDto?)null);

        var input = new JsonObject { ["mc_number"] = "MC999999" };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);
        var root = JsonDocument.Parse(result).RootElement;

        Assert.False(root.TryGetProperty("error", out _));
        Assert.Contains("No credit data", root.GetProperty("warning").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("credit_score").ValueKind);
    }

    [Fact]
    public async Task Execute_HappyPath_ShapesResponse()
    {
        var checkedAt = DateTime.UtcNow;
        brokerCreditService.GetBrokerCreditAsync("MC123456", Arg.Any<CancellationToken>())
            .Returns(new BrokerCreditDto
            {
                McNumber = "123456",
                CreditScore = 87,
                DaysToPay = 28,
                AuthorityActive = true,
                Source = BrokerCreditSource.Dat,
                CheckedAt = checkedAt
            });

        var input = new JsonObject { ["mc_number"] = "MC123456" };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);
        var root = JsonDocument.Parse(result).RootElement;

        Assert.Equal("123456", root.GetProperty("mc_number").GetString());
        Assert.Equal(87, root.GetProperty("credit_score").GetInt32());
        Assert.Equal(28, root.GetProperty("days_to_pay").GetInt32());
        Assert.True(root.GetProperty("authority_active").GetBoolean());
        Assert.Equal("Dat", root.GetProperty("source").GetString());
    }

    [Fact]
    public async Task Execute_InactiveAuthority_SurfacesFlag()
    {
        brokerCreditService.GetBrokerCreditAsync("MC555555", Arg.Any<CancellationToken>())
            .Returns(new BrokerCreditDto
            {
                McNumber = "555555",
                AuthorityActive = false,
                Source = BrokerCreditSource.Fmcsa,
                CheckedAt = DateTime.UtcNow
            });

        var input = new JsonObject { ["mc_number"] = "MC555555" };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);
        var root = JsonDocument.Parse(result).RootElement;

        Assert.False(root.GetProperty("authority_active").GetBoolean());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("credit_score").ValueKind);
    }
}
