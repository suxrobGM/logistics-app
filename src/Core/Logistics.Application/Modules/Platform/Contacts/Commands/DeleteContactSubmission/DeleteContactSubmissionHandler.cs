using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Platform.Contacts.Commands;

internal sealed class DeleteContactSubmissionHandler(IMasterUnitOfWork masterUow)
    : DeleteMasterEntityHandler<DeleteContactSubmissionCommand, ContactSubmission>(masterUow);
