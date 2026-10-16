using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Abstractions.Geocoding;
using Logistics.Application.Modules.Operations.Common.Services;
using Logistics.Application.Modules.Operations.Trailers.Services;
using Logistics.Application.Modules.Platform.DataImport.Importers;
using Logistics.Application.Modules.Platform.DataImport.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using MockQueryable;
using NSubstitute;
using Xunit;
using Keys = Logistics.Application.Modules.Platform.DataImport.Constants.ImportFieldKeys;

namespace Logistics.Application.Tests.Platform.DataImport;

public class ImportImporterTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Trailer, Guid> _trailerRepo = Substitute.For<ITenantRepository<Trailer, Guid>>();
    private readonly List<Trailer> _trailers = [];
    private readonly TrailerImporter _sut;

    public ImportImporterTests()
    {
        _tenantUow.Repository<Trailer>().Returns(_trailerRepo);
        _trailerRepo.Query().Returns(_ => _trailers.BuildMock());
        _sut = new TrailerImporter(_tenantUow, Substitute.For<ITrailerHookService>());
    }

    #region Create, skip, update

    [Fact]
    public async Task ImportAsync_NewTrailer_CreatesIt()
    {
        var rows = new[] { Row(2, (Keys.Number, "T-1"), (Keys.Type, "Reefer"), (Keys.Length, "53")) };

        var results = await _sut.ImportAsync(rows, Options(commit: true), CancellationToken.None);

        Assert.Equal(ImportRowAction.Create, Assert.Single(results).Action);
        await _trailerRepo.Received(1).AddAsync(
            Arg.Is<Trailer>(t => t.Number == "T-1" && t.Type == TrailerType.Reefer && t.LengthMeters == 16.15m),
            Arg.Any<CancellationToken>());
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImportAsync_ExistingWithoutUpdateExisting_SkipsIt()
    {
        var existing = new Trailer { Number = "T-1", Type = TrailerType.DryVan, Make = "Utility" };
        _trailers.Add(existing);

        var results = await _sut.ImportAsync([Row(2, (Keys.Number, "T-1"), (Keys.Make, "Wabash"))],
            Options(commit: true), CancellationToken.None);

        Assert.Equal(ImportRowAction.Skip, Assert.Single(results).Action);
        Assert.Equal("Utility", existing.Make);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImportAsync_ExistingWithUpdateExisting_UpdatesProvidedFieldsOnly()
    {
        var existing = new Trailer { Number = "T-1", Type = TrailerType.DryVan, Make = "Utility", Vin = "VIN1" };
        _trailers.Add(existing);

        var results = await _sut.ImportAsync([Row(2, (Keys.Number, "T-1"), (Keys.Make, "Wabash"))],
            Options(commit: true, updateExisting: true), CancellationToken.None);

        Assert.Equal(ImportRowAction.Update, Assert.Single(results).Action);
        Assert.Equal("Wabash", existing.Make);
        Assert.Equal("VIN1", existing.Vin);
        await _trailerRepo.DidNotReceive().AddAsync(Arg.Any<Trailer>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImportAsync_DryRun_ReportsWithoutWriting()
    {
        var results = await _sut.ImportAsync([Row(2, (Keys.Number, "T-1"))], Options(commit: false),
            CancellationToken.None);

        Assert.Equal(ImportRowAction.Create, Assert.Single(results).Action);
        await _trailerRepo.DidNotReceive().AddAsync(Arg.Any<Trailer>(), Arg.Any<CancellationToken>());
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Row errors

    [Fact]
    public async Task ImportAsync_DuplicateNumberInFile_FailsTheLaterRow()
    {
        var results = await _sut.ImportAsync([Row(2, (Keys.Number, "T-1")), Row(3, (Keys.Number, "t-1"))],
            Options(commit: false), CancellationToken.None);

        Assert.Equal(ImportRowAction.Create, results[0].Action);
        Assert.Equal(ImportRowAction.Error, results[1].Action);
        Assert.Equal(3, results[1].RowNumber);
    }

    [Fact]
    public async Task ImportAsync_MissingNumberAndBadYear_ReportsBoth()
    {
        var result = Assert.Single(await _sut.ImportAsync([Row(2, (Keys.Year, "twenty"))], Options(commit: false),
            CancellationToken.None));

        Assert.Equal(ImportRowAction.Error, result.Action);
        Assert.Contains(result.Messages, m => m.Contains("Trailer number is required"));
        Assert.Contains(result.Messages, m => m.Contains("Year 'twenty'"));
    }

    [Fact]
    public async Task LoadImporter_UnknownCustomer_IsARowError()
    {
        var userId = Guid.NewGuid();
        var dispatcher = new Employee { Id = userId, Email = "me@test.dev", FirstName = "Me", LastName = "Dispatcher" };
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.GetUserId().Returns(userId);
        var loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();
        var customerRepo = Substitute.For<ITenantRepository<Customer, Guid>>();
        var truckRepo = Substitute.For<ITenantRepository<Truck, Guid>>();
        var employeeRepo = Substitute.For<ITenantRepository<Employee, Guid>>();
        loadRepo.Query().Returns(_ => new List<Load>().BuildMock());
        customerRepo.Query().Returns(_ => new List<Customer>().BuildMock());
        truckRepo.Query().Returns(_ => new List<Truck>().BuildMock());
        employeeRepo.Query().Returns(_ => new List<Employee> { dispatcher }.BuildMock());
        employeeRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(dispatcher);
        _tenantUow.Repository<Load>().Returns(loadRepo);
        _tenantUow.Repository<Customer>().Returns(customerRepo);
        _tenantUow.Repository<Truck>().Returns(truckRepo);
        _tenantUow.Repository<Employee>().Returns(employeeRepo);
        var sut = new LoadImporter(_tenantUow, Substitute.For<IGeocodingService>(),
            Substitute.For<IVehicleTransportGuard>(), currentUser);

        var row = Row(2,
            (Keys.Reference, "PRO-1"), (Keys.Customer, "Nobody Inc"), (Keys.Rate, "$1,200"),
            ("pickupFullAddress", "1 Main St, Dallas, TX 75201"), ("deliveryFullAddress", "2 Bay Rd, Houston, TX 77001"));

        var result = Assert.Single(await sut.ImportAsync([row], Options(commit: false), CancellationToken.None));

        Assert.Equal(ImportRowAction.Error, result.Action);
        Assert.Contains(result.Messages, m => m.Contains("Unknown customer 'Nobody Inc'"));
    }

    [Fact]
    public async Task CustomerImporter_IncompleteAddressAndForeignCountry_AreRowErrors()
    {
        var customerRepo = Substitute.For<ITenantRepository<Customer, Guid>>();
        customerRepo.Query().Returns(_ => new List<Customer>().BuildMock());
        _tenantUow.Repository<Customer>().Returns(customerRepo);
        var sut = new CustomerImporter(_tenantUow);

        var results = await sut.ImportAsync(
        [
            Row(2, (Keys.Name, "Acme"), (Keys.City, "Dallas")),
            Row(3, (Keys.Name, "Berlin GmbH"), (Keys.FullAddress, "Invalidenstr. 1, 10115 Berlin, Germany"),
                (Keys.State, "BE"))
        ], Options(commit: false), CancellationToken.None);

        Assert.All(results, r => Assert.Equal(ImportRowAction.Error, r.Action));
        Assert.Contains(results[0].Messages, m => m.StartsWith("Address:"));
        Assert.Contains(results[1].Messages, m => m.Contains("outside your company's US region"));
    }

    #endregion

    private static ImportRow Row(int rowNumber, params (string Key, string Value)[] cells) =>
        new(rowNumber, cells.ToDictionary(c => c.Key, c => c.Value), new Dictionary<string, string>());

    private static ImportOptions Options(bool commit, bool updateExisting = false) =>
        new(commit, updateExisting, false, Region.US, DateFormatType.US, "USD", "US");
}
