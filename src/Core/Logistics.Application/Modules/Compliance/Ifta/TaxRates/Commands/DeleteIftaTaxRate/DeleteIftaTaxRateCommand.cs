using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Ifta.TaxRates.Commands;

public sealed class DeleteIftaTaxRateCommand : ICommand<Result>, IHaveId
{
    public Guid Id { get; set; }
}
