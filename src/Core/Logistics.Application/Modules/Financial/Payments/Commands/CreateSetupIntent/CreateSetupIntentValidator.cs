using FluentValidation;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

internal sealed class CreateSetupIntentValidator : AbstractValidator<CreateSetupIntentCommand>
{
    public CreateSetupIntentValidator()
    {

    }
}
