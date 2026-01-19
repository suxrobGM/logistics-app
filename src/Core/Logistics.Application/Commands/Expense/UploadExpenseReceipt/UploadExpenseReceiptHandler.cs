using Logistics.Application.Abstractions;
using Logistics.Application.Constants;
using Logistics.Application.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UploadExpenseReceiptHandler(IBlobStorageService blobStorageService)
    : IAppRequestHandler<UploadExpenseReceiptCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UploadExpenseReceiptCommand req, CancellationToken ct)
    {
        try
        {
            // Generate unique blob path
            var extension = Path.GetExtension(req.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var blobPath = $"{DateTime.UtcNow:yyyy/MM}/{uniqueFileName}";

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
