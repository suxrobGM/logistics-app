using Logistics.Application.Abstractions.ProductLicense;
using Logistics.Domain.Options;
using Logistics.Infrastructure.Integrations.Common;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Licensing;

internal sealed class ProductLicenseHeartbeatSender(
    HttpClient client,
    IOptions<ProductLicenseOptions> options,
    ILogger<ProductLicenseHeartbeatSender> logger) : IProductLicenseHeartbeatSender
{
    public Task<bool> SendAsync(ProductLicenseHeartbeatDto heartbeat, CancellationToken ct = default) =>
        client.TryPostAsync(options.Value.HeartbeatUrl, heartbeat, logger, "License heartbeat", ct);
}
