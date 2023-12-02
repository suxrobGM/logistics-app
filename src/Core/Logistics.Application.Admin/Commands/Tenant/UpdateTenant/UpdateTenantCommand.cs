using Logistics.Domain.ValueObjects;
using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Admin.Commands;

public class UpdateTenantCommand : IRequest<ResponseResult>
{
    public string Id { get; set; } = default!;
    public string? Name { get; set; }
    public string? CompanyName { get; set; }
    public Address? CompanyAddress { get; set; }
    public string? ConnectionString { get; set; }
}
