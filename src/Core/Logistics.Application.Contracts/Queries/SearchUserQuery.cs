namespace Logistics.Application.Contracts.Queries;

public class SearchUserQuery
{
    public string? ExternalId { get; set; }
    public string? UserName { get; set; }
}
