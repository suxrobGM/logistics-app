using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPublishedBlogPostBySlugHandler(
    IMasterUnitOfWork masterUow) : IAppRequestHandler<GetPublishedBlogPostBySlugQuery, Result<BlogPostDto>>
{
    public async Task<Result<BlogPostDto>> Handle(GetPublishedBlogPostBySlugQuery req, CancellationToken ct)
    {
        var blogPost = await masterUow.Repository<BlogPost>()
            .GetAsync(x => x.Slug == req.Slug && x.Status == BlogPostStatus.Published, ct);

        if (blogPost is null)
        {
            return Result<BlogPostDto>.Fail($"Blog post with slug '{req.Slug}' not found");
        }

        return Result<BlogPostDto>.Ok(blogPost.ToDto());
    }
}
