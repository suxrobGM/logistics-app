using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.BlogPosts.Queries;

internal sealed class GetBlogPostsHandler(
    IMasterUnitOfWork masterUow) : IRequestHandler<GetBlogPostsQuery, PagedResult<BlogPostDto>>
{
    public Task<PagedResult<BlogPostDto>> Handle(GetBlogPostsQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<BlogPost>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(search) ||
                x.Content.ToLower().Contains(search) ||
                (x.Excerpt != null && x.Excerpt.ToLower().Contains(search)));
        }

        if (!string.IsNullOrEmpty(req.Category))
        {
            query = query.Where(x => x.Category == req.Category);
        }

        if (req.Status.HasValue)
        {
            query = query.Where(x => x.Status == req.Status.Value);
        }

        var totalItems = query.Count();

        var items = query
            .OrderBy(req.OrderBy)
            .ApplyPaging(req.Page, req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(PagedResult<BlogPostDto>.Ok(items, totalItems, req.PageSize));
    }
}
