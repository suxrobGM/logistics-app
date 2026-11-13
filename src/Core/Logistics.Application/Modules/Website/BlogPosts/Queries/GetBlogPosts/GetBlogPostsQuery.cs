using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Website.BlogPosts.Queries;

public sealed class GetBlogPostsQuery : SearchableQuery, IQuery<PagedResult<BlogPostDto>>
{
    public string? Category { get; set; }
    public BlogPostStatus? Status { get; set; }
}
