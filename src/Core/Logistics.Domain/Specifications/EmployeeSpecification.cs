namespace Logistics.Domain.Specifications;

public class EmployeeSpecification : BaseSpecification<Employee>
{
    public EmployeeSpecification(string searchInput)
        : base(i =>
            (!string.IsNullOrEmpty(i.FirstName) &&
            i.FirstName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase)) ||

            (!string.IsNullOrEmpty(i.LastName) &&
            i.LastName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase)) ||

            (!string.IsNullOrEmpty(i.UserName) &&
            i.UserName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase))
        )
    {
    }
}
