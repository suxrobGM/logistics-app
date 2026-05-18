using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.TimeEntries.Commands;

[RequiresFeature(TenantFeature.Timesheets)]
public class DeleteTimeEntryCommand : ICommand<Result>
{
    public required Guid Id { get; set; }
}
