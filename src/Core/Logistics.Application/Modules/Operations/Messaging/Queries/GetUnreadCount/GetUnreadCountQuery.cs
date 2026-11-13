using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Messaging.Queries;

public class GetUnreadCountQuery : IQuery<Result<int>>
{
    public Guid EmployeeId { get; set; }
}
