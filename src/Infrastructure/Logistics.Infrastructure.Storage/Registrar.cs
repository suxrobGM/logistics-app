using Azure.Storage.Blobs;
using Logistics.Application.Services;
using Logistics.Infrastructure.Storage.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Storage;

public static class Registrar
{
    /// <summary>
    ///     Add storage such as Local File System, Azure Blob Storage, etc.
    /// </summary>
    public static IServiceCollection AddStorageInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Blob Storage (Azure or File based on configuration)
        AddBlobStorage(services, configuration);
        return services;
    }

    /// <summary>
    ///     Add Blob Storage service based on configuration
    /// </summary>
    private static void AddBlobStorage(IServiceCollection services, IConfiguration configuration)
    {
        var storageType = configuration.GetValue<string>("BlobStorage:Type")?.ToLowerInvariant();

        switch (storageType)
        {
            case "azure":
            {
                AddAzureBlobStorage(services, configuration);
                break;
            }
            default:
            {
                AddFileBlobStorage(services, configuration);
                break;
            }
        }
    }

    /// <summary>
    ///     Add Azure Blob Storage service
    /// </summary>
    private static void AddAzureBlobStorage(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureBlobStorageOptions>(configuration.GetSection(AzureBlobStorageOptions.SectionName));

        services.AddSingleton(_ =>
        {
            var connectionString = configuration.GetConnectionString("AzureBlobStorage");
            return new BlobServiceClient(connectionString);
        });

        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
    }

    /// <summary>
    ///     Add File System Blob Storage service
    /// </summary>
    private static void AddFileBlobStorage(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileBlobStorageOptions>(configuration.GetSection(FileBlobStorageOptions.SectionName));
        services.AddScoped<IBlobStorageService, FileBlobStorageService>();
    }
}
