namespace Logistics.Application.Modules.Operations.Trailers.Commands;

public class DeleteTrailerCommand : ICommand, IHaveId
{
    public Guid Id { get; set; }
}
