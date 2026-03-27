using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

[RequiresFeature(TenantFeature.LoadBoard)]
public class DeleteLoadBoardConfigurationCommand : IAppRequest
{
    public Guid Id { get; set; }
}
