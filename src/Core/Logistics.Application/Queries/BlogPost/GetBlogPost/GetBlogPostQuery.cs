using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetBlogPostQuery : IAppRequest<Result<BlogPostDto>>
{
    public Guid Id { get; set; }
}
