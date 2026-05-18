using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Privacy.Commands;

public class RequestDataExportCommand : ICommand<Result<Guid>>
{
}
