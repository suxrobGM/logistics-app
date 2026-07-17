namespace Logistics.Application.Modules.Platform.BlogPosts.Commands;

/// <summary>
/// The blog-post fields shared by the create and update commands, so both inherit one set of
/// validation rules.
/// </summary>
public interface IBlogPostFields
{
    string Title { get; }
    string Content { get; }
    string? Excerpt { get; }
    string? Category { get; }
    string AuthorName { get; }
    string? FeaturedImage { get; }
}
