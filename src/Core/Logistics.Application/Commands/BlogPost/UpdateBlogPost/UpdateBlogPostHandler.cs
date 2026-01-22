using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class UpdateBlogPostHandler(
    IMasterUnitOfWork masterUow,
    ILogger<UpdateBlogPostHandler> logger) : IAppRequestHandler<UpdateBlogPostCommand, Result>
{
    public async Task<Result> Handle(UpdateBlogPostCommand req, CancellationToken ct)
    {
        var blogPost = await masterUow.Repository<BlogPost>().GetByIdAsync(req.Id, ct);

        if (blogPost is null)
        {
            return Result.Fail($"Blog post with ID '{req.Id}' not found");
        }

        // Regenerate slug if title changed
        if (blogPost.Title != req.Title)
        {
            var newSlug = SlugGenerator.Generate(req.Title);
            newSlug = await EnsureUniqueSlug(newSlug, req.Id, ct);
            blogPost.Slug = newSlug;
        }

        blogPost.Title = req.Title;
        blogPost.Content = req.Content;
        blogPost.Excerpt = req.Excerpt;
        blogPost.Category = req.Category;
        blogPost.AuthorName = req.AuthorName;
        blogPost.FeaturedImage = req.FeaturedImage;
        blogPost.IsFeatured = req.IsFeatured;
        blogPost.UpdatedAt = DateTime.UtcNow;

        masterUow.Repository<BlogPost>().Update(blogPost);
        await masterUow.SaveChangesAsync(ct);

        logger.LogInformation("Updated blog post {Id} with title '{Title}'", req.Id, req.Title);
        return Result.Ok();
    }

    private async Task<string> EnsureUniqueSlug(string slug, Guid excludeId, CancellationToken ct)
    {
        var baseSlug = slug;
        var counter = 1;

        while (await SlugExists(slug, excludeId, ct))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    private async Task<bool> SlugExists(string slug, Guid excludeId, CancellationToken ct)
    {
        var existing = await masterUow.Repository<BlogPost>()
            .GetAsync(x => x.Slug == slug && x.Id != excludeId, ct);
        return existing is not null;
    }
}
