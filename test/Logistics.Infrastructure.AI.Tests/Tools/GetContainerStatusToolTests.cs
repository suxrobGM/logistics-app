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

public class GetContainerStatusToolTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Container, Guid> containerRepo =
        Substitute.For<ITenantRepository<Container, Guid>>();
    private readonly ITenantRepository<Load, Guid> loadRepo =
        Substitute.For<ITenantRepository<Load, Guid>>();
    private readonly GetContainerStatusTool sut;

    public GetContainerStatusToolTests()
    {
        tenantUow.Repository<Container>().Returns(containerRepo);
        tenantUow.Repository<Load>().Returns(loadRepo);
        sut = new GetContainerStatusTool(tenantUow);
    }

    [Fact]
    public void Name_IsSnakeCase()
    {
        Assert.Equal("get_container_status", sut.Name);
    }

    [Fact]
    public async Task Execute_NoIdentifier_ReturnsErrorNamingBothParams()
    {
        var result = await sut.ExecuteAsync(new JsonObject(), CancellationToken.None);

        var error = JsonDocument.Parse(result).RootElement.GetProperty("error").GetString();
        Assert.Contains("container_number", error);
        Assert.Contains("container_id", error);
    }

    [Fact]
    public async Task Execute_UnknownNumber_ReturnsError()
    {
        containerRepo
            .GetAsync(Arg.Any<Expression<Func<Container, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Container?)null);

        var input = new JsonObject { ["container_number"] = "MSCU9999999" };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);

        var error = JsonDocument.Parse(result).RootElement.GetProperty("error").GetString();
        Assert.Contains("MSCU9999999", error);
    }

    [Fact]
    public async Task Execute_ByNumber_ShapesResponseWithTerminalAndLinkedLoad()
    {
        var terminal = new Terminal
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

        var container = new Container
        {
            Number = "MSCU1234567",
            IsoType = ContainerIsoType.Hc40,
            SealNumber = "SL-0099",
            BillOfLadingNumber = "BL-777",
            BookingReference = "BK-42",
            IsLaden = true,
            GrossWeight = 18500m,
            CurrentTerminal = terminal
        };
        container.MarkAtPort(terminal);

        containerRepo
            .GetAsync(Arg.Any<Expression<Func<Container, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(container);
        loadRepo
            .GetAsync(Arg.Any<Expression<Func<Load, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(CreateLoad("LA to Phoenix"));

        var input = new JsonObject { ["container_number"] = "mscu1234567" };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);
        var root = JsonDocument.Parse(result).RootElement;

        Assert.Equal("MSCU1234567", root.GetProperty("number").GetString());
        Assert.Equal("Hc40", root.GetProperty("iso_type").GetString());
        Assert.Equal("40' High Cube", root.GetProperty("iso_type_description").GetString());
        Assert.Equal("AtPort", root.GetProperty("status").GetString());
        Assert.True(root.GetProperty("is_laden").GetBoolean());
        Assert.Equal("SL-0099", root.GetProperty("seal_number").GetString());
        Assert.Equal("BL-777", root.GetProperty("bill_of_lading_number").GetString());

        var terminalJson = root.GetProperty("current_terminal");
        Assert.Equal("USLAX", terminalJson.GetProperty("code").GetString());
        Assert.Equal("SeaPort", terminalJson.GetProperty("type").GetString());
        // Record default ToString would leak "Address { Line1 = ... }" - assert the flat form.
        Assert.Contains("San Pedro", terminalJson.GetProperty("address").GetString());
        Assert.DoesNotContain("Line1", terminalJson.GetProperty("address").GetString());

        Assert.Equal("LA to Phoenix", root.GetProperty("linked_load").GetProperty("name").GetString());
    }

    [Fact]
    public async Task Execute_ByIdWithNoTerminalOrLoad_EmitsNulls()
    {
        var containerId = Guid.NewGuid();
        containerRepo.GetByIdAsync(containerId, Arg.Any<CancellationToken>())
            .Returns(new Container { Number = "TCLU7654321", IsoType = ContainerIsoType.Gp20 });
        loadRepo
            .GetAsync(Arg.Any<Expression<Func<Load, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Load?)null);

        var input = new JsonObject { ["container_id"] = containerId.ToString() };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);
        var root = JsonDocument.Parse(result).RootElement;

        Assert.Equal("TCLU7654321", root.GetProperty("number").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("current_terminal").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("linked_load").ValueKind);
    }

    private static Load CreateLoad(string name)
    {
        return new Load
        {
            Name = name,
            Type = LoadType.IntermodalContainer,
            Customer = null!,
            OriginAddress = new Address
            {
                Line1 = "1 Origin", City = "City", State = "ST", ZipCode = "00000", Country = "US"
            },
            OriginLocation = new GeoPoint(0, 0),
            DestinationAddress = new Address
            {
                Line1 = "1 Dest", City = "City", State = "ST", ZipCode = "00000", Country = "US"
            },
            DestinationLocation = new GeoPoint(0, 0),
            DeliveryCost = Money.Zero("USD")
        };
    }
}
