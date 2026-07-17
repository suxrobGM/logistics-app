using System.Linq.Expressions;
using Logistics.Application.Modules.Compliance.Ifta.TaxRates.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Ifta;

public class CreateIftaTaxRateHandlerTests
{
    private readonly IMasterUnitOfWork masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly IMasterRepository<IftaTaxRate, Guid> rateRepo =
        Substitute.For<IMasterRepository<IftaTaxRate, Guid>>();

    private readonly CreateIftaTaxRateHandler sut;

    public CreateIftaTaxRateHandlerTests()
    {
        masterUow.Repository<IftaTaxRate>().Returns(rateRepo);
        sut = new CreateIftaTaxRateHandler(masterUow, Substitute.For<ILogger<CreateIftaTaxRateHandler>>());
    }

    private static CreateIftaTaxRateCommand Command(string country = "us", string? region = "tx") => new()
    {
        CountryCode = country,
        Region = region,
        Year = 2027,
        Quarter = 1,
        RatePerGallon = 0.20m,
    };

    private void SetupExisting(IftaTaxRate? existing) =>
        rateRepo.GetAsync(Arg.Any<Expression<Func<IftaTaxRate, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existing);

    [Fact]
    public async Task Handle_NoDuplicate_AddsRateAndSaves()
    {
        SetupExisting(null);

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await rateRepo.Received(1).AddAsync(
            Arg.Is<IftaTaxRate>(r =>
                r.Jurisdiction.CountryCode == "US" &&
                r.Jurisdiction.Region == "TX" &&
                r.Year == 2027 &&
                r.Quarter == 1 &&
                r.RatePerGallon == 0.20m),
            Arg.Any<CancellationToken>());
        await masterUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateJurisdictionYearQuarter_ReturnsFail()
    {
        SetupExisting(new IftaTaxRate
        {
            Jurisdiction = new TaxJurisdiction { CountryCode = "US", Region = "TX" },
            Year = 2027,
            Quarter = 1,
            RatePerGallon = 0.19m,
        });

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("already exists", result.Error);
        await rateRepo.DidNotReceive().AddAsync(Arg.Any<IftaTaxRate>(), Arg.Any<CancellationToken>());
        await masterUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BlankRegion_NormalizedToCountryWideNull()
    {
        SetupExisting(null);

        var result = await sut.Handle(Command(region: "  "), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await rateRepo.Received(1).AddAsync(
            Arg.Is<IftaTaxRate>(r => r.Jurisdiction.Region == null),
            Arg.Any<CancellationToken>());
    }
}
