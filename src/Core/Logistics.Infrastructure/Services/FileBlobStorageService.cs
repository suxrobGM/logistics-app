using Logistics.Domain.Services;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Services;

public class FileBlobStorageService : IBlobStorageService
{
    private readonly FileBlobStorageOptions _options;
    private readonly ITenantService _tenantService;

    public FileBlobStorageService(IOptions<FileBlobStorageOptions> options, ITenantService tenantService)
    {
        _options = options.Value;
        _tenantService = tenantService;
    }

    public async Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var containerPath = GetContainerPath(containerName);
        EnsureDirectoryExists(containerPath);

        var filePath = Path.Combine(containerPath, blobName);
        var fileDirectory = Path.GetDirectoryName(filePath);
        
        if (!string.IsNullOrEmpty(fileDirectory))
        {
            EnsureDirectoryExists(fileDirectory);
        }

        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, cancellationToken);

        // Store metadata alongside the file
        await StoreMetadataAsync(filePath, contentType, content.Length);

        return GetFileUri(containerName, blobName);
    }

    public async Task<Stream> DownloadAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(containerName, blobName);
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {blobName}");
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return await Task.FromResult(fileStream);
    }

    public async Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(containerName, blobName);
        var metadataPath = GetMetadataPath(filePath);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        if (File.Exists(metadataPath))
        {
            File.Delete(metadataPath);
        }

        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(containerName, blobName);
        return await Task.FromResult(File.Exists(filePath));
    }

    public async Task<BlobFileProperties> GetPropertiesAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(containerName, blobName);
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {blobName}");
        }

        var fileInfo = new FileInfo(filePath);
        var metadata = await LoadMetadataAsync(filePath);

        return new BlobFileProperties(
            metadata.ContentType,
            fileInfo.Length,
            fileInfo.LastWriteTime,
            GenerateETag(fileInfo)
        );
    }

    private string GetContainerPath(string containerName)
    {
        var tenant = _tenantService.GetTenant();
        var tenantId = tenant.Id.ToString();
        return Path.Combine(_options.RootPath, tenantId, containerName);
    }

    private string GetFilePath(string containerName, string blobName)
    {
        var containerPath = GetContainerPath(containerName);
        return Path.Combine(containerPath, blobName);
    }

    private string GetMetadataPath(string filePath)
    {
        return $"{filePath}.metadata";
    }

    private string GetFileUri(string containerName, string blobName)
    {
        var tenant = _tenantService.GetTenant();
        var tenantId = tenant.Id.ToString();
        
        if (!string.IsNullOrEmpty(_options.BaseUrl))
        {
            return $"{_options.BaseUrl.TrimEnd('/')}/{tenantId}/{containerName}/{blobName}";
        }
        
        return $"file:///{tenantId}/{containerName}/{blobName}";
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private static string GenerateETag(FileInfo fileInfo)
    {
        var hash = $"{fileInfo.Length}-{fileInfo.LastWriteTime.Ticks}";
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(hash));
    }

    private async Task StoreMetadataAsync(string filePath, string contentType, long contentLength)
    {
        var metadataPath = GetMetadataPath(filePath);
        var metadata = new FileMetadata
        {
            ContentType = contentType,
            ContentLength = contentLength,
            CreatedAt = DateTime.UtcNow
        };

        var json = System.Text.Json.JsonSerializer.Serialize(metadata);
        await File.WriteAllTextAsync(metadataPath, json);
    }

    private async Task<FileMetadata> LoadMetadataAsync(string filePath)
    {
        var metadataPath = GetMetadataPath(filePath);
        
        if (!File.Exists(metadataPath))
        {
            // Fallback to file info if metadata doesn't exist
            var fileInfo = new FileInfo(filePath);
            return new FileMetadata
            {
                ContentType = "application/octet-stream",
                ContentLength = fileInfo.Length,
                CreatedAt = fileInfo.CreationTime
            };
        }

        var json = await File.ReadAllTextAsync(metadataPath);
        return System.Text.Json.JsonSerializer.Deserialize<FileMetadata>(json) ?? new FileMetadata
        {
            ContentType = "application/octet-stream",
            ContentLength = 0,
            CreatedAt = DateTime.UtcNow
        };
    }
}

public class FileBlobStorageOptions
{
    public const string SectionName = "FileBlobStorage";
    
    public string RootPath { get; set; } = "wwwroot/uploads";
    public string? BaseUrl { get; set; }
}

internal class FileMetadata
{
    public string ContentType { get; set; } = string.Empty;
    public long ContentLength { get; set; }
    public DateTime CreatedAt { get; set; }
}