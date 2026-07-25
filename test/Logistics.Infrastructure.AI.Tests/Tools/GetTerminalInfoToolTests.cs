using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.AI.Tools;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

public class GetTerminalInfoToolTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Terminal, Guid> terminalRepo =
        Substitute.For<ITenantRepository<Terminal, Guid>>();
    private readonly GetTerminalInfoTool sut;

    public GetTerminalInfoToolTests()
    {
        tenantUow.Repository<Terminal>().Returns(terminalRepo);
        sut = new GetTerminalInfoTool(tenantUow);
    }

    [Fact]
    public void Name_IsSnakeCase()
    {
        Assert.Equal("get_terminal_info", sut.Name);
    }

    [Fact]
    public async Task Execute_NoIdentifier_ReturnsErrorNamingBothParams()
    {
        var result = await sut.ExecuteAsync(new JsonObject(), CancellationToken.None);

        var error = JsonDocument.Parse(result).RootElement.GetProperty("error").GetString();
        Assert.Contains("code", error);
        Assert.Contains("terminal_id", error);
    }

    [Fact]
    public async Task Execute_UnknownCode_ReturnsError()
    {
        terminalRepo
            .GetAsync(Arg.Any<Expression<Func<Terminal, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Terminal?)null);

        var input = new JsonObject { ["code"] = "ZZZZZ" };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);

        var error = JsonDocument.Parse(result).RootElement.GetProperty("error").GetString();
        Assert.Contains("ZZZZZ", error);
    }

    [Fact]
    public async Task Execute_ByCode_ShapesResponse()
    {
        terminalRepo
            .GetAsync(Arg.Any<Expression<Func<Terminal, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(CreateTerminal());

        // Lower case in, canonical UN/LOCODE out.
        var input = new JsonObject { ["code"] = "uslax" };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);
        var root = JsonDocument.Parse(result).RootElement;

        Assert.Equal("USLAX", root.GetProperty("code").GetString());
        Assert.Equal("Los Angeles", root.GetProperty("name").GetString());
        Assert.Equal("SeaPort", root.GetProperty("type").GetString());
        Assert.Equal("Sea Port", root.GetProperty("type_description").GetString());
        Assert.Equal("US", root.GetProperty("country_code").GetString());
        Assert.Equal("San Pedro", root.GetProperty("city").GetString());
    }

    [Fact]
    public async Task Execute_FormatsAddressFlat_NotAsRecordSyntax()
    {
        terminalRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(CreateTerminal());

        var input = new JsonObject { ["terminal_id"] = Guid.NewGuid().ToString() };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);
        var address = JsonDocument.Parse(result).RootElement.GetProperty("address").GetString();

        Assert.Equal("425 S Palos Verdes St, San Pedro, CA, 90731, US", address);
    }

    /// <summary>No coordinates exist on Terminal - the tool must not imply otherwise.</summary>
    [Fact]
    public async Task Execute_DoesNotEmitCoordinates()
    {
        terminalRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(CreateTerminal());

        var input = new JsonObject { ["terminal_id"] = Guid.NewGuid().ToString() };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);
        var root = JsonDocument.Parse(result).RootElement;

        Assert.False(root.TryGetProperty("latitude", out _));
        Assert.False(root.TryGetProperty("longitude", out _));
    }

    private static Terminal CreateTerminal()
    {
        return new Terminal
        {
            Name = "Los Angeles",
            Code = "USLAX",
            CountryCode = "US",
            Type = TerminalType.SeaPort,
            Address = new Address
            {
                Line1 = "425 S Palos Verdes St",
                City = "San Pedro",
                State = "CA",
                ZipCode = "90731",
                Country = "US"
            }
        };
    }
}
