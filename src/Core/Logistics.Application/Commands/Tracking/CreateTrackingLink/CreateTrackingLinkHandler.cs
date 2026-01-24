using System.Security.Claims;
using System.Security.Cryptography;
using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Logistics.Application.Commands;

internal sealed class CreateTrackingLinkHandler(
    ITenantUnitOfWork tenantUow,
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration)
    : IAppRequestHandler<CreateTrackingLinkCommand, Result<TrackingLinkDto>>
{
    private const int DefaultExpirationDays = 30;

    public async Task<Result<TrackingLinkDto>> Handle(CreateTrackingLinkCommand req, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Result<TrackingLinkDto>.Fail("User not authenticated.");
        }

        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);
        if (load is null)
        {
            return Result<TrackingLinkDto>.Fail("Load not found.");
        }

        var trackingLink = new TrackingLink
        {
            Token = GenerateSecureToken(),
            LoadId = req.LoadId,
            ExpiresAt = DateTime.UtcNow.AddDays(DefaultExpirationDays),
            CreatedByUserId = currentUserId.Value
        };

        await tenantUow.Repository<TrackingLink>().AddAsync(trackingLink, ct);
        await tenantUow.SaveChangesAsync(ct);

        var tenant = tenantUow.GetCurrentTenant();
        var portalBaseUrl = configuration["CustomerPortal:BaseUrl"] ?? "http://localhost:7004";

        return Result<TrackingLinkDto>.Ok(new TrackingLinkDto
        {
            Id = trackingLink.Id,
            Token = trackingLink.Token,
            Url = $"{portalBaseUrl}/track/{tenant.Id}/{trackingLink.Token}",
            LoadId = load.Id,
            LoadNumber = load.Number,
            LoadName = load.Name,
            ExpiresAt = trackingLink.ExpiresAt,
            IsActive = trackingLink.IsActive,
            IsExpired = false,
            AccessCount = 0,
            LastAccessedAt = null,
            CreatedAt = trackingLink.CreatedAt
        });
    }

    private static string GenerateSecureToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
