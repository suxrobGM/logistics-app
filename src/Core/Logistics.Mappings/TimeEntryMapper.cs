using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class TimeEntryMapper
{
    extension(TimeEntry entity)
    {
        public TimeEntryDto ToDto()
        {
            return new TimeEntryDto
            {
                Id = entity.Id.ToString(),
                EmployeeId = entity.EmployeeId.ToString(),
                EmployeeName = entity.Employee.GetFullName(),
                Date = entity.Date,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                TotalHours = entity.TotalHours,
                Type = entity.Type.ToString().ToLowerInvariant(),
                PayrollInvoiceId = entity.PayrollInvoiceId?.ToString(),
                Notes = entity.Notes,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
