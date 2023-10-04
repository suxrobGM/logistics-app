using CommunityToolkit.Mvvm.Messaging.Messages;
using Logistics.Models;

namespace Logistics.DriverApp.Messages;

public class ActiveLoadsChangedMessage : ValueChangedMessage<LoadDto[]>
{
    public ActiveLoadsChangedMessage(LoadDto[] value) : base(value)
    {
    }
}
