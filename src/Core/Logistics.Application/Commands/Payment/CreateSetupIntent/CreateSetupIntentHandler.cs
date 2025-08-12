using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreateSetupIntentHandler : RequestHandler<CreateSetupIntentCommand, Result<SetupIntentDto>>
{
    private readonly ILogger<CreateSetupIntentHandler> _logger;
    private readonly IStripeService _stripeService;
    private readonly ITenantUnitOfWork _tenantUow;

    public CreateSetupIntentHandler(
        ITenantUnitOfWork tenantUow,
        IStripeService stripeService,
        ILogger<CreateSetupIntentHandler> logger)
    {
        _tenantUow = tenantUow;
        _stripeService = stripeService;
        _logger = logger;
    }

    public override async Task<Result<SetupIntentDto>> Handle(CreateSetupIntentCommand req, CancellationToken ct)
    {
        var tenant = _tenantUow.GetCurrentTenant();

        var setupIntent = await _stripeService.CreateSetupIntentAsync(tenant);

        var dto = new SetupIntentDto
        {
            ClientSecret = setupIntent.ClientSecret
        };
        return Result<SetupIntentDto>.Succeed(dto);
    }
}
