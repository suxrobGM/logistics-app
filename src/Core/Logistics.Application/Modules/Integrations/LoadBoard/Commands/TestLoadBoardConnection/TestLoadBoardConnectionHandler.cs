using Logistics.Application.Modules.Integrations.LoadBoard.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

internal sealed class TestLoadBoardConnectionHandler(
    ITenantUnitOfWork tenantUow,
    ILoadBoardProviderGateway gateway)
    : IRequestHandler<TestLoadBoardConnectionCommand, Result>
{
    public async Task<Result> Handle(TestLoadBoardConnectionCommand req, CancellationToken ct)
    {
        var config = await tenantUow.Repository<LoadBoardConfiguration>().GetByIdAsync(req.Id, ct);
        return config is null
            ? Result.Fail("Load board provider not found")
            : await gateway.TestConnectionAsync(config, ct);
    }
}
