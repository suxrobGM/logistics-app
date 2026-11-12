using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class TimeEntryMapper
{
    extension(TimeEntry entity)
    {
        /// <summary>For one row: reads the related names off the navigations. Lists pass them in
        /// through the overload instead, looked up for the whole page at once.</summary>
        public TimeEntryDto ToDto() => entity.ToDto(entity.Employee.GetFullName());

        public TimeEntryDto ToDto(string? employeeName)
        {
            return new TimeEntryDto
            {
                Id = entity.Id.ToString(),
                EmployeeId = entity.EmployeeId.ToString(),
                EmployeeName = employeeName,
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
