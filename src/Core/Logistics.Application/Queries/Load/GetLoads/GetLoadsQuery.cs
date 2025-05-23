﻿using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetLoadsQuery : SearchableQuery, IRequest<PagedResult<LoadDto>>
{
    public bool LoadAllPages { get; set; }
    public bool OnlyActiveLoads { get; set; }
    public Guid? UserId { get; set; }
    public Guid? TruckId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
