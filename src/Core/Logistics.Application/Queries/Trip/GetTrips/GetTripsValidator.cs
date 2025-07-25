﻿using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetTripsValidator : AbstractValidator<GetTenantsQuery>
{
    public GetTripsValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);
        
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
