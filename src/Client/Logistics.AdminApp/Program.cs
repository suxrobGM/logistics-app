using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Logistics.AdminApp.Components;
using Logistics.AdminApp.Authorization;
using Logistics.HttpClient;
using Microsoft.AspNetCore.Authorization;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddApiHttpClient(builder.Configuration);
builder.Services.AddAuthorizationCore();
builder.Services.AddRadzenComponents();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Oidc", options.ProviderOptions);
    options.UserOptions.RoleClaim = "role";
});

Console.WriteLine($"Client Hosting Environment: {builder.HostEnvironment.Environment}");

await builder
    .Build()
    .RunAsync();
