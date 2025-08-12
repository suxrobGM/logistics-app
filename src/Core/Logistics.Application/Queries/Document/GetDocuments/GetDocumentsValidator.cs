using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetDocumentsValidator : AbstractValidator<GetDocumentsQuery>
{
    public GetDocumentsValidator()
    {
        When(i => i.OwnerType.HasValue || i.OwnerId.HasValue, () =>
        {
            RuleFor(i => i.OwnerType)
                .NotNull()
                .WithMessage("Owner type is required when owner ID is provided");

            RuleFor(i => i.OwnerId)
                .NotNull()
                .WithMessage("Owner ID is required when owner type is provided");

            RuleFor(i => i.OwnerId)
                .NotEqual(Guid.Empty)
                .WithMessage("Owner ID cannot be empty");

            RuleFor(i => i.OwnerType)
                .IsInEnum()
                .WithMessage("Invalid owner type");
        });

        RuleFor(i => i.Status)
            .IsInEnum()
            .When(i => i.Status.HasValue)
            .WithMessage("Invalid document status");

        RuleFor(i => i.Type)
            .IsInEnum()
            .When(i => i.Type.HasValue)
            .WithMessage("Invalid document type");
    }
}
