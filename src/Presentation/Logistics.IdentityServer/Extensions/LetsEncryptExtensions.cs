namespace Logistics.IdentityServer.Extensions;

public static class LetsEncryptExtensions
{
    public static IApplicationBuilder UseLetsEncryptChallenge(this IApplicationBuilder builder)
    {
        return builder.UseRouter(route =>
        {
            route.MapGet(".well-known/acme-challenge/{id}", async (request, response, routeData) =>
            {
                var id = routeData.Values["id"] as string;
                var hostEnv = request.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();
                var file = Path.Combine(hostEnv.ContentRootPath, ".well-known", "acme-challenge", id!);
                await response.SendFileAsync(file);
            });
        });
    }
}
