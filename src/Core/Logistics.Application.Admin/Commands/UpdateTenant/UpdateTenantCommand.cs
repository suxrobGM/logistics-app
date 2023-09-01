namespace Logistics.Application.Admin.Commands;

public class UpdateTenantCommand : Request<ResponseResult>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? ConnectionString { get; set; }
}
