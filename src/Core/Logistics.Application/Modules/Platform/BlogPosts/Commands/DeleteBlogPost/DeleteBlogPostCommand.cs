using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.BlogPosts.Commands;

public sealed class DeleteBlogPostCommand : ICommand<Result>
{
    public Guid Id { get; set; }
}
