using Logistics.Domain.Core;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Common.Extensions;

public static class PagedQueryableExtensions
{
    extension<T>(IQueryable<T> query) where T : IEntity<Guid>
    {
        /// <summary>Filter before calling. Sorting ends on the id so pages never repeat or skip rows.</summary>
        public Task<PagedResult<TDto>> ToPagedResultAsync<TDto>(
            PagedQuery req,
            Func<T, TDto> map,
            CancellationToken ct,
            string? defaultOrderBy = null) =>
            query.ToPagedResultAsync(req, (page, _) => Task.FromResult(page.Select(map)), ct, defaultOrderBy);

        /// <summary>Maps the whole page at once, so related rows can be looked up in one query.</summary>
        public async Task<PagedResult<TDto>> ToPagedResultAsync<TDto>(
            PagedQuery req,
            Func<IReadOnlyList<T>, CancellationToken, Task<IEnumerable<TDto>>> mapPage,
            CancellationToken ct,
            string? defaultOrderBy = null)
        {
            var totalItems = await query.CountAsync(ct);
            var page = await query
                .OrderBy(req.OrderBy, x => x.Id, defaultOrderBy)
                .ApplyPaging(req.Page, req.PageSize)
                .ToArrayAsync(ct);

            var items = await mapPage(page, ct);
            return PagedResult<TDto>.Ok(items.ToArray(), totalItems, req.PageSize);
        }
    }
}
