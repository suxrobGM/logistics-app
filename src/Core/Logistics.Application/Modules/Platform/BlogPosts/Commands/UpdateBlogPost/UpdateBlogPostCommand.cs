using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.BlogPosts.Commands;

public sealed class UpdateBlogPostCommand : ICommand<Result>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? Category { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? FeaturedImage { get; set; }
    public bool IsFeatured { get; set; }
}
