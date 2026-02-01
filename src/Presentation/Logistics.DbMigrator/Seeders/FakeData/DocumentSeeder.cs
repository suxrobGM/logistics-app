using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Utils;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds sample POD (Proof of Delivery) and BOL (Bill of Lading) documents for loads.
/// </summary>
internal class DocumentSeeder(ILogger<DocumentSeeder> logger) : SeederBase(logger)
{
    private static readonly string[] recipientNames =
    [
        "John Smith", "Maria Garcia", "Robert Johnson", "Jennifer Williams",
        "Michael Brown", "Sarah Davis", "David Miller", "Emily Wilson",
        "James Anderson", "Lisa Martinez", "William Taylor", "Amanda Thomas"
    ];

    public override string Name => nameof(DocumentSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 150;
    public override IReadOnlyList<string> DependsOn =>
        [nameof(LoadSeeder), nameof(EmployeeSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<DeliveryDocument>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var loadRepository = context.TenantUnitOfWork.Repository<Load>();
        var documentRepository = context.TenantUnitOfWork.Repository<DeliveryDocument>();
        var employeeRepository = context.TenantUnitOfWork.Repository<Employee>();

        // Get all loads
        var loads = await loadRepository.GetListAsync(ct: cancellationToken);
        if (loads.Count == 0)
        {
            logger.LogWarning("No loads available for document seeding");
            LogCompleted(0);
            return;
        }

        // Get drivers from context or load from database
        var drivers = context.CreatedEmployees?.Drivers
            ?? await employeeRepository.GetListAsync(e => e.Role != null && e.Role.Name == "Driver", ct: cancellationToken);
        if (drivers.Count == 0)
        {
            logger.LogWarning("No drivers available for document seeding");
            LogCompleted(0);
            return;
        }

        var count = 0;

        // Create POD and BOL for about 60% of loads
        foreach (var load in loads.Take((int)(loads.Count * 0.6)))
        {
            var driver = random.Pick(drivers);
            var captureLocation = random.Pick(RoutePoints.Points);

            // Create Bill of Lading (pickup)
            var bol = CreateBillOfLading(load, driver, captureLocation);
            await documentRepository.AddAsync(bol, cancellationToken);
            count++;

            // Create Proof of Delivery (delivery)
            var pod = CreateProofOfDelivery(load, driver, captureLocation);
            await documentRepository.AddAsync(pod, cancellationToken);
            count++;
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        LogCompleted(count);
    }

    private DeliveryDocument CreateBillOfLading(
        Load load,
        Employee driver,
        (Domain.Primitives.ValueObjects.Address Address, double Longitude, double Latitude) location)
    {
        var capturedAt = load.PickedUpAt ?? load.DispatchedAt ?? DateTime.UtcNow.AddDays(-random.Next(1, 30));

        return DeliveryDocument.Create(
            fileName: $"bol_{load.Id}_{Guid.NewGuid():N}.jpg",
            originalFileName: $"BOL_{load.Number}.jpg",
            contentType: "image/jpeg",
            fileSizeBytes: random.Next(500_000, 2_000_000),
            blobPath: $"documents/loads/{load.Id}/bol_{Guid.NewGuid():N}.jpg",
            blobContainer: "load-documents",
            type: DocumentType.BillOfLading,
            loadId: load.Id,
            uploadedById: driver.Id,
            recipientName: random.Pick(recipientNames),
            recipientSignature: GenerateFakeSignature(),
            captureLatitude: location.Latitude + (random.NextDouble() - 0.5) * 0.01,
            captureLongitude: location.Longitude + (random.NextDouble() - 0.5) * 0.01,
            capturedAt: capturedAt,
            tripStopId: null,
            notes: "Shipper verified load contents and condition."
        );
    }

    private DeliveryDocument CreateProofOfDelivery(
        Load load,
        Employee driver,
        (Domain.Primitives.ValueObjects.Address Address, double Longitude, double Latitude) location)
    {
        var capturedAt = load.DeliveredAt ?? load.PickedUpAt?.AddHours(random.Next(4, 48)) ?? DateTime.UtcNow.AddDays(-random.Next(1, 15));

        return DeliveryDocument.Create(
            fileName: $"pod_{load.Id}_{Guid.NewGuid():N}.jpg",
            originalFileName: $"POD_{load.Number}.jpg",
            contentType: "image/jpeg",
            fileSizeBytes: random.Next(500_000, 2_000_000),
            blobPath: $"documents/loads/{load.Id}/pod_{Guid.NewGuid():N}.jpg",
            blobContainer: "load-documents",
            type: DocumentType.ProofOfDelivery,
            loadId: load.Id,
            uploadedById: driver.Id,
            recipientName: random.Pick(recipientNames),
            recipientSignature: GenerateFakeSignature(),
            captureLatitude: location.Latitude + (random.NextDouble() - 0.5) * 0.01,
            captureLongitude: location.Longitude + (random.NextDouble() - 0.5) * 0.01,
            capturedAt: capturedAt,
            tripStopId: null,
            notes: "Delivery completed successfully. No damage reported."
        );
    }

    private string GenerateFakeSignature()
    {
        // Generate a simple placeholder for base64 signature data
        // In reality, this would be actual signature image data
        var signatureBytes = new byte[100];
        random.NextBytes(signatureBytes);
        return Convert.ToBase64String(signatureBytes);
    }
}
