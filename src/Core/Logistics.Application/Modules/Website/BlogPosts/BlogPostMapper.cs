using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Website.BlogPosts;

[Mapper]
public static partial class BlogPostMapper
{
    [MapperIgnoreTarget(nameof(BlogPostDto.AuthorInitials))]
    public static partial BlogPostDto ToDto(this BlogPost entity);
}
