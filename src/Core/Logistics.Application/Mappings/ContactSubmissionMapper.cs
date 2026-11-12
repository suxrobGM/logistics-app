using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class ContactSubmissionMapper
{
    public static partial ContactSubmissionDto ToDto(this ContactSubmission entity);
}
