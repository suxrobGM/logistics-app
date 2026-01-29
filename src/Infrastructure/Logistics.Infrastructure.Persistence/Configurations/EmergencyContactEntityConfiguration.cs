using Logistics.Domain.Entities.Safety;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class EmergencyContactEntityConfiguration : IEntityTypeConfiguration<EmergencyContact>
{
    public void Configure(EntityTypeBuilder<EmergencyContact> builder)
    {
        builder.ToTable("EmergencyContacts");

        builder.HasIndex(i => i.ContactType);
        builder.HasIndex(i => i.Priority);
        builder.HasIndex(i => i.EmployeeId);

        builder.HasOne(i => i.Employee)
            .WithMany()
            .HasForeignKey(i => i.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.Name).HasMaxLength(200).IsRequired();
        builder.Property(i => i.PhoneNumber).HasMaxLength(50).IsRequired();
        builder.Property(i => i.Email).HasMaxLength(200);
    }
}
