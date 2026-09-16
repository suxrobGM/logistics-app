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
    private readonly IMasterUnitOfWork _masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly IMasterRepository<IftaTaxRate, Guid> _rateRepo =
        Substitute.For<IMasterRepository<IftaTaxRate, Guid>>();

    private readonly UpdateIftaTaxRateHandler _sut;
    private readonly Guid _rateId = Guid.NewGuid();

    public UpdateIftaTaxRateHandlerTests()
    {
        _masterUow.Repository<IftaTaxRate>().Returns(_rateRepo);
        _sut = new UpdateIftaTaxRateHandler(_masterUow, Substitute.For<ILogger<UpdateIftaTaxRateHandler>>());
    }

    private UpdateIftaTaxRateCommand Command() => new()
    {
        Id = _rateId,
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
        _rateRepo.GetByIdAsync(_rateId, Arg.Any<CancellationToken>()).Returns((IftaTaxRate?)null);

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
        await _masterUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoConflict_UpdatesRateAndSaves()
    {
        var rate = Rate();
        _rateRepo.GetByIdAsync(_rateId, Arg.Any<CancellationToken>()).Returns(rate);
        _rateRepo.GetAsync(Arg.Any<Expression<Func<IftaTaxRate, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((IftaTaxRate?)null);

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0.21m, rate.RatePerGallon);
        _rateRepo.Received(1).Update(rate);
        await _masterUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConflictsWithAnotherRate_ReturnsFail()
    {
        var rate = Rate(region: "OK");
        _rateRepo.GetByIdAsync(_rateId, Arg.Any<CancellationToken>()).Returns(rate);
        _rateRepo.GetAsync(Arg.Any<Expression<Func<IftaTaxRate, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Rate());

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("already exists", result.Error);
        await _masterUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
