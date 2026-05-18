using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Privacy.Commands;

internal sealed class RecordConsentHandler(IMasterUnitOfWork masterUow)
    : IAppRequestHandler<RecordConsentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RecordConsentCommand req, CancellationToken ct)
    {
        var record = new ConsentRecord
        {
            UserId = req.UserId,
            ConsentType = req.ConsentType,
            Granted = req.Granted,
            Timestamp = DateTime.UtcNow,
            IpAddress = req.IpAddress,
            UserAgent = req.UserAgent
        };

        await masterUow.Repository<ConsentRecord>().AddAsync(record, ct);
        await masterUow.SaveChangesAsync(ct);

        return Result<Guid>.Ok(record.Id);
    }
}
