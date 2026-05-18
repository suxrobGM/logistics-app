using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Tax.Commands;

public record DeleteTenantTaxRateCommand(Guid Id) : ICommand<Result>;
