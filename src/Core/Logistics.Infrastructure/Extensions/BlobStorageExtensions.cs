using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Logistics.Domain.Services;
using Logistics.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Extensions;
public static class BlobStorageExtensions
{
    /// <summary>
    /// Add Azure Blob Storage service
    /// </summary>
    public static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureBlobStorageOptions>(options =>
            configuration.GetSection(AzureBlobStorageOptions.SectionName).Bind(options));

        services.AddSingleton(provider =>
        {
            var connectionString = configuration.GetConnectionString("AzureBlobStorage");
            return new BlobServiceClient(connectionString);
        });

        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
        return services;
    }

    /// <summary>
    /// Add File System Blob Storage service
    /// </summary>
    public static IServiceCollection AddFileBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileBlobStorageOptions>(options =>
            configuration.GetSection(FileBlobStorageOptions.SectionName).Bind(options));

        services.AddScoped<IBlobStorageService, FileBlobStorageService>();
        return services;
    }

    /// <summary>
    /// Add Blob Storage service based on configuration
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