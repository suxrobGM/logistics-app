using System.Text.Json.Serialization;
using Logistics.Shared;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public sealed class GetTenantsQuery : SearchableQuery, IRequest<PagedResult<TenantDto>>
{
    [JsonIgnore]
    public bool IncludeConnectionStrings { get; set; }
}
