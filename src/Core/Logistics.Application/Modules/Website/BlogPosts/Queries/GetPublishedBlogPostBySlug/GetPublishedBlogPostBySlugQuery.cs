using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Website.BlogPosts.Queries;

public sealed class GetPublishedBlogPostBySlugQuery : IQuery<Result<BlogPostDto>>
{
    public string Slug { get; set; } = string.Empty;
}
