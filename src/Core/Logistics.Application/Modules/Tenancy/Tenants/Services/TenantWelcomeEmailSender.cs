using Logistics.Application.Abstractions.Communications;
using Logistics.Application.Abstractions.Communications.EmailModels;
using Logistics.Application.Abstractions.Options;
using Logistics.Application.Common.Constants;
using Logistics.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Tenancy.Tenants.Services;

internal sealed class TenantWelcomeEmailSender(
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    IOptions<IdentityServerOptions> identityServerOptions) : ITenantWelcomeEmailSender
{
    public async Task SendAsync(User owner, Tenant tenant, string? temporaryPassword = null)
    {
        var model = new TenantWelcomeEmailModel
        {
            OwnerName = owner.GetFullName(),
            CompanyName = tenant.DisplayName,
            Email = owner.Email!,
            TemporaryPassword = temporaryPassword,
            LoginUrl = $"{identityServerOptions.Value.UserFacingAuthority}/Account/Login"
        };

        var body = await emailTemplateService.RenderAsync("TenantWelcome", model);
        await emailSender.SendEmailAsync(owner.Email!,
            $"Welcome to {PlatformConstants.PlatformName} - {tenant.DisplayName}", body);
    }
}
