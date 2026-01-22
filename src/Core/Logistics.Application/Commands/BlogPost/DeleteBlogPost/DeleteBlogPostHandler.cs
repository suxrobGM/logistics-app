using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class DeleteBlogPostHandler(
    IMasterUnitOfWork masterUow,
    ILogger<DeleteBlogPostHandler> logger) : IAppRequestHandler<DeleteBlogPostCommand, Result>
{
    public async Task<Result> Handle(DeleteBlogPostCommand req, CancellationToken ct)
    {
        var blogPost = await masterUow.Repository<BlogPost>().GetByIdAsync(req.Id, ct);

        if (blogPost is null)
        {
            return Result.Fail($"Blog post with ID '{req.Id}' not found");
        }

        masterUow.Repository<BlogPost>().Delete(blogPost);
        await masterUow.SaveChangesAsync(ct);

        logger.LogInformation("Deleted blog post {Id}", req.Id);
        return Result.Ok();
    }
}
