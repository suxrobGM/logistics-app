using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.AiSettings.Queries;

/// <summary>
/// Returns the platform-wide AI dispatch settings (global model + per-plan quotas) for the admin portal.
/// </summary>
public sealed class GetAiSettingsQuery : IQuery<Result<AiSettingsDto>>;
