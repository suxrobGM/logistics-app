using System.Linq.Expressions;
using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPostedTrucksHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetPostedTrucksQuery, Result<List<PostedTruckDto>>>
{
    public async Task<Result<List<PostedTruckDto>>> Handle(GetPostedTrucksQuery req, CancellationToken ct)
    {
        // Build predicate
        Expression<Func<PostedTruck, bool>> predicate = p => true;

        if (req.ProviderType.HasValue)
        {
            var provider = req.ProviderType.Value;
            predicate = CombinePredicates(predicate, p => p.ProviderType == provider);
        }

        if (req.Status.HasValue)
        {
            var status = req.Status.Value;
            predicate = CombinePredicates(predicate, p => p.Status == status);
        }

        if (req.TruckId.HasValue)
        {
            var truckId = req.TruckId.Value;
            predicate = CombinePredicates(predicate, p => p.TruckId == truckId);
        }

        var postedTrucks = await tenantUow.Repository<PostedTruck>().GetListAsync(predicate, ct);

        // Order by CreatedAt descending in memory
        var dtos = postedTrucks
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostedTruckDto
            {
                Id = p.Id,
                TruckId = p.TruckId,
                TruckNumber = p.Truck.Number,
                ProviderType = p.ProviderType,
                ProviderName = p.ProviderType.ToString(),
                ExternalPostId = p.ExternalPostId,
                AvailableAtAddress = p.AvailableAtAddress,
                AvailableAtLocation = p.AvailableAtLocation,
                DestinationPreference = p.DestinationPreference,
                DestinationRadius = p.DestinationRadius,
                AvailableFrom = p.AvailableFrom,
                AvailableTo = p.AvailableTo,
                EquipmentType = p.EquipmentType,
                MaxWeight = p.MaxWeight,
                MaxLength = p.MaxLength,
                Status = p.Status,
                ExpiresAt = p.ExpiresAt,
                LastRefreshedAt = p.LastRefreshedAt,
                CreatedAt = p.CreatedAt
            }).ToList();

        return Result<List<PostedTruckDto>>.Ok(dtos);
    }

    private static Expression<Func<PostedTruck, bool>> CombinePredicates(
        Expression<Func<PostedTruck, bool>> left,
        Expression<Func<PostedTruck, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(PostedTruck), "p");
        var leftBody = ReplaceParameter(left.Body, left.Parameters[0], parameter);
        var rightBody = ReplaceParameter(right.Body, right.Parameters[0], parameter);
        var combined = Expression.AndAlso(leftBody, rightBody);
        return Expression.Lambda<Func<PostedTruck, bool>>(combined, parameter);
    }

    private static Expression ReplaceParameter(Expression body, ParameterExpression from, ParameterExpression to)
    {
        return new ParameterReplacer(from, to).Visit(body);
    }

    private sealed class ParameterReplacer(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == from ? to : base.VisitParameter(node);
    }
}
