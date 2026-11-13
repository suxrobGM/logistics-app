using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Website.Contacts.Commands;

public sealed class DeleteContactSubmissionCommand : ICommand<Result>, IHaveId
{
    public Guid Id { get; set; }
}
