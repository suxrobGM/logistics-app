using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetUnreadCountQuery : IAppRequest<Result<int>>
{
    public Guid EmployeeId { get; set; }
}
