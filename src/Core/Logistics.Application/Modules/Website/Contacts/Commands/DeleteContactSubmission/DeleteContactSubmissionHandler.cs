using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Website.Contacts.Commands;

internal sealed class DeleteContactSubmissionHandler(IMasterUnitOfWork masterUow)
    : DeleteMasterEntityHandler<DeleteContactSubmissionCommand, ContactSubmission>(masterUow);
