using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Operations.Trailers.Commands;

internal sealed class DeleteTrailerHandler(ITenantUnitOfWork tenantUow)
    : DeleteTenantEntityHandler<DeleteTrailerCommand, Trailer>(tenantUow);
