using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class DeleteTripValidator : AbstractValidator<DeleteTripCommand>
{
    public DeleteTripValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
