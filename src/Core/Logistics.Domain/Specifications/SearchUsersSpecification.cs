namespace Logistics.Domain.Specifications;

public class SearchUsersSpecification : BaseSpecification<User>
{
    public SearchUsersSpecification(string searchInput)
        : base(i =>
            (!string.IsNullOrEmpty(i.FirstName) &&
            i.FirstName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase)) ||

            (!string.IsNullOrEmpty(i.LastName) &&
            i.LastName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase)) ||

            (!string.IsNullOrEmpty(i.UserName) &&
            i.UserName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase)) ||
            
            (!string.IsNullOrEmpty(i.Email) &&
             i.Email.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase))
        )
    {
    }
}
