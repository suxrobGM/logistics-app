using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.ApiKeys.Queries;

public sealed class GetApiKeysQuery : IQuery<Result<List<ApiKeyDto>>>;
