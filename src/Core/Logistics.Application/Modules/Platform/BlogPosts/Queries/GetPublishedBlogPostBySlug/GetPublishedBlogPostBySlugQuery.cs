using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.BlogPosts.Queries;

public sealed class GetPublishedBlogPostBySlugQuery : IQuery<Result<BlogPostDto>>
{
    public string Slug { get; set; } = string.Empty;
}
