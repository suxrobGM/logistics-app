using System.Text.Json.Serialization;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class UpdateNotificationCommand : IRequest<ResponseResult>
{
    [JsonIgnore] 
    public string Id { get; set; } = default!;
    public bool? IsRead { get; set; }
}
