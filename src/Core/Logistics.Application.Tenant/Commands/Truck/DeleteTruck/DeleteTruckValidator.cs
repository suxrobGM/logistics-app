using FluentValidation;

namespace Logistics.Application.Tenant.Commands;

internal sealed class DeleteTruckValidator : AbstractValidator<DeleteTruckCommand>
{
    public DeleteTruckValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
