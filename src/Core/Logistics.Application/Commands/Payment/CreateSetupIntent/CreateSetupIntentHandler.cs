using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

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
