using Logistics.Application.Modules.Platform.DataImport.Services;
using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Platform.DataImport.Importers;

/// <summary>
///     Finds the employee a "driver" cell names, by email or by full name ("Jane Doe" or "Doe, Jane").
///     A name two employees share matches nobody, so a row is never pinned on the wrong person.
/// </summary>
internal sealed class EmployeeMatcher
{
    private readonly Dictionary<string, Employee?> _byKey = new(StringComparer.OrdinalIgnoreCase);

    public EmployeeMatcher(IEnumerable<Employee> employees)
    {
        foreach (var employee in employees)
        {
            Add(employee.Email, employee);
            Add(ImportHeaderMapper.Normalize(employee.FirstName + employee.LastName), employee);
        }
    }

    public Employee? Find(string value)
    {
        if (value.Contains('@'))
        {
            return _byKey.GetValueOrDefault(value.Trim());
        }

        var parts = value.Split(',', 2, StringSplitOptions.TrimEntries);
        var name = parts.Length == 2 ? parts[1] + parts[0] : value;
        return _byKey.GetValueOrDefault(ImportHeaderMapper.Normalize(name));
    }

    private void Add(string key, Employee employee)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        _byKey[key] = _byKey.TryGetValue(key, out var existing) && existing?.Id != employee.Id ? null : employee;
    }
}
