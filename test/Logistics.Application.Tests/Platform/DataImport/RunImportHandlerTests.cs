using Logistics.Application.Abstractions.Documents;
using Logistics.Application.Modules.Platform.DataImport.Commands;
using Logistics.Application.Modules.Platform.DataImport.Importers;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Platform.DataImport;

public class RunImportHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ISpreadsheetReader _reader = Substitute.For<ISpreadsheetReader>();
    private readonly ITenantRepository<Trailer, Guid> _trailerRepo = Substitute.For<ITenantRepository<Trailer, Guid>>();
    private readonly RunImportHandler _sut;

    public RunImportHandlerTests()
    {
        _tenantUow.GetCurrentTenant().Returns(TestTenant.Create());
        _tenantUow.Repository<Trailer>().Returns(_trailerRepo);
        _trailerRepo.Query().Returns(_ => new List<Trailer> { new() { Number = "T-2", Type = TrailerType.Flatbed } }.BuildMock());
        _reader.Read(Arg.Any<Stream>(), Arg.Any<string>()).Returns(new SpreadsheetTable(
            ["Trailer #", "Type"],
            [
                new SpreadsheetRow(2, ["T-1", "Reefer"]),
                new SpreadsheetRow(3, ["T-2", "Flatbed"]),
                new SpreadsheetRow(4, [null, "Van"])
            ]));
        _sut = new RunImportHandler(_tenantUow, _reader, [new TrailerImporter(_tenantUow)]);
    }

    [Fact]
    public async Task Handle_RequiredFieldUnmapped_Fails()
    {
        var result = await _sut.Handle(Command(dryRun: true, mapNumber: false), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Trailer number", result.Error);
    }

    [Fact]
    public async Task Handle_Import_SummarisesCreatedSkippedAndFailedRows()
    {
        var result = await _sut.Handle(Command(dryRun: false, mapNumber: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var summary = result.Value!;
        Assert.True(summary.Committed);
        Assert.Equal((3, 1, 0, 1, 1), (summary.TotalRows, summary.Created, summary.Updated, summary.Skipped, summary.Failed));
        Assert.Equal([2, 3, 4], summary.Rows.Select(r => r.RowNumber));
    }

    private static RunImportCommand Command(bool dryRun, bool mapNumber) => new()
    {
        Content = new MemoryStream(),
        FileName = "trailers.csv",
        ContentType = "text/csv",
        FileSizeBytes = 10,
        EntityType = ImportEntityType.Trailers,
        DryRun = dryRun,
        Mapping =
        [
            new ImportColumnMappingDto { ColumnIndex = 0, FieldKey = mapNumber ? "number" : null },
            new ImportColumnMappingDto { ColumnIndex = 1, FieldKey = "type" }
        ]
    };
}
