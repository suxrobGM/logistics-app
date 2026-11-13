using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Platform.DemoRequests;

[Mapper]
public static partial class DemoRequestMapper
{
    public static partial DemoRequestDto ToDto(this DemoRequest entity);
}
