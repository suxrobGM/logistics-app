using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Platform.DemoRequests.Commands;

internal sealed class DeleteDemoRequestHandler(IMasterUnitOfWork masterUow)
    : DeleteMasterEntityHandler<DeleteDemoRequestCommand, DemoRequest>(masterUow);
