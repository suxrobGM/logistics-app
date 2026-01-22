using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPublishedBlogPostsHandler(
    IMasterUnitOfWork masterUow) : IAppRequestHandler<GetPublishedBlogPostsQuery, PagedResult<BlogPostDto>>
{
    public async Task<PagedResult<BlogPostDto>> Handle(GetPublishedBlogPostsQuery req, CancellationToken ct)
    {
        var totalItems = await masterUow.Repository<BlogPost>()
            .CountAsync(x => x.Status == BlogPostStatus.Published, ct);

        var spec = new GetPublishedBlogPosts(req.OrderBy, req.Page, req.PageSize, req.Search, req.Category);

        var items = masterUow.Repository<BlogPost>()
            .ApplySpecification(spec)
            .Select(i => i.ToDto())
            .ToArray();

        return PagedResult<BlogPostDto>.Succeed(items, totalItems, req.PageSize);
    }
}
