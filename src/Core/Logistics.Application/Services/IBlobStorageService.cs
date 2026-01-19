namespace Logistics.Application.Services;

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
}

public record BlobFileProperties(
    string ContentType,
    long ContentLength,
    DateTimeOffset LastModified,
    string ETag
);
