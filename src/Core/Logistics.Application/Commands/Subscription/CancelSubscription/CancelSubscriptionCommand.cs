﻿using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class CancelSubscriptionCommand : IRequest<Result>
{
    public string Id { get; set; } = null!;
}
