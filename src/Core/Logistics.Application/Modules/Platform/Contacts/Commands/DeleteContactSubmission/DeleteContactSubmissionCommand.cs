using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Contacts.Commands;

public sealed class DeleteContactSubmissionCommand : ICommand<Result>, IHaveId
{
    public Guid Id { get; set; }
}
