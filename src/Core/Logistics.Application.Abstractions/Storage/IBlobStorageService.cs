using Logistics.Application.Abstractions.Storage;
namespace Logistics.Application.Abstractions.Storage;

public interface IBlobStorageService
{
    /// <summary>
    ///     Upload a file to blob storage
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <param name="blobName">Blob name/path</param>
    /// <param name="content">File content stream</param>
    /// <param name="contentType">Content type</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The blob URI</returns>
    Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType,
        CancellationToken ct = default);

    /// <summary>
    ///     Download a file from blob storage
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <param name="blobName">Blob name/path</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>File content stream</returns>
    Task<Stream> DownloadAsync(string containerName, string blobName, CancellationToken ct = default);

    /// <summary>
    ///     Delete a file from blob storage
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <param name="blobName">Blob name/path</param>
    /// <param name="ct">Cancellation token</param>
    Task DeleteAsync(string containerName, string blobName, CancellationToken ct = default);

    /// <summary>
    ///     Check if a blob exists
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <param name="blobName">Blob name/path</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if exists</returns>
    Task<bool> ExistsAsync(string containerName, string blobName, CancellationToken ct = default);

    /// <summary>
    ///     Get blob properties
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <param name="blobName">Blob name/path</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Blob properties</returns>
    Task<BlobFileProperties> GetPropertiesAsync(string containerName, string blobName,
        CancellationToken ct = default);

    /// <summary>
    ///     Get the public URL for a blob
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <param name="blobName">Blob name/path</param>
    /// <param name="tenantId">Tenant ID for tenant-aware storage</param>
    /// <returns>Public URL for the blob</returns>
    string GetPublicUrl(string containerName, string blobName, Guid tenantId);

    /// <summary>
    ///     Generate a short-lived signed download URL for a blob.
    ///     Use this for sensitive downloads (eg GDPR data exports) where the URL
    ///     must not be guessable and must auto-expire.
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <param name="blobName">Blob name/path</param>
    /// <param name="expiry">Lifetime of the signed URL</param>
    /// <param name="tenantId">Tenant ID for tenant-aware storage</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Time-limited signed URL</returns>
    Task<string> GetSignedUrlAsync(string containerName, string blobName, TimeSpan expiry, Guid tenantId,
        CancellationToken ct = default);
}

public record BlobFileProperties(
    string ContentType,
    long ContentLength,
    DateTimeOffset LastModified,
    string ETag
);
