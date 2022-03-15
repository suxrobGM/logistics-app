namespace Logistics.Domain.Specifications;

public class UsersSpecification : BaseSpecification<User>
{
    public UsersSpecification(string searchInput) 
        : base(i => SearchCriteria(searchInput, i))
    {
    }

    private static bool SearchCriteria(string searchInput, User user)
    {
        var firstName = false;
        var lastName = false;
        var userName = false;

        if (!string.IsNullOrEmpty(user.FirstName))
        {
            firstName = user.FirstName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase);
        }

        if (!string.IsNullOrEmpty(user.LastName))
        {
            lastName = user.LastName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase);
        }

        if (!string.IsNullOrEmpty(user.UserName))
        {
            userName = user.UserName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase);
        }

        return firstName || lastName || userName;
    }
}
