using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.BlogPosts.Queries;

internal sealed class GetPublishedBlogPostsHandler(
    IMasterUnitOfWork masterUow) : IRequestHandler<GetPublishedBlogPostsQuery, PagedResult<BlogPostDto>>
{
    public async Task<PagedResult<BlogPostDto>> Handle(GetPublishedBlogPostsQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<BlogPost>().Query()
            .Where(x => x.Status == BlogPostStatus.Published);

        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(search) ||
                (x.Excerpt != null && x.Excerpt.ToLower().Contains(search)));
        }

        if (!string.IsNullOrEmpty(req.Category))
        {
            query = query.Where(x => x.Category == req.Category);
        }

        return await query.ToPagedResultAsync(req, i => i.ToDto(), ct, defaultOrderBy: "-PublishedAt");
    }
}
