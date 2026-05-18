using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

public class CreateSetupIntentCommand : ICommand<Result<SetupIntentDto>>
{
}
