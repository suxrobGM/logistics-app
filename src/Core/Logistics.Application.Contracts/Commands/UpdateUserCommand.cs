namespace Logistics.Application.Contracts.Commands;

public class UpdateUserCommand : RequestBase<DataResult>
{
    public string? Id { get; set; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
}
