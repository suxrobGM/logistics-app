using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Website.BlogPosts.Queries;

public sealed class GetBlogPostQuery : IQuery<Result<BlogPostDto>>
{
    public Guid Id { get; set; }
}
