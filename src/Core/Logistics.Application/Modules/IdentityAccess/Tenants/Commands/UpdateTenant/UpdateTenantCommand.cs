using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

public class UpdateTenantCommand : ICommand
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? CompanyName { get; set; }
    public string? BillingEmail { get; set; }
    public string? DotNumber { get; set; }
    public string? McNumber { get; set; }
    public string? VatNumber { get; set; }
    public string? EoriNumber { get; set; }
    public string? CompanyRegistrationNumber { get; set; }
    public string? TaxResidencyCountry { get; set; }
    public string? PhoneNumber { get; set; }
    public Address? CompanyAddress { get; set; }
    public string? ConnectionString { get; set; }
    public TenantSettings? Settings { get; set; }
}
