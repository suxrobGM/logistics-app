﻿using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public sealed class GetSubscriptionPlanQuery : IRequest<Result<SubscriptionPlanDto>>
{
    public Guid Id { get; set; }
}
