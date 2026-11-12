using Logistics.Domain.Core;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Utilities;

public static class PagedQueryableExtensions
{
    extension<T>(IQueryable<T> query) where T : IEntity<Guid>
    {
        /// <summary>
        ///     Counts, sorts by <see cref="PagedQuery.OrderBy" /> with the id as the final key (so pages
        ///     never repeat or skip rows), pages and maps in one go. Filter before calling.
        /// </summary>
        public Task<PagedResult<TDto>> ToPagedResultAsync<TDto>(
            PagedQuery req,
            Func<T, TDto> map,
            CancellationToken ct,
            string? defaultOrderBy = null) =>
            query.ToPagedResultAsync(req, (page, _) => Task.FromResult(page.Select(map)), ct, defaultOrderBy);

        /// <summary>
        ///     Like the per-row overload, but maps the whole page at once - for lists that look up related
        ///     names or children for every row in one query instead of lazy-loading them row by row.
        /// </summary>
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
