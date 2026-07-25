using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.AISettings.Queries;

/// <summary>
/// Returns the platform-wide AI dispatch settings (global model + per-plan quotas) for the admin portal.
/// </summary>
public sealed class GetAISettingsQuery : IQuery<Result<AISettingsDto>>;
