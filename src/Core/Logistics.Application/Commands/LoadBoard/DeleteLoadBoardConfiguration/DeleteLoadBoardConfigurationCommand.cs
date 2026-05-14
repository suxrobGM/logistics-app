using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

[RequiresFeature(TenantFeature.LoadBoard)]
public class DeleteLoadBoardConfigurationCommand : ICommand
{
    public Guid Id { get; set; }
}
