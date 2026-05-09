using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class RequestDataExportValidator : AbstractValidator<RequestDataExportCommand>
{
    public RequestDataExportValidator()
    {
        // Per-user rate limit is enforced in the handler (needs DB access).
    }
}
