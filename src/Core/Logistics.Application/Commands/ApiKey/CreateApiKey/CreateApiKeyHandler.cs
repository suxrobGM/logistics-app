using System.Security.Cryptography;
using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateApiKeyHandler(
    ITenantUnitOfWork tenantUow,
    ITenantService tenantService) : IAppRequestHandler<CreateApiKeyCommand, Result<ApiKeyCreatedDto>>
{
    public async Task<Result<ApiKeyCreatedDto>> Handle(CreateApiKeyCommand req, CancellationToken ct)
    {
        var tenant = tenantService.GetCurrentTenant();
        var rawKey = GenerateKey(tenant.Id);
        var hash = ApiKeyHasher.Hash(rawKey);
        var prefix = BuildPrefix(rawKey);

        var apiKey = new ApiKey
        {
            Name = req.Name,
            KeyHash = hash,
            KeyPrefix = prefix
        };

        await tenantUow.Repository<ApiKey>().AddAsync(apiKey, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<ApiKeyCreatedDto>.Ok(new ApiKeyCreatedDto(
            apiKey.Id,
            apiKey.Name,
            apiKey.KeyPrefix,
            rawKey));
    }

    private static string GenerateKey(Guid tenantId)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        var randomPart = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
        return $"logsx_{tenantId}_{randomPart}";
    }

    private static string BuildPrefix(string rawKey)
    {
        var first8 = rawKey[..8];
        var last4 = rawKey[^4..];
        return $"{first8}...{last4}";
    }
}
