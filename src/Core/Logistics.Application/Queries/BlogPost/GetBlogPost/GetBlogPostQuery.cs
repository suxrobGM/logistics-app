using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetBlogPostQuery : IQuery<Result<BlogPostDto>>
{
    public Guid Id { get; set; }
}
