using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Data.Configurations;

public class EmployeeEntityConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
            
        builder.ComplexProperty(i => i.Salary, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });
            
        builder.HasMany(i => i.Roles)
            .WithMany(i => i.Employees)
            .UsingEntity<EmployeeTenantRole>(
                l => l.HasOne<TenantRole>(i => i.Role).WithMany(i => i.EmployeeRoles),
                r => r.HasOne<Employee>(i => i.Employee).WithMany(i => i.EmployeeRoles),
                c => c.ToTable("EmployeeRoles"));
    }
}