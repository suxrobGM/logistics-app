﻿using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdateTenantCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? CompanyName { get; set; }
    public string? BillingEmail { get; set; }
    public string? DotNumber { get; set; }
    public Address? CompanyAddress { get; set; }
    public string? ConnectionString { get; set; }
}
