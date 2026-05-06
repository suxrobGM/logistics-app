using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public record DeleteTenantTaxRateCommand(Guid Id) : IAppRequest<Result>;
