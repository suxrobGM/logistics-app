using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.BlogPosts.Queries;

internal sealed class GetPublishedBlogPostsHandler(
    IMasterUnitOfWork masterUow) : IAppRequestHandler<GetPublishedBlogPostsQuery, PagedResult<BlogPostDto>>
{
    public Task<PagedResult<BlogPostDto>> Handle(GetPublishedBlogPostsQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<BlogPost>().Query()
            .Where(x => x.Status == BlogPostStatus.Published);

        if (!string.IsNullOrEmpty(req.Search))
        {
            query = query.Where(x =>
                x.Title.Contains(req.Search) ||
                (x.Excerpt != null && x.Excerpt.Contains(req.Search)));
        }

        if (!string.IsNullOrEmpty(req.Category))
        {
            query = query.Where(x => x.Category == req.Category);
        }

        var totalItems = query.Count();

        var items = query
            .OrderBy(string.IsNullOrEmpty(req.OrderBy) ? "-PublishedAt" : req.OrderBy)
            .ApplyPaging(req.Page, req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(PagedResult<BlogPostDto>.Ok(items, totalItems, req.PageSize));
    }
}
