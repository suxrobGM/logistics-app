using Logistics.Application.Abstractions.Storage;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Application.Modules.Integrations.Documents.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Logistics.Application.Tests.Integrations.Documents;

public class DeliveryDocumentBlobCleanupTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IBlobStorageService blobStorage = Substitute.For<IBlobStorageService>();
    private readonly IDocumentAccessService documentAccess = Substitute.For<IDocumentAccessService>();
    private readonly DeliveryDocumentService sut;

    public DeliveryDocumentBlobCleanupTests()
    {
        var caller = new DocumentCaller(Guid.NewGuid(), IsReviewer: true);

        tenantUow.Repository<DeliveryDocument>()
            .Returns(Substitute.For<ITenantRepository<DeliveryDocument, Guid>>());
        documentAccess.ResolveCallerAsync(Arg.Any<CancellationToken>()).Returns(caller);
        documentAccess.CanAccessOwnerAsync(
                caller, DocumentOwnerType.Load, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(true);
        blobStorage.UploadAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Stream>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns("https://storage.test/document");

        sut = new DeliveryDocumentService(
            tenantUow, blobStorage, documentAccess, NullLogger<DeliveryDocumentService>.Instance);
    }

    [Fact]
    public async Task Capture_DatabaseWritesNoRows_DeletesUploadedPhoto()
    {
        tenantUow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(0);

        var result = await sut.CaptureAsync(
            DeliveryDocumentKind.ProofOfDelivery, CaptureWithOnePhoto(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await ReceivedOneDeleteAsync();
    }

    [Fact]
    public async Task Capture_DatabaseSaveThrows_DeletesUploadedPhoto()
    {
        tenantUow.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));

        var result = await sut.CaptureAsync(
            DeliveryDocumentKind.BillOfLading, CaptureWithOnePhoto(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await ReceivedOneDeleteAsync();
    }

    private async Task ReceivedOneDeleteAsync() =>
        await blobStorage.Received(1).DeleteAsync(
            BlobConstants.DocumentsContainerName,
            Arg.Any<string>(),
            CancellationToken.None);

    private static CaptureDeliveryDocumentParameters CaptureWithOnePhoto() => new(
        LoadId: Guid.NewGuid(),
        TripStopId: null,
        Photos:
        [
            new FileUpload
            {
                Content = new MemoryStream([1, 2, 3]),
                FileName = "photo.jpg",
                ContentType = "image/jpeg",
                FileSizeBytes = 3
            }
        ],
        SignatureBase64: null,
        RecipientName: null,
        Latitude: null,
        Longitude: null,
        Notes: null);
}
