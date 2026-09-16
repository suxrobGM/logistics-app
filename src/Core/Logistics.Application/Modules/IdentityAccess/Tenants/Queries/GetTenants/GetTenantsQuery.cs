using System.Text.Json.Serialization;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Queries;

public sealed class GetTenantsQuery : SearchableQuery, IQuery<PagedResult<TenantDto>>
{
    [JsonIgnore] public bool IncludeConnectionStrings { get; set; }
}
