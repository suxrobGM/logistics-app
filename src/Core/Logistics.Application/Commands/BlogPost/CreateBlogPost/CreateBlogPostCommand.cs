using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public sealed class CreateBlogPostCommand : IAppRequest<Result<Guid>>
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? Category { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? FeaturedImage { get; set; }
    public bool IsFeatured { get; set; }
}
