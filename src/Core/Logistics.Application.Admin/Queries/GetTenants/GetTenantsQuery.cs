using System.Text.Json.Serialization;
using Logistics.Shared;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Admin.Queries;

public sealed class GetTenantsQuery : SearchableQuery, IRequest<PagedResponseResult<TenantDto>>
{
    [JsonIgnore]
    public bool IncludeConnectionStrings { get; set; }
}
