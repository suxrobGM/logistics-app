using FluentValidation;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetEmployeeByIdValidator : AbstractValidator<GetEmployeeByIdQuery>
{
    public GetEmployeeByIdValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
    }
}
