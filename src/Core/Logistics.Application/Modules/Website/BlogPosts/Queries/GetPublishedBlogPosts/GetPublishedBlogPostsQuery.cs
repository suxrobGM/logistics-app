using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Website.BlogPosts.Queries;

public sealed class GetPublishedBlogPostsQuery : SearchableQuery, IQuery<PagedResult<BlogPostDto>>
{
    public string? Category { get; set; }
}
