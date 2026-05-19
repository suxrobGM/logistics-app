using Logistics.Application.Abstractions.Email.Models;
using Logistics.Application.Abstractions.Email;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Logistics.Application.Abstractions.Storage;
using Logistics.Application.Abstractions.Privacy;

namespace Logistics.Application.Modules.Compliance.Privacy.Services;

internal sealed class DataExportProcessingService(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IDataExportService dataExportService,
    IBlobStorageService blobStorage,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    IOptions<ImpersonationOptions> impersonationOptions,
    ILogger<DataExportProcessingService> logger) : IDataExportProcessingService
{
    private readonly string portalBaseUrl = impersonationOptions.Value.TmsPortalUrl;

    public async Task ProcessPendingAsync(CancellationToken ct = default)
    {
        var pending = await masterUow.Repository<DataExportRequest>()
            .Query()
            .Where(r => r.Status == DataExportStatus.Pending)
            .OrderBy(r => r.RequestedAt)
            .Take(20)
            .ToListAsync(ct);

        foreach (var request in pending)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await ProcessOneAsync(request, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Data export failed for request '{RequestId}'.", request.Id);
                request.Status = DataExportStatus.Failed;
                request.ErrorMessage = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
                await masterUow.SaveChangesAsync(ct);
            }
        }
    }

    private async Task ProcessOneAsync(DataExportRequest request, CancellationToken ct)
    {
        request.Status = DataExportStatus.Processing;
        await masterUow.SaveChangesAsync(ct);

        var user = await masterUow.Repository<User>().GetByIdAsync(request.UserId, ct)
            ?? throw new InvalidOperationException($"User '{request.UserId}' not found.");

        // Pick any tenant the user belongs to as the storage tenant context.
        // The blob is per-user, but the abstraction requires a tenant scope.
        var anchorTenant = await masterUow.Repository<Tenant>()
            .Query()
            .FirstOrDefaultAsync(t => t.Users.Any(u => u.Id == user.Id), ct)
            ?? throw new InvalidOperationException(
                $"User '{user.Id}' does not belong to any tenant; cannot anchor blob storage.");

        tenantUow.SetCurrentTenant(anchorTenant);

        await using var zipStream = await dataExportService.GenerateExportAsync(user.Id, ct);

        var blobName = $"{user.Id:N}/{request.Id:N}.zip";
        await blobStorage.UploadAsync(PrivacyDefaults.ExportBlobContainer, blobName, zipStream, "application/zip", ct);

        request.BlobContainer = PrivacyDefaults.ExportBlobContainer;
        request.BlobName = blobName;
        request.BlobTenantId = anchorTenant.Id;
        request.ExpiresAt = DateTime.UtcNow + PrivacyDefaults.ExportBlobRetention;
        request.Status = DataExportStatus.Ready;
        await masterUow.SaveChangesAsync(ct);

        await SendReadyEmailAsync(user, request);
        logger.LogInformation("Data export '{RequestId}' ready for user '{UserId}'.", request.Id, user.Id);
    }

    private async Task SendReadyEmailAsync(User user, DataExportRequest request)
    {
        if (string.IsNullOrEmpty(user.Email))
        {
            return;
        }

        var model = new DataExportReadyEmailModel
        {
            UserName = user.GetFullName(),
            PortalUrl = $"{portalBaseUrl.TrimEnd('/')}{PrivacyDefaults.PortalPrivacyPath}?export={request.Id}",
            ExpiresAt = request.ExpiresAt?.ToString("MMMM dd, yyyy 'UTC'") ?? "soon"
        };

        var body = await emailTemplateService.RenderAsync("DataExportReady", model);
        await emailSender.SendEmailAsync(user.Email, "Your data export is ready", body);
    }
}
