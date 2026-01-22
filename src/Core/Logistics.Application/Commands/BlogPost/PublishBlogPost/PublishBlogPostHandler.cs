using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class PublishBlogPostHandler(
    IMasterUnitOfWork masterUow,
    ILogger<PublishBlogPostHandler> logger) : IAppRequestHandler<PublishBlogPostCommand, Result>
{
    public async Task<Result> Handle(PublishBlogPostCommand req, CancellationToken ct)
    {
        var blogPost = await masterUow.Repository<BlogPost>().GetByIdAsync(req.Id, ct);

        if (blogPost is null)
        {
            return Result.Fail($"Blog post with ID '{req.Id}' not found");
        }

        blogPost.Status = BlogPostStatus.Published;
        blogPost.PublishedAt ??= DateTime.UtcNow;
        blogPost.UpdatedAt = DateTime.UtcNow;

        masterUow.Repository<BlogPost>().Update(blogPost);
        await masterUow.SaveChangesAsync(ct);

        logger.LogInformation("Published blog post {Id} at {PublishedAt}", req.Id, blogPost.PublishedAt);
        return Result.Ok();
    }
}
