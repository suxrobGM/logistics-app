using Logistics.Application.Abstractions;
using Logistics.Application.Constants;
using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UploadExpenseReceiptHandler(IBlobStorageService blobStorageService)
    : IAppRequestHandler<UploadExpenseReceiptCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UploadExpenseReceiptCommand req, CancellationToken ct)
    {
        try
        {
            var uniqueFileName = BlobPathHelper.GenerateUniqueFileName(req.FileName);
            var blobPath = BlobPathHelper.GetReceiptBlobPath(uniqueFileName);

            await blobStorageService.UploadAsync(
                BlobConstants.ReceiptsContainerName,
                blobPath,
                req.FileContent,
                req.ContentType,
                ct);

            return Result<string>.Ok(blobPath);
        }
        catch (Exception ex)
        {
            return Result<string>.Fail($"Failed to upload receipt: {ex.Message}");
        }
    }
}
