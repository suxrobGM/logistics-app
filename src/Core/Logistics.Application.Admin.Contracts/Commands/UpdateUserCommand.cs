namespace Logistics.Application.Admin.Commands;

public class UpdateUserCommand : RequestBase<ResponseResult>
{
    public string? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}
