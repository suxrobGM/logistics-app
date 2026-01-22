using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetBlogPostsHandler(
    IMasterUnitOfWork masterUow) : IAppRequestHandler<GetBlogPostsQuery, PagedResult<BlogPostDto>>
{
    public async Task<PagedResult<BlogPostDto>> Handle(GetBlogPostsQuery req, CancellationToken ct)
    {
        var totalItems = await masterUow.Repository<BlogPost>().CountAsync(null, ct);
        var spec = new GetBlogPosts(req.OrderBy, req.Page, req.PageSize, req.Search, req.Category, req.Status);

        var items = masterUow.Repository<BlogPost>()
            .ApplySpecification(spec)
            .Select(i => i.ToDto())
            .ToArray();

        return PagedResult<BlogPostDto>.Succeed(items, totalItems, req.PageSize);
    }
}
