using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Logistics.Application.Services;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Storage.Providers;

/// <summary>
///     Cloudflare R2 (S3-compatible) blob storage implementation.
///     Uses a single bucket with keys prefixed by tenant id: {tenantId}/{container}/{blobName}.
/// </summary>
public class R2BlobStorageService(
    IAmazonS3 s3Client,
    IOptions<R2BlobStorageOptions> options,
    ITenantService tenantService)
    : IBlobStorageService
{
    private readonly R2BlobStorageOptions options = options.Value;

    public async Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType,
        CancellationToken ct = default)
    {
        var tenantId = tenantService.GetCurrentTenant().Id;
        var key = GetTenantKey(containerName, blobName, tenantId);

        var request = new PutObjectRequest
        {
            BucketName = options.BucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false,
            DisablePayloadSigning = true
        };

        await s3Client.PutObjectAsync(request, ct);
        return GetPublicUrl(containerName, blobName, tenantId);
    }

    public async Task<Stream> DownloadAsync(string containerName, string blobName, CancellationToken ct = default)
    {
        var key = GetTenantKey(containerName, blobName);

        using var response = await s3Client.GetObjectAsync(options.BucketName, key, ct);
        var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream, ct);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task DeleteAsync(string containerName, string blobName, CancellationToken ct = default)
    {
        var key = GetTenantKey(containerName, blobName);
        await s3Client.DeleteObjectAsync(options.BucketName, key, ct);
    }

    public async Task<bool> ExistsAsync(string containerName, string blobName, CancellationToken ct = default)
    {
        var key = GetTenantKey(containerName, blobName);

        try
        {
            await s3Client.GetObjectMetadataAsync(options.BucketName, key, ct);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<BlobFileProperties> GetPropertiesAsync(string containerName, string blobName,
        CancellationToken ct = default)
    {
        var key = GetTenantKey(containerName, blobName);
        var response = await s3Client.GetObjectMetadataAsync(options.BucketName, key, ct);

        return new BlobFileProperties(
            response.Headers.ContentType,
            response.ContentLength,
            response.LastModified ?? DateTimeOffset.UtcNow,
            response.ETag?.Trim('"') ?? string.Empty
        );
    }

    public string GetPublicUrl(string containerName, string blobName, Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(options.PublicBaseUrl))
        {
            throw new InvalidOperationException(
                $"R2 {nameof(R2BlobStorageOptions.PublicBaseUrl)} is not configured. " +
                "Set a public base URL (custom domain or r2.dev subdomain) to enable public blob URLs.");
        }

        return $"{options.PublicBaseUrl.TrimEnd('/')}/{tenantId}/{containerName}/{blobName}";
    }

    private string GetTenantKey(string containerName, string blobName)
    {
        var tenantId = tenantService.GetCurrentTenant().Id;
        return GetTenantKey(containerName, blobName, tenantId);
    }

    private static string GetTenantKey(string containerName, string blobName, Guid tenantId) =>
        $"{tenantId}/{containerName}/{blobName}";
}

public class R2BlobStorageOptions
{
    public const string SectionName = "R2BlobStorage";

    public string AccountId { get; set; } = string.Empty;
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string? PublicBaseUrl { get; set; }
}
