namespace Logistics.Shared.Models;

public class PayrollDto
{
    public string Id { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PaymentDto Payment { get; set; } = default!;
    public EmployeeDto Employee { get; set; } = default!;
}
