using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Logistics.AdminApp.Components;
using Logistics.AdminApp.Authorization;
using Logistics.Client;
using Microsoft.AspNetCore.Authorization;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Oidc", options.ProviderOptions);
    options.UserOptions.RoleClaim = "role";
});

builder.Services.AddWebApiClient(builder.Configuration);
builder.Services.AddAuthorizationCore();
builder.Services.AddRadzenComponents();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

await builder
    .Build()
    .RunAsync();
