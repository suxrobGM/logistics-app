using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Email.Models;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Domain.Entities;
using Logistics.Application.Abstractions.Options;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Services;

internal sealed class TenantWelcomeEmailSender(
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    IOptions<IdentityServerOptions> identityServerOptions) : ITenantWelcomeEmailSender
{
    public async Task SendAsync(User owner, Tenant tenant, string? temporaryPassword = null)
    {
        var model = new TenantWelcomeEmailModel
        {
            OwnerName = $"{owner.FirstName} {owner.LastName}",
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
