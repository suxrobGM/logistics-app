using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class EmployeeEntityConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");

        // Email is unique case-insensitively through the raw-SQL index ux_employees_email_lower, created in the
        // migration because EF can't model an expression index. DriverImporter matches drivers on it.

        builder.ComplexProperty(i => i.Salary, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });

        builder.HasOne(i => i.Role)
            .WithMany(i => i.Employees)
            .HasForeignKey(i => i.RoleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
