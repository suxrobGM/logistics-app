using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Website.BlogPosts.Commands;

internal sealed class DeleteBlogPostHandler(IMasterUnitOfWork masterUow)
    : DeleteMasterEntityHandler<DeleteBlogPostCommand, BlogPost>(masterUow);
