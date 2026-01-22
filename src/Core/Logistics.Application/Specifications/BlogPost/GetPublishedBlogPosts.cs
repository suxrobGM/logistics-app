using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public sealed class GetPublishedBlogPosts : BaseSpecification<BlogPost>
{
    public GetPublishedBlogPosts(
        string? orderBy,
        int page,
        int pageSize,
        string? search = null,
        string? category = null)
    {
        Criteria = x =>
            x.Status == BlogPostStatus.Published &&
            (string.IsNullOrEmpty(search) ||
             x.Title.Contains(search) ||
             (x.Excerpt != null && x.Excerpt.Contains(search))) &&
            (string.IsNullOrEmpty(category) || x.Category == category);

        OrderBy(string.IsNullOrEmpty(orderBy) ? "-PublishedAt" : orderBy);
        ApplyPaging(page, pageSize);
    }
}
