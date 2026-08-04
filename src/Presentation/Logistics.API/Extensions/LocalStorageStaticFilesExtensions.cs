using Logistics.Infrastructure.Storage;
using Logistics.Infrastructure.Storage.Providers;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Logistics.API.Extensions;

public static class LocalStorageStaticFilesExtensions
{
    /// <summary>
    /// Serves local file-storage uploads as static files, whenever
    /// <see cref="Registrar.AddStorageInfrastructure"/> picked the file provider.
    /// </summary>
    public static IApplicationBuilder UseLocalStorageStaticFiles(this IApplicationBuilder app)
    {
        var config = app.ApplicationServices.GetRequiredService<IConfiguration>();
        if (!Registrar.IsFileBlobStorage(config))
        {
            return app;
        }

        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        var options = app.ApplicationServices.GetRequiredService<IOptions<FileBlobStorageOptions>>().Value;
        var cacheSeconds = options.CacheSeconds;

        var absoluteUploadsPath = Path.IsPathRooted(options.RootPath)
            ? options.RootPath
            : Path.Combine(env.ContentRootPath, options.RootPath);

        Directory.CreateDirectory(absoluteUploadsPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(absoluteUploadsPath),
            RequestPath = new PathString(options.RequestPath),
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
