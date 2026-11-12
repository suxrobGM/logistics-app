using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
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
        public async Task<PagedResult<TDto>> ToPagedResultAsync<TDto>(
            PagedQuery req,
            Func<T, TDto> map,
            CancellationToken ct,
            string? defaultOrderBy = null)
        {
            var totalItems = await query.CountAsync(ct);
            var page = await query
                .OrderBy(req.OrderBy, x => x.Id, defaultOrderBy)
                .ApplyPaging(req.Page, req.PageSize)
                .ToArrayAsync(ct);

            return PagedResult<TDto>.Ok(page.Select(map).ToArray(), totalItems, req.PageSize);
        }
    }
}
