using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class BlogPostMapper
{
    [MapperIgnoreSource(nameof(BlogPost.DomainEvents))]
    [MapperIgnoreTarget(nameof(BlogPostDto.AuthorInitials))]
    public static partial BlogPostDto ToDto(this BlogPost entity);
}
