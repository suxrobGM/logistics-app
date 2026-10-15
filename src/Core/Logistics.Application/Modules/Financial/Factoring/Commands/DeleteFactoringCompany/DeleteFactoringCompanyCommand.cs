using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

public class DeleteFactoringCompanyCommand : ICommand<Result>, IHaveId
{
    public Guid Id { get; set; }
}
