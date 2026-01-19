using Logistics.Application.Abstractions;
using Logistics.Application.Constants;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class DownloadExpenseReceiptHandler(
    ITenantUnitOfWork tenantUow,
    IBlobStorageService blobStorageService)
    : IAppRequestHandler<DownloadExpenseReceiptQuery, Result<DocumentDownloadDto>>
{
    public async Task<Result<DocumentDownloadDto>> Handle(DownloadExpenseReceiptQuery req, CancellationToken ct)
    {
        var expense = await tenantUow.Repository<Expense>().GetByIdAsync(req.ExpenseId, ct);
        if (expense is null)
        {
            return Result<DocumentDownloadDto>.Fail($"Could not find expense with ID '{req.ExpenseId}'");
        }

        if (string.IsNullOrEmpty(expense.ReceiptBlobPath))
        {
            return Result<DocumentDownloadDto>.Fail("This expense does not have a receipt attached");
        }

        // Verify requester exists (audit)
        var requester = await tenantUow.Repository<Employee>().GetByIdAsync(req.RequestedById, ct);
        if (requester is null)
        {
            return Result<DocumentDownloadDto>.Fail($"Could not find employee with ID '{req.RequestedById}'");
        }

        try
        {
            var exists =
                await blobStorageService.ExistsAsync(BlobConstants.ReceiptsContainerName, expense.ReceiptBlobPath, ct);
            if (!exists)
            {
                return Result<DocumentDownloadDto>.Fail("Receipt file not found in storage");
            }

            var stream =
                await blobStorageService.DownloadAsync(BlobConstants.ReceiptsContainerName, expense.ReceiptBlobPath,
                    ct);
            var properties =
                await blobStorageService.GetPropertiesAsync(BlobConstants.ReceiptsContainerName,
                    expense.ReceiptBlobPath, ct);

            // Extract filename from blob path
            var fileName = Path.GetFileName(expense.ReceiptBlobPath);

            var dto = new DocumentDownloadDto
            {
                FileName = fileName,
                OriginalFileName = fileName,
                ContentType = properties.ContentType,
                FileSizeBytes = properties.ContentLength,
                FileContent = stream
            };

            return Result<DocumentDownloadDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return Result<DocumentDownloadDto>.Fail($"Failed to download receipt: {ex.Message}");
        }
    }
}
