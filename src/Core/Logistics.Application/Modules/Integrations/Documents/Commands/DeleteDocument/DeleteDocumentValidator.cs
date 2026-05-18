using FluentValidation;

namespace Logistics.Application.Modules.Integrations.Documents.Commands;

internal sealed class DeleteDocumentValidator : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty()
            .WithMessage("Document ID is required");
    }
}
