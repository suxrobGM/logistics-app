using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public class EmployeeDto
{
    public Guid? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FullName { get; set; }
    public string? TruckNumber { get; set; }
    public Guid? TruckId { get; set; }
    public string? LastKnownLocation { get; set; }
    public double? LastKnownLocationLat { get; set; }
    public double? LastKnownLocationLng { get; set; }
    public decimal Salary { get; set; }
    public SalaryType SalaryType { get; set; }
    public DateTime JoinedDate { get; set; }
    public IEnumerable<RoleDto> Roles { get; set; } = [];
}
