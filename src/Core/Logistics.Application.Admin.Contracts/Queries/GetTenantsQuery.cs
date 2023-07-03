using System.Text.Json.Serialization;

namespace Logistics.Application.Admin.Queries;

public sealed class GetTenantsQuery : SearchableRequest<TenantDto>
{
    [JsonIgnore]
    public bool IncludeConnectionStrings { get; set; }
}
