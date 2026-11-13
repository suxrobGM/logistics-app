using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Website.BlogPosts.Commands;

public sealed class UnpublishBlogPostCommand : ICommand<Result>
{
    public Guid Id { get; set; }
}
