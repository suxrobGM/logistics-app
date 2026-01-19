using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class DeleteExpenseCommand : IAppRequest<Result>
{
    public Guid Id { get; set; }
}
