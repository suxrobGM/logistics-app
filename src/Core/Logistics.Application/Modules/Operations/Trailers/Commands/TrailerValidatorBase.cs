using FluentValidation;

namespace Logistics.Application.Modules.Operations.Trailers.Commands;

internal abstract class TrailerValidatorBase<T> : AbstractValidator<T> where T : ITrailerFields
{
    protected TrailerValidatorBase()
    {
        RuleFor(i => i.Number).NotEmpty().MaximumLength(50);
        RuleFor(i => i.Type).IsInEnum();
        RuleFor(i => i.Status).IsInEnum();
        RuleFor(i => i.Vin).MaximumLength(17);
        RuleFor(i => i.LicensePlate).MaximumLength(20);
        RuleFor(i => i.LicensePlateState).MaximumLength(10);
        RuleFor(i => i.Year).InclusiveBetween(1900, 2100).When(i => i.Year.HasValue);
        RuleFor(i => i.LengthMeters).GreaterThan(0).When(i => i.LengthMeters.HasValue);
        RuleFor(i => i.Notes).MaximumLength(2000);
    }
}
