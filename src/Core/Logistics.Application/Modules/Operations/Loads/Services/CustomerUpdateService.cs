using System.Globalization;
using Logistics.Application.Abstractions.Communications;
using Logistics.Application.Abstractions.Communications.EmailModels;
using Logistics.Application.Abstractions.Context;
using Logistics.Application.Abstractions.Options;
using Logistics.Application.Modules.Operations.Tracking.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Operations.Loads.Services;

internal sealed class CustomerUpdateService(
    ITenantUnitOfWork tenantUow,
    IFeatureService featureService,
    ITrackingLinkService trackingLinks,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    IOptions<CustomerPortalOptions> portalOptions,
    ILogger<CustomerUpdateService> logger) : ICustomerUpdateService
{
    public async Task SendStatusUpdateAsync(Load load, LoadStatus newStatus, CancellationToken ct = default)
    {
        if (GetStatusEvent(newStatus) is not { } updateEvent || load.WasCustomerUpdateSent(updateEvent))
        {
            return;
        }

        await DeliverAsync(load, updateEvent, message: null, eta: null, ct);
    }

    public async Task SendLateNoticeAsync(TripStop stop, DateTime eta, CancellationToken ct = default)
    {
        await DeliverAsync(stop.Load, CustomerUpdateEvent.Late, message: null, eta, ct);
    }

    public async Task<Result> SendDelayNoticeAsync(Load load, string message, CancellationToken ct = default)
    {
        var delivery = await DeliverAsync(load, CustomerUpdateEvent.DelayNotice, message, GetDeliveryEta(load), ct);
        return delivery.Outcome == CustomerUpdateOutcome.Sent
            ? Result.Ok()
            : Result.Fail(delivery.Reason ?? "The delay notice could not be sent.");
    }

    private async Task<Delivery> DeliverAsync(
        Load load, CustomerUpdateEvent updateEvent, string? message, DateTime? eta, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.CustomerUpdates))
        {
            return new Delivery(CustomerUpdateOutcome.Skipped, "Customer updates are turned off for this tenant");
        }

        List<string> recipients = load.Customer is { } customer ? await GetRecipientsAsync(customer, ct) : [];
        var skipReason = GetSkipReason(load.Customer, updateEvent, recipients.Count);
        if (skipReason is not null)
        {
            load.RecordCustomerUpdate(updateEvent, CustomerUpdateOutcome.Skipped, 0, skipReason);
            return new Delivery(CustomerUpdateOutcome.Skipped, skipReason);
        }

        try
        {
            var model = await BuildModelAsync(tenant, load, updateEvent, message, eta, ct);
            var (templateName, headline) = GetEmailKind(updateEvent);
            var body = await emailTemplateService.RenderAsync(templateName, model);
            var subject = string.Create(
                CultureInfo.InvariantCulture, $"{headline} - Load #{load.Number} | {model.CompanyName}");

            var sent = 0;
            foreach (var recipient in recipients)
            {
                if (await emailSender.SendEmailAsync(recipient, subject, body))
                {
                    sent++;
                }
            }

            var outcome = sent > 0 ? CustomerUpdateOutcome.Sent : CustomerUpdateOutcome.Failed;
            var reason = sent < recipients.Count ? $"{recipients.Count - sent} of {recipients.Count} emails failed" : null;
            load.RecordCustomerUpdate(updateEvent, outcome, sent, reason);
            return new Delivery(outcome, reason);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to send the {Event} customer update for load {LoadId}", updateEvent, load.Id);
            const string reason = "The email could not be sent";
            load.RecordCustomerUpdate(updateEvent, CustomerUpdateOutcome.Failed, 0, reason);
            return new Delivery(CustomerUpdateOutcome.Failed, reason);
        }
    }

    private static string? GetSkipReason(Customer? customer, CustomerUpdateEvent updateEvent, int recipientCount)
    {
        if (customer is null)
        {
            return "The load has no customer";
        }

        if (!customer.UpdatePreferences.Wants(updateEvent))
        {
            return "Turned off for this customer";
        }

        return recipientCount == 0 ? "The customer has no email address" : null;
    }

    private async Task<List<string>> GetRecipientsAsync(Customer customer, CancellationToken ct)
    {
        var portalEmails = await tenantUow.Repository<CustomerUser>().Query()
            .Where(u => u.CustomerId == customer.Id && u.IsActive)
            .Select(u => u.Email)
            .ToListAsync(ct);

        return
        [
            .. new[] { customer.Email }
                .Concat(portalEmails)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
        ];
    }

    private async Task<ShipmentUpdateEmailModel> BuildModelAsync(
        Tenant tenant, Load load, CustomerUpdateEvent updateEvent, string? message, DateTime? eta,
        CancellationToken ct)
    {
        var times = new ShipmentTimeFormatter(tenant.Settings.Timezone);
        var isDelivered = updateEvent == CustomerUpdateEvent.Delivered;
        var trackingUrl = isDelivered ? null : trackingLinks.GetUrl(await trackingLinks.GetOrCreateAsync(load.Id, ct));

        return new ShipmentUpdateEmailModel
        {
            CompanyName = tenant.DisplayName,
            LoadNumber = load.Number,
            LoadName = load.Name,
            OriginCity = load.OriginAddress.City,
            DestinationCity = load.DestinationAddress.City,
            Window = updateEvent == CustomerUpdateEvent.Dispatched
                ? times.FormatWindow(load.PickupAppointmentFrom, load.PickupAppointmentTo)
                : times.FormatWindow(load.DeliveryAppointmentFrom, load.DeliveryAppointmentTo),
            Eta = times.Format(eta ?? (updateEvent == CustomerUpdateEvent.PickedUp ? GetDeliveryEta(load) : null)),
            TrackingUrl = trackingUrl,
            ShipmentUrl = $"{portalOptions.Value.BaseUrl}/shipments/{load.Id}",
            Message = message,
            DeliveredAt = isDelivered ? times.FormatDate(load.DeliveredAt ?? DateTime.UtcNow) : null,
            ReceiverName = isDelivered ? await GetReceiverNameAsync(load.Id, ct) : null,
            Vehicles = load.Type == LoadType.Vehicle
                ? [.. load.Vehicles.Select(v => v.Vin is null ? v.GetDisplayName() : $"{v.GetDisplayName()} - VIN {v.Vin}")]
                : []
        };
    }

    private async Task<string?> GetReceiverNameAsync(Guid loadId, CancellationToken ct)
    {
        return await tenantUow.Repository<DeliveryDocument>().Query()
            .Where(d => d.LoadId == loadId && d.Type == DocumentType.ProofOfDelivery && d.RecipientName != null)
            .OrderByDescending(d => d.CapturedAt)
            .Select(d => d.RecipientName)
            .FirstOrDefaultAsync(ct);
    }

    private static DateTime? GetDeliveryEta(Load load)
    {
        return load.TripStops
            .Where(s => s.Type == TripStopType.DropOff && s.ArrivedAt == null)
            .Select(s => s.EstimatedArrivalAt)
            .FirstOrDefault();
    }

    private static CustomerUpdateEvent? GetStatusEvent(LoadStatus status) => status switch
    {
        LoadStatus.Dispatched => CustomerUpdateEvent.Dispatched,
        LoadStatus.PickedUp => CustomerUpdateEvent.PickedUp,
        LoadStatus.Delivered => CustomerUpdateEvent.Delivered,
        _ => null
    };

    private static (string TemplateName, string Headline) GetEmailKind(CustomerUpdateEvent updateEvent) =>
        updateEvent switch
        {
            CustomerUpdateEvent.Dispatched => ("ShipmentDispatched", "Scheduled"),
            CustomerUpdateEvent.PickedUp => ("ShipmentPickedUp", "Picked up"),
            CustomerUpdateEvent.Delivered => ("ShipmentDelivered", "Delivered"),
            _ => ("ShipmentLate", "Running late")
        };

    private sealed record Delivery(CustomerUpdateOutcome Outcome, string? Reason);
}
