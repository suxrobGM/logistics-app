using Logistics.Application.Abstractions.Storage;
using Logistics.Application.Modules.Platform.Portal.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Platform;

public class DownloadPortalLoadDocumentHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Load, Guid> _loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();
    private readonly ITenantRepository<LoadDocument, Guid> _documentRepo =
        Substitute.For<ITenantRepository<LoadDocument, Guid>>();
    private readonly IBlobStorageService _blobStorage = Substitute.For<IBlobStorageService>();

    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Load _load;
    private readonly DownloadPortalLoadDocumentHandler _sut;

    public DownloadPortalLoadDocumentHandlerTests()
    {
        _load = CreateLoad(_customerId);
        _tenantUow.Repository<Load>().Returns(_loadRepo);
        _tenantUow.Repository<LoadDocument>().Returns(_documentRepo);
        _loadRepo.GetByIdAsync(_load.Id, Arg.Any<CancellationToken>()).Returns(_load);
        _blobStorage.ExistsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        _blobStorage.DownloadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new MemoryStream([1, 2, 3]));
        _sut = new DownloadPortalLoadDocumentHandler(_tenantUow, _blobStorage);
    }

    [Fact]
    public async Task Handle_DocumentOfCustomersLoad_ReturnsFile()
    {
        var document = AddDocument(_load.Id);

        var result = await _sut.Handle(Query(_load.Id, document.Id, _customerId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("pod.pdf", result.Value!.OriginalFileName);
    }

    [Fact]
    public async Task Handle_LoadOfAnotherCustomer_FailsWithoutReadingStorage()
    {
        var document = AddDocument(_load.Id);

        var result = await _sut.Handle(Query(_load.Id, document.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _blobStorage.DidNotReceiveWithAnyArgs().DownloadAsync(default!, default!, default);
    }

    [Fact]
    public async Task Handle_DocumentOfAnotherLoad_Fails()
    {
        var document = AddDocument(Guid.NewGuid());

        var result = await _sut.Handle(Query(_load.Id, document.Id, _customerId), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_DeletedDocument_Fails()
    {
        var document = AddDocument(_load.Id);
        document.UpdateStatus(DocumentStatus.Deleted);

        var result = await _sut.Handle(Query(_load.Id, document.Id, _customerId), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    private LoadDocument AddDocument(Guid loadId)
    {
        var document = LoadDocument.Create(
            "abc.pdf", "pod.pdf", "application/pdf", 3, "loads/abc.pdf", "documents",
            DocumentType.ProofOfDelivery, loadId, Guid.NewGuid());
        _documentRepo.GetByIdAsync(document.Id, Arg.Any<CancellationToken>()).Returns(document);
        return document;
    }

    private static DownloadPortalLoadDocumentQuery Query(Guid loadId, Guid documentId, Guid customerId) => new()
    {
        LoadId = loadId,
        DocumentId = documentId,
        CustomerId = customerId
    };

    private static Load CreateLoad(Guid customerId) => new()
    {
        Name = "Load 1",
        Type = LoadType.GeneralFreight,
        OriginAddress = new Address { Line1 = "1 A St", City = "NYC", State = "NY", ZipCode = "10001", Country = "US" },
        OriginLocation = new GeoPoint(-74.0, 40.7),
        DestinationAddress = new Address { Line1 = "2 B St", City = "LA", State = "CA", ZipCode = "90001", Country = "US" },
        DestinationLocation = new GeoPoint(-118.2, 34.0),
        DeliveryCost = new Money { Amount = 1000m, Currency = "USD" },
        CustomerId = customerId,
        Customer = new Customer { Name = "ACME" }
    };
}
