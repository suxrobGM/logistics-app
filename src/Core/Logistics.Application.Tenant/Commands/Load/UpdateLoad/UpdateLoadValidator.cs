﻿using FluentValidation;
using Logistics.Domain.Constraints;

namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateLoadValidator : AbstractValidator<UpdateLoadCommand>
{
    public UpdateLoadValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
        RuleFor(i => i.Distance).GreaterThan(0);
        RuleFor(i => i.DeliveryCost)
            .GreaterThan(LoadConsts.MinDeliveryCost)
            .LessThan(LoadConsts.MaxDeliveryCost);
        
        When(i => !string.IsNullOrEmpty(i.OriginAddress), () =>
        {
            RuleFor(i => i.OriginAddressLat).NotEmpty().InclusiveBetween(-90, 90);
            RuleFor(i => i.OriginAddressLong).NotEmpty().InclusiveBetween(-180, 180);
        });
        
        When(i => !string.IsNullOrEmpty(i.DestinationAddress), () =>
        {
            RuleFor(i => i.DestinationAddressLat).NotEmpty().InclusiveBetween(-90, 90);
            RuleFor(i => i.DestinationAddressLong).NotEmpty().InclusiveBetween(-180, 180);
        });
    }
}