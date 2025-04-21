using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class CreateSetupIntentCommand : IRequest<Result<SetupIntentDto>>
{
}
