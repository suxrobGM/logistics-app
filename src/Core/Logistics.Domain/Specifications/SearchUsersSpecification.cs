namespace Logistics.Domain.Specifications;

public class SearchUsersSpecification : BaseSpecification<User>
{
    public SearchUsersSpecification(string search)
        : base(i =>
            (!string.IsNullOrEmpty(i.FirstName) &&
            i.FirstName.Contains(search, StringComparison.InvariantCultureIgnoreCase)) ||

            (!string.IsNullOrEmpty(i.LastName) &&
            i.LastName.Contains(search, StringComparison.InvariantCultureIgnoreCase)) ||

            (!string.IsNullOrEmpty(i.UserName) &&
            i.UserName.Contains(search, StringComparison.InvariantCultureIgnoreCase)) ||
            
            (!string.IsNullOrEmpty(i.Email) &&
             i.Email.Contains(search, StringComparison.InvariantCultureIgnoreCase))
        )
    {
    }
}
