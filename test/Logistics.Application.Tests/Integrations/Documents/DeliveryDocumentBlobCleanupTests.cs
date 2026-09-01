using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Abstractions.Storage;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Application.Modules.Integrations.Documents.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Identity.Roles;
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
    private readonly ICurrentUserService currentUser = Substitute.For<ICurrentUserService>();

    public DeliveryDocumentBlobCleanupTests()
    {
        var callerId = Guid.NewGuid();
        var employeeRepo = Substitute.For<ITenantRepository<Employee, Guid>>();
        var loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();
        var documentRepo = Substitute.For<ITenantRepository<DeliveryDocument, Guid>>();

        tenantUow.Repository<Employee>().Returns(employeeRepo);
        tenantUow.Repository<Load>().Returns(loadRepo);
        tenantUow.Repository<DeliveryDocument>().Returns(documentRepo);
        currentUser.GetUserId().Returns(callerId);
        employeeRepo.GetByIdAsync(callerId, Arg.Any<CancellationToken>()).Returns(new Employee
        {
            Id = callerId,
            Email = "manager@test.com",
            FirstName = "Test",
            LastName = "Manager",
            Role = new TenantRole(TenantRoles.Manager)
        });
        loadRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<Load>());
        blobStorage.UploadAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Stream>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns("https://storage.test/document");
    }

    [Fact]
    public async Task CapturePod_DatabaseWritesNoRows_DeletesUploadedPhoto()
    {
        tenantUow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(0);
        var handler = new CaptureProofOfDeliveryHandler(
            tenantUow,
            blobStorage,
            currentUser,
            NullLogger<CaptureProofOfDeliveryHandler>.Instance);

        var result = await handler.Handle(new CaptureProofOfDeliveryCommand
        {
            LoadId = Guid.NewGuid(),
            Photos = [Photo()]
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await ReceivedOneDeleteAsync();
    }

    [Fact]
    public async Task CaptureBol_DatabaseSaveThrows_DeletesUploadedPhoto()
    {
        tenantUow.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));
        var handler = new CaptureBillOfLadingHandler(
            tenantUow,
            blobStorage,
            currentUser,
            NullLogger<CaptureBillOfLadingHandler>.Instance);

        var result = await handler.Handle(new CaptureBillOfLadingCommand
        {
            LoadId = Guid.NewGuid(),
            Photos = [Photo()]
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await ReceivedOneDeleteAsync();
    }

    private async Task ReceivedOneDeleteAsync() =>
        await blobStorage.Received(1).DeleteAsync(
            BlobConstants.DocumentsContainerName,
            Arg.Any<string>(),
            CancellationToken.None);

    private static FileUpload Photo() => new()
    {
        Content = new MemoryStream([1, 2, 3]),
        FileName = "photo.jpg",
        ContentType = "image/jpeg",
        FileSizeBytes = 3
    };
}
