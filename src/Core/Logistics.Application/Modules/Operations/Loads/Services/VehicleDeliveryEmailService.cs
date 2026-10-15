using System.Globalization;
using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Email.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Operations.Loads.Services;

internal sealed class VehicleDeliveryEmailService(
    ITenantUnitOfWork tenantUow,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    IOptions<CustomerPortalOptions> portalOptions,
    ILogger<VehicleDeliveryEmailService> logger) : IVehicleDeliveryEmailService
{
    public async Task SendAsync(Load load, CancellationToken ct = default)
    {
        var email = load.Customer?.Email;
        if (load is not { Type: LoadType.Vehicle, Status: LoadStatus.Delivered } || load.Vehicles.Count == 0
            || string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        try
        {
            var receiverName = await tenantUow.Repository<DeliveryDocument>().Query()
                .Where(d => d.LoadId == load.Id && d.Type == DocumentType.ProofOfDelivery && d.RecipientName != null)
                .OrderByDescending(d => d.CapturedAt)
                .Select(d => d.RecipientName)
                .FirstOrDefaultAsync(ct);

            var tenant = tenantUow.GetCurrentTenant();
            var companyName = tenant.CompanyName ?? tenant.Name;

            var model = new VehicleDeliveredEmailModel
            {
                CompanyName = companyName,
                LoadNumber = load.Number,
                LoadName = load.Name,
                DestinationCity = load.DestinationAddress.City,
                DeliveredAt = (load.DeliveredAt ?? DateTime.UtcNow).ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture),
                ReceiverName = receiverName,
                Vehicles = [.. load.Vehicles.Select(v => v.Vin is null ? v.GetDisplayName() : $"{v.GetDisplayName()} - VIN {v.Vin}")],
                ShipmentUrl = $"{portalOptions.Value.BaseUrl}/shipments/{load.Id}"
            };

            var body = await emailTemplateService.RenderAsync("VehicleDelivered", model);
            var sent = await emailSender.SendEmailAsync(email, $"Delivered - Load #{load.Number} | {companyName}", body);

            if (!sent)
            {
                logger.LogWarning("Delivery email for vehicle load {LoadId} was not sent", load.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send the delivery email for vehicle load {LoadId}", load.Id);
        }
    }
}
