using System.Text.Json.Serialization;

namespace Logistics.Application.Tenant.Commands;

public class UpdateNotificationCommand : Request<ResponseResult>
{
    [JsonIgnore]
    public string? Id { get; set; }
    public bool? IsRead { get; set; }
}
