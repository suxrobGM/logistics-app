using System.Linq.Expressions;
using Logistics.Application.Modules.Compliance.Ifta.TaxRates.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Ifta;

public class UpdateIftaTaxRateHandlerTests
{
    private readonly IMasterUnitOfWork masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly IMasterRepository<IftaTaxRate, Guid> rateRepo =
        Substitute.For<IMasterRepository<IftaTaxRate, Guid>>();

    private readonly UpdateIftaTaxRateHandler sut;
    private readonly Guid rateId = Guid.NewGuid();

    public UpdateIftaTaxRateHandlerTests()
    {
        masterUow.Repository<IftaTaxRate>().Returns(rateRepo);
        sut = new UpdateIftaTaxRateHandler(masterUow, Substitute.For<ILogger<UpdateIftaTaxRateHandler>>());
    }

    private UpdateIftaTaxRateCommand Command() => new()
    {
        Id = rateId,
        CountryCode = "US",
        Region = "TX",
        Year = 2027,
        Quarter = 1,
        RatePerGallon = 0.21m,
    };

    private static IftaTaxRate Rate(string region = "TX") => new()
    {
        Jurisdiction = new TaxJurisdiction { CountryCode = "US", Region = region },
        Year = 2027,
        Quarter = 1,
        RatePerGallon = 0.20m,
    };

    [Fact]
    public async Task Handle_RateNotFound_ReturnsFail()
    {
        rateRepo.GetByIdAsync(rateId, Arg.Any<CancellationToken>()).Returns((IftaTaxRate?)null);

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
        await masterUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoConflict_UpdatesRateAndSaves()
    {
        var rate = Rate();
        rateRepo.GetByIdAsync(rateId, Arg.Any<CancellationToken>()).Returns(rate);
        rateRepo.GetAsync(Arg.Any<Expression<Func<IftaTaxRate, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((IftaTaxRate?)null);

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0.21m, rate.RatePerGallon);
        rateRepo.Received(1).Update(rate);
        await masterUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConflictsWithAnotherRate_ReturnsFail()
    {
        var rate = Rate(region: "OK");
        rateRepo.GetByIdAsync(rateId, Arg.Any<CancellationToken>()).Returns(rate);
        rateRepo.GetAsync(Arg.Any<Expression<Func<IftaTaxRate, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Rate());

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("already exists", result.Error);
        await masterUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
