using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.ApiKeys.Queries;

public sealed class GetApiKeysQuery : IQuery<Result<List<ApiKeyDto>>>;
