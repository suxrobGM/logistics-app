using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.BlogPosts.Queries;

public sealed class GetPublishedBlogPostsQuery : SearchableQuery, IQuery<PagedResult<BlogPostDto>>
{
    public string? Category { get; set; }
}
