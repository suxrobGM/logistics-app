namespace Logistics.Domain.Specifications;

public class SearchUsers : BaseSpecification<User>
{
    public SearchUsers(string? search)
    {
        if (string.IsNullOrEmpty(search))
            return;

        Criteria = i =>
            (!string.IsNullOrEmpty(i.FirstName) &&
             i.FirstName.Contains(search)) ||
            (!string.IsNullOrEmpty(i.LastName) &&
             i.LastName.Contains(search)) ||
            (!string.IsNullOrEmpty(i.UserName) &&
             i.UserName.Contains(search)) ||
            (!string.IsNullOrEmpty(i.Email) &&
             i.Email.Contains(search));
    }
}