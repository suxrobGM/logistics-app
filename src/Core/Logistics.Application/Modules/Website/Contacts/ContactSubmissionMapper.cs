using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Website.Contacts;

[Mapper]
public static partial class ContactSubmissionMapper
{
    public static partial ContactSubmissionDto ToDto(this ContactSubmission entity);
}
