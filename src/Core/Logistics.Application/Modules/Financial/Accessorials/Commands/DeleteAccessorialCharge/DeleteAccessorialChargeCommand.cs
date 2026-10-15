namespace Logistics.Application.Modules.Financial.Accessorials.Commands;

public class DeleteAccessorialChargeCommand : ICommand, IHaveId
{
    public Guid Id { get; set; }
}
