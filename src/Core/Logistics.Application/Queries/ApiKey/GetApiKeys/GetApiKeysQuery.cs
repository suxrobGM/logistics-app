using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetApiKeysQuery : IQuery<Result<List<ApiKeyDto>>>;
