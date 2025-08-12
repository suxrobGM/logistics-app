using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Queries;

public class GetDocumentsQuery : IRequest<Result<IEnumerable<DocumentDto>>>
{
    /// <summary>
    ///     Filter by document owner type and ID.
    ///     If you specify the OwnerType, you must also specify the OwnerId.
    /// </summary>
    public DocumentOwnerType? OwnerType { get; set; } // Load | Employee

    /// <summary>
    ///     The ID of the document owner (LoadId or EmployeeId).
    /// </summary>
    public Guid? OwnerId { get; set; } // LoadId or EmployeeId

    public DocumentStatus? Status { get; set; }
    public DocumentType? Type { get; set; }
}
