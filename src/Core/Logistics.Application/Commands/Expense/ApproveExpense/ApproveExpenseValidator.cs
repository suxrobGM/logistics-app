using FluentValidation;

namespace Logistics.Application.Commands;

public class ApproveExpenseValidator : AbstractValidator<ApproveExpenseCommand>
{
    public ApproveExpenseValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ApproverId).NotEmpty();
    }
}
