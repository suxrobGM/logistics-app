using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.BlogPosts.Commands;

public sealed class DeleteBlogPostCommand : ICommand<Result>, IHaveId
{
    public Guid Id { get; set; }
}
