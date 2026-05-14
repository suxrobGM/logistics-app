using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public sealed class PublishBlogPostCommand : ICommand<Result>
{
    public Guid Id { get; set; }
}
