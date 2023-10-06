using CommunityToolkit.Mvvm.Messaging.Messages;
using Logistics.DriverApp.Models;

namespace Logistics.DriverApp.Messages;

public class ActiveLoadsRequestMessage : RequestMessage<IReadOnlyCollection<ActiveLoad>>
{
    
}
