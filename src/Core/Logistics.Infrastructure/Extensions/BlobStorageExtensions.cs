using Azure.Storage.Blobs;
using Logistics.Application.Services;
using Logistics.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Extensions;

public static class BlobStorageExtensions
{
    /// <summary>
    ///     Add Azure Blob Storage service
    /// </summary>
    public static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureBlobStorageOptions>(configuration.GetSection(AzureBlobStorageOptions.SectionName));

        services.AddSingleton(provider =>
        {
            var connectionString = configuration.GetConnectionString("AzureBlobStorage");
            return new BlobServiceClient(connectionString);
        });

        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
        return services;
    }

    /// <summary>
    ///     Add File System Blob Storage service
    /// </summary>
    public static IServiceCollection AddFileBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileBlobStorageOptions>(configuration.GetSection(FileBlobStorageOptions.SectionName));

        services.AddScoped<IBlobStorageService, FileBlobStorageService>();
        return services;
    }

    /// <summary>
    ///     Add Blob Storage service based on configuration
    /// </summary>
    public static IServiceCollection AddBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var storageType = configuration.GetValue<string>("BlobStorage:Type")?.ToLowerInvariant();

        return storageType switch
        {
            "azure" => services.AddAzureBlobStorage(configuration),
            "file" or "local" => services.AddFileBlobStorage(configuration),
            _ => services.AddFileBlobStorage(configuration) // Default to file storage
        };
    }
}
