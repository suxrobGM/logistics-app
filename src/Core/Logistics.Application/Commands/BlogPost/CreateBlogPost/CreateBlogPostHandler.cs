using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreateBlogPostHandler(
    IMasterUnitOfWork masterUow,
    ILogger<CreateBlogPostHandler> logger) : IAppRequestHandler<CreateBlogPostCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateBlogPostCommand req, CancellationToken ct)
    {
        var slug = SlugGenerator.Generate(req.Title);
        slug = await EnsureUniqueSlug(slug, ct);

        var blogPost = new BlogPost
        {
            Title = req.Title,
            Slug = slug,
            Content = req.Content,
            Excerpt = req.Excerpt,
            Category = req.Category,
            AuthorName = req.AuthorName,
            FeaturedImage = req.FeaturedImage,
            IsFeatured = req.IsFeatured,
        };

        await masterUow.Repository<BlogPost>().AddAsync(blogPost, ct);
        await masterUow.SaveChangesAsync(ct);

        logger.LogInformation("Created blog post '{Title}' with slug '{Slug}'", req.Title, slug);
        return Result<Guid>.Ok(blogPost.Id);
    }

    private async Task<string> EnsureUniqueSlug(string slug, CancellationToken ct)
    {
        var baseSlug = slug;
        var counter = 1;

        while (await SlugExists(slug, ct))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    private async Task<bool> SlugExists(string slug, CancellationToken ct)
    {
        var existing = await masterUow.Repository<BlogPost>()
            .GetAsync(x => x.Slug == slug, ct);
        return existing is not null;
    }
}
