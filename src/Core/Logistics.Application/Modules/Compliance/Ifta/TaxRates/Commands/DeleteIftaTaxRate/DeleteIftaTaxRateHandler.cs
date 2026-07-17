using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Compliance.Ifta.TaxRates.Commands;

internal sealed class DeleteIftaTaxRateHandler(IMasterUnitOfWork masterUow)
    : DeleteMasterEntityHandler<DeleteIftaTaxRateCommand, IftaTaxRate>(masterUow);
