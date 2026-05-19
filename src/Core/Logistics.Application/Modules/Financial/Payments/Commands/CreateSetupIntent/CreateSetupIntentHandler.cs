using Logistics.Application.Abstractions;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Payments.Stripe;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

internal sealed class CreateSetupIntentHandler(
    ITenantUnitOfWork tenantUow,
    IStripePaymentService stripePaymentService,
    ILogger<CreateSetupIntentHandler> logger) : IAppRequestHandler<CreateSetupIntentCommand, Result<SetupIntentDto>>
{
    public async Task<Result<SetupIntentDto>> Handle(CreateSetupIntentCommand req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        var setupIntent = await stripePaymentService.CreateSetupIntentAsync(tenant);

        var dto = new SetupIntentDto
        {
            ClientSecret = setupIntent.ClientSecret
        };
        return Result<SetupIntentDto>.Ok(dto);
    }
}
