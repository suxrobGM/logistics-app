using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class RequestDataExportCommand : ICommand<Result<Guid>>
{
}
