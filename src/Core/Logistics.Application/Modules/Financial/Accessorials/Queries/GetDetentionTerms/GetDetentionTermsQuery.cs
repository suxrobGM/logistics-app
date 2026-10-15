using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Accessorials.Queries;

/// <summary>The current tenant's default detention terms, as stored (nulls included).</summary>
public class GetDetentionTermsQuery : IQuery<Result<DetentionTermsDto>>;
