using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class BlogPostEntityConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("BlogPosts");

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.Excerpt).HasMaxLength(500);
        builder.Property(x => x.Category).HasMaxLength(100);
        builder.Property(x => x.AuthorName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FeaturedImage).HasMaxLength(500);

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => x.PublishedAt);
        builder.HasIndex(x => x.IsFeatured);
    }
}
