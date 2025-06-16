using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.EF.Data.Configurations;

public class TripEntityConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.Navigation(t => t.Loads)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .Metadata.SetField("Loads");   // so `OrderBy(tl => tl.StopOrder)` in queries
    }
}