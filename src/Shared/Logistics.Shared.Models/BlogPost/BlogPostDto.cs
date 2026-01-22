using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record BlogPostDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? Excerpt { get; init; }
    public string? Category { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string? FeaturedImage { get; init; }
    public bool IsFeatured { get; init; }
    public BlogPostStatus Status { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// Gets the author's initials derived from AuthorName.
    /// </summary>
    public string AuthorInitials => GetInitials(AuthorName);

    private static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => string.Empty,
            1 => parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant(),
            _ => $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}"
        };
    }
}
