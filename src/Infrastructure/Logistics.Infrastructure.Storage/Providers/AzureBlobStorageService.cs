using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Logistics.Domain.Persistence;
using Microsoft.Extensions.Options;
using Logistics.Application.Abstractions.Storage;

namespace Logistics.Infrastructure.Storage.Providers;

public class AzureBlobStorageService(
    BlobServiceClient blobServiceClient,
    IOptions<AzureBlobStorageOptions> options,
    ITenantUnitOfWork tenantUow)
    : IBlobStorageService
{
    private readonly AzureBlobStorageOptions options = options.Value;

    public async Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType,
        CancellationToken ct = default)
    {
        var tenantAwareContainerName = GetTenantAwareContainerName(containerName);
        var containerClient = await GetContainerClientAsync(tenantAwareContainerName, ct);
        var blobClient = containerClient.GetBlobClient(blobName);

        var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };

        await blobClient.UploadAsync(content,
            new BlobUploadOptions { HttpHeaders = blobHttpHeaders, Conditions = null }, ct);

        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadAsync(string containerName, string blobName,
        CancellationToken ct = default)
    {
        var tenantAwareContainerName = GetTenantAwareContainerName(containerName);
        var containerClient = await GetContainerClientAsync(tenantAwareContainerName, ct);
        var blobClient = containerClient.GetBlobClient(blobName);

        var response = await blobClient.DownloadStreamingAsync(cancellationToken: ct);
        return response.Value.Content;
    }

    public async Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var tenantAwareContainerName = GetTenantAwareContainerName(containerName);
        var containerClient = await GetContainerClientAsync(tenantAwareContainerName, cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots,
            cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(string containerName, string blobName,
        CancellationToken ct = default)
    {
        var tenantAwareContainerName = GetTenantAwareContainerName(containerName);
        var containerClient = await GetContainerClientAsync(tenantAwareContainerName, ct);
        var blobClient = containerClient.GetBlobClient(blobName);

        var response = await blobClient.ExistsAsync(ct);
        return response.Value;
    }

    public async Task<BlobFileProperties> GetPropertiesAsync(string containerName, string blobName,
        CancellationToken ct = default)
    {
        var tenantAwareContainerName = GetTenantAwareContainerName(containerName);
        var containerClient = await GetContainerClientAsync(tenantAwareContainerName, ct);
        var blobClient = containerClient.GetBlobClient(blobName);

        var response = await blobClient.GetPropertiesAsync(cancellationToken: ct);
        var properties = response.Value;

        return new BlobFileProperties(
            properties.ContentType,
            properties.ContentLength,
            properties.LastModified,
            properties.ETag.ToString()
        );
    }

    public string GetPublicUrl(string containerName, string blobName, Guid tenantId)
    {
        var tenantAwareContainerName = GetTenantAwareContainerName(containerName, tenantId);
        var containerClient = blobServiceClient.GetBlobContainerClient(tenantAwareContainerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        return blobClient.Uri.ToString();
    }

    public Task<string> GetSignedUrlAsync(string containerName, string blobName, TimeSpan expiry, Guid tenantId,
        CancellationToken ct = default)
    {
        var tenantAwareContainerName = GetTenantAwareContainerName(containerName, tenantId);
        var containerClient = blobServiceClient.GetBlobContainerClient(tenantAwareContainerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!blobClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException(
                "BlobServiceClient is not configured with shared-key credentials, so SAS URLs cannot be issued. " +
                "Configure AzureBlobStorage:ConnectionString with an account key, or implement user-delegation SAS.");
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = tenantAwareContainerName,
            BlobName = blobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return Task.FromResult(blobClient.GenerateSasUri(sasBuilder).ToString());
    }

    private string GetTenantAwareContainerName(string containerName)
    {
        var tenant = tenantUow.GetCurrentTenant();
        return GetTenantAwareContainerName(containerName, tenant.Id);
    }

    private static string GetTenantAwareContainerName(string containerName, Guid tenantId)
    {
        var tenantIdStr = tenantId.ToString().ToLowerInvariant().Replace("-", "");

        // Azure container names must be lowercase and can't contain hyphens,
        // So we create a tenant-specific container name
        return $"tenant{tenantIdStr}-{containerName}".ToLowerInvariant();
    }

    private async Task<BlobContainerClient> GetContainerClientAsync(string containerName,
        CancellationToken ct)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: ct);
        return containerClient;
    }
}

public class AzureBlobStorageOptions
{
    public const string SectionName = "AzureBlobStorage";

    public string ConnectionString { get; set; } = string.Empty;
    public string DefaultContainer { get; set; } = "documents";
}
