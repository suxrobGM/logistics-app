using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

internal sealed class DeleteLoadBoardConfigurationHandler(ITenantUnitOfWork tenantUow)
    : DeleteTenantEntityHandler<DeleteLoadBoardConfigurationCommand, LoadBoardConfiguration>(tenantUow);
