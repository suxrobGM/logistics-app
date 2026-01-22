using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetBlogPostHandler(
    IMasterUnitOfWork masterUow) : IAppRequestHandler<GetBlogPostQuery, Result<BlogPostDto>>
{
    public async Task<Result<BlogPostDto>> Handle(GetBlogPostQuery req, CancellationToken ct)
    {
        var blogPost = await masterUow.Repository<BlogPost>().GetByIdAsync(req.Id, ct);

        if (blogPost is null)
        {
            return Result<BlogPostDto>.Fail($"Blog post with ID '{req.Id}' not found");
        }

        return Result<BlogPostDto>.Ok(blogPost.ToDto());
    }
}
