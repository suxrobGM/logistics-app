using System.Text.Json.Serialization;

namespace Logistics.Application.Admin.Queries;

public sealed class GetTenantsRequest : SearchableRequest<TenantDto>
{
    [JsonIgnore]
    public bool IncludeConnectionStrings { get; set; }
}
