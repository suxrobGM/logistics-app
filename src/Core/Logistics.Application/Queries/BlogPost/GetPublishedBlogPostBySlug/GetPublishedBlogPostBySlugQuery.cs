using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetPublishedBlogPostBySlugQuery : IAppRequest<Result<BlogPostDto>>
{
    public string Slug { get; set; } = string.Empty;
}
