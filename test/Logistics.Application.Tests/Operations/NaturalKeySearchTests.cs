using Logistics.Application.Modules.Operations.Containers.Queries;
using Logistics.Application.Modules.Operations.Terminals.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations;

/// <summary>
/// Container.Number and Terminal.Code are stored canonical, so a raw lowercase search term used to
/// match nothing. The list handlers normalise the term through the same helper the setter uses.
/// </summary>
public class NaturalKeySearchTests
{
    private readonly ITenantRepository<Container, Guid> containerRepo =
        Substitute.For<ITenantRepository<Container, Guid>>();

    private readonly ITenantRepository<Terminal, Guid> terminalRepo =
        Substitute.For<ITenantRepository<Terminal, Guid>>();

    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();

    public NaturalKeySearchTests()
    {
        tenantUow.Repository<Container>().Returns(containerRepo);
        tenantUow.Repository<Terminal>().Returns(terminalRepo);

        containerRepo.Query().Returns(new List<Container>
        {
            new() { Number = "MSCU1234567", IsoType = ContainerIsoType.Gp20 },
            new() { Number = "TCLU7654321", IsoType = ContainerIsoType.Gp40 }
        }.BuildMock());

        terminalRepo.Query().Returns(new List<Terminal> { NewTerminal("USLAX"), NewTerminal("DEHAM") }.BuildMock());
    }

    private static Terminal NewTerminal(string code) => new()
    {
        Name = $"Terminal {code}",
        Code = code,
        CountryCode = "US",
        Type = TerminalType.SeaPort,
        Address = new Address
        {
            Line1 = "1 Test St", City = "Test", State = "TX", ZipCode = "00000", Country = "US"
        }
    };

    [Theory]
    [InlineData("mscu1234567")]
    [InlineData("MSCU1234567")]
    [InlineData("  mscu1234567  ")]
    [InlineData("mscu")]
    public async Task GetContainers_SearchIsCaseAndWhitespaceInsensitive(string search)
    {
        var sut = new GetContainersHandler(tenantUow);

        var result = await sut.Handle(new GetContainersQuery { Search = search }, CancellationToken.None);

        Assert.Equal("MSCU1234567", Assert.Single(result.Value!).Number);
    }

    [Theory]
    [InlineData("uslax")]
    [InlineData("USLAX")]
    [InlineData(" uslax ")]
    public async Task GetTerminals_SearchIsCaseAndWhitespaceInsensitive(string search)
    {
        var sut = new GetTerminalsHandler(tenantUow);

        var result = await sut.Handle(new GetTerminalsQuery { Search = search }, CancellationToken.None);

        Assert.Equal("USLAX", Assert.Single(result.Value!).Code);
    }

    /// <summary>Name stays a raw match, so it must not be uppercased along with the code.</summary>
    [Fact]
    public async Task GetTerminals_StillMatchesOnFreeFormName()
    {
        var sut = new GetTerminalsHandler(tenantUow);

        var result = await sut.Handle(new GetTerminalsQuery { Search = "Terminal DEHAM" }, CancellationToken.None);

        Assert.Equal("DEHAM", Assert.Single(result.Value!).Code);
    }
}
