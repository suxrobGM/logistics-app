using Microsoft.Extensions.FileProviders;

namespace Logistics.API.Extensions;

public static class LocalStorageStaticFilesExtensions
{
    /// <summary>
    /// Serves files for local/file storage type from a configurable root.
    /// Reads:
    ///   BlobStorage:Type -> "file" | "local" | null (default)
    ///   FileBlobStorage:RootPath -> relative or absolute path (default: "wwwroot/uploads")
    ///   FileBlobStorage:RequestPath -> request base path (default: "/uploads")
    ///   FileBlobStorage:CacheSeconds -> Cache-Control max-age seconds (default: 3600)
    /// </summary>
    public static IApplicationBuilder UseLocalStorageStaticFiles(this IApplicationBuilder app)
    {
        var config = app.ApplicationServices.GetRequiredService<IConfiguration>();
        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

        var storageType = config.GetValue<string>("BlobStorage:Type")?.ToLowerInvariant();
        // Only enable when using local/file storage
        if (storageType is not ("file" or "local" or null))
            return app;

        var uploadsPath = config.GetValue<string>("FileBlobStorage:RootPath") ?? "wwwroot/uploads";
        var requestPath = new PathString(config.GetValue<string>("FileBlobStorage:RequestPath") ?? "/uploads");
        var cacheSeconds = config.GetValue<int?>("FileBlobStorage:CacheSeconds") ?? 3600;

        var absoluteUploadsPath = Path.IsPathRooted(uploadsPath)
            ? uploadsPath
            : Path.Combine(env.ContentRootPath, uploadsPath);

        Directory.CreateDirectory(absoluteUploadsPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(absoluteUploadsPath),
            RequestPath = requestPath,
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                ctx.Context.Response.Headers.Append("X-Frame-Options", "DENY");
                ctx.Context.Response.Headers.Append("Cache-Control", $"public,max-age={cacheSeconds}");
            }
        });

        return app;
    }
}
