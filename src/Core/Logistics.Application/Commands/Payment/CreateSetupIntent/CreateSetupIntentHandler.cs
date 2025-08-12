using Logistics.Application.Services;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreateSetupIntentHandler : RequestHandler<CreateSetupIntentCommand, Result<SetupIntentDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IStripeService _stripeService;
    private readonly ILogger<CreateSetupIntentHandler> _logger;

    public CreateSetupIntentHandler(
        ITenantUnityOfWork tenantUow,
        IStripeService stripeService,
        ILogger<CreateSetupIntentHandler> logger)
    {
        _tenantUow = tenantUow;
        _stripeService = stripeService;
        _logger = logger;
    }

    protected override async Task<Result<SetupIntentDto>> HandleValidated(
        CreateSetupIntentCommand req, CancellationToken cancellationToken)
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
