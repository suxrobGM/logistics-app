namespace Logistics.Application.Abstractions.Storage;

public static class BlobStorageExtensions
{
    /// <summary>Reads a whole blob into memory, for attachments and images embedded in a PDF.</summary>
    public static async Task<byte[]> DownloadBytesAsync(
        this IBlobStorageService blobStorage, string containerName, string blobName, CancellationToken ct = default)
    {
        await using var stream = await blobStorage.DownloadAsync(containerName, blobName, ct);
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, ct);
        return buffer.ToArray();
    }
}
