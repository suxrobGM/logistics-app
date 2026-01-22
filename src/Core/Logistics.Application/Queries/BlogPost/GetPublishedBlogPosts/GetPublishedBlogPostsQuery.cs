using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetPublishedBlogPostsQuery : SearchableQuery, IAppRequest<PagedResult<BlogPostDto>>
{
    public string? Category { get; set; }
}
