using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public sealed class GetBlogPosts : BaseSpecification<BlogPost>
{
    public GetBlogPosts(
        string? orderBy,
        int page,
        int pageSize,
        string? search = null,
        string? category = null,
        BlogPostStatus? status = null)
    {
        Criteria = x =>
            (string.IsNullOrEmpty(search) ||
             x.Title.Contains(search) ||
             x.Content.Contains(search) ||
             (x.Excerpt != null && x.Excerpt.Contains(search))) &&
            (string.IsNullOrEmpty(category) || x.Category == category) &&
            (!status.HasValue || x.Status == status.Value);

        OrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
}
