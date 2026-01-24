using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace Logistics.Application.Commands;

internal sealed class SendTrackingLinkEmailHandler(
    ITenantUnitOfWork tenantUow,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    IConfiguration configuration)
    : IAppRequestHandler<SendTrackingLinkEmailCommand, Result>
{
    public async Task<Result> Handle(SendTrackingLinkEmailCommand req, CancellationToken ct)
    {
        var trackingLink = await tenantUow.Repository<TrackingLink>().GetByIdAsync(req.TrackingLinkId, ct);
        if (trackingLink is null)
        {
            return Result.Fail("Tracking link not found.");
        }

        if (!trackingLink.IsValid)
        {
            return Result.Fail("This tracking link has expired or been revoked.");
        }

        var load = await tenantUow.Repository<Load>().GetByIdAsync(trackingLink.LoadId, ct);
        if (load is null)
        {
            return Result.Fail("Load not found.");
        }

        var tenant = tenantUow.GetCurrentTenant();
        var portalBaseUrl = configuration["CustomerPortal:BaseUrl"] ?? "http://localhost:7004";
        var trackingUrl = $"{portalBaseUrl}/track/{tenant.Id}/{trackingLink.Token}";
        var companyName = tenant.CompanyName ?? tenant.Name;

        var originCity = load.OriginAddress?.City ?? "Origin";
        var destinationCity = load.DestinationAddress?.City ?? "Destination";

        var model = new TrackingLinkEmailModel
        {
            CompanyName = companyName,
            LoadNumber = load.Number,
            LoadName = load.Name,
            OriginCity = originCity,
            DestinationCity = destinationCity,
            PersonalMessage = req.PersonalMessage,
            TrackingUrl = trackingUrl,
            ExpiresAt = trackingLink.ExpiresAt.ToString("MMMM dd, yyyy")
        };

        var subject = $"Track Your Shipment - Load #{load.Number} | {companyName}";
        var body = await emailTemplateService.RenderAsync("TrackingLink", model);

        var sent = await emailSender.SendEmailAsync(req.RecipientEmail, subject, body);
        if (!sent)
        {
            return Result.Fail("Failed to send email. Please try again.");
        }

        return Result.Ok();
    }
}
