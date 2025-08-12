using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class DeleteLoadDocumentValidator : AbstractValidator<DeleteLoadDocumentCommand>
{
    public DeleteLoadDocumentValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty()
            .WithMessage("Document ID is required");

        RuleFor(x => x.RequestedById)
            .NotEmpty()
            .WithMessage("Requester ID is required");
    }
}
