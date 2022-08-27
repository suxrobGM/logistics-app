namespace Logistics.Domain.Specifications;

public class SearchUsers : BaseSpecification<User>
{
    public SearchUsers(string search)
    {
     Criteria = i =>
      (!string.IsNullOrEmpty(i.FirstName) &&
       i.FirstName.Contains(search, StringComparison.InvariantCultureIgnoreCase)) ||

      (!string.IsNullOrEmpty(i.LastName) &&
       i.LastName.Contains(search, StringComparison.InvariantCultureIgnoreCase)) ||

      (!string.IsNullOrEmpty(i.UserName) &&
       i.UserName.Contains(search, StringComparison.InvariantCultureIgnoreCase)) ||

      (!string.IsNullOrEmpty(i.Email) &&
       i.Email.Contains(search, StringComparison.InvariantCultureIgnoreCase));
    }
}
