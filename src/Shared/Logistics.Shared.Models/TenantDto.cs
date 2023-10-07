using System.Text.Json.Serialization;

namespace Logistics.Shared.Models;

public class TenantDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ConnectionString { get; set; }
}
