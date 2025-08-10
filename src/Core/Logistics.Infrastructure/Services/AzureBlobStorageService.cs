using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Logistics.Domain.Services;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Services;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ITenantService _tenantService;
    private readonly AzureBlobStorageOptions _options;

    public AzureBlobStorageService(BlobServiceClient blobServiceClient, IOptions<AzureBlobStorageOptions> options, ITenantService tenantService)
    {
        _blobServiceClient = blobServiceClient;
        _tenantService = tenantService;
        _options = options.Value;
    }

    public async Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var tenantAwareContainerName = GetTenantAwareContainerName(containerName);
        var containerClient = await GetContainerClientAsync(tenantAwareContainerName, cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);

        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = contentType
        };

        await blobClient.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeaders,
            Conditions = null
        }, cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var tenantAwareContainerName = GetTenantAwareContainerName(containerName);
        var containerClient = await GetContainerClientAsync(tenantAwareContainerName, cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);

        var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }

    public async Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var tenantAwareContainerName = GetTenantAwareContainerName(containerName);
        var containerClient = await GetContainerClientAsync(tenantAwareContainerName, cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var tenantAwareContainerName = GetTenantAwareContainerName(containerName);
        var containerClient = await GetContainerClientAsync(tenantAwareContainerName, cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);

        var response = await blobClient.ExistsAsync(cancellationToken);
        return response.Value;
    }

    public async Task<BlobFileProperties> GetPropertiesAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var tenantAwareContainerName = GetTenantAwareContainerName(containerName);
        var containerClient = await GetContainerClientAsync(tenantAwareContainerName, cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);

        var response = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
        var properties = response.Value;

        return new BlobFileProperties(
            properties.ContentType,
            properties.ContentLength,
            properties.LastModified,
            properties.ETag.ToString()
        );
    }

    private string GetTenantAwareContainerName(string containerName)
    {
        var tenant = _tenantService.GetTenant();
        var tenantId = tenant.Id.ToString().ToLowerInvariant().Replace("-", "");
        
        // Azure container names must be lowercase and can't contain hyphens
        // So we create a tenant-specific container name
        return $"tenant{tenantId}-{containerName}".ToLowerInvariant();
    }

    private async Task<BlobContainerClient> GetContainerClientAsync(string containerName, CancellationToken cancellationToken)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        return containerClient;
    }
}

public class AzureBlobStorageOptions
{
    public const string SectionName = "AzureBlobStorage";
    
    public string ConnectionString { get; set; } = string.Empty;
    public string DefaultContainer { get; set; } = "documents";
}
