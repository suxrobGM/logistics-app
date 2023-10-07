using System.Text.Json.Serialization;
using Logistics.Application.Common;
using Logistics.Models;

namespace Logistics.Application.Admin.Queries;

public sealed class GetTenantsQuery : SearchableQuery<TenantDto>
{
    [JsonIgnore]
    public bool IncludeConnectionStrings { get; set; }
}
