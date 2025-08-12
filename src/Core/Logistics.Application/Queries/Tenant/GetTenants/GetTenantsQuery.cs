using System.Text.Json.Serialization;
using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetTenantsQuery : SearchableQuery, IAppRequest<PagedResult<TenantDto>>
{
    [JsonIgnore] public bool IncludeConnectionStrings { get; set; }
}
