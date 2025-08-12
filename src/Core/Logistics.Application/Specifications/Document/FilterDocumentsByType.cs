using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public class FilterDocumentsByType : BaseSpecification<LoadDocument>
{
    public FilterDocumentsByType(DocumentType? type, DocumentStatus? status)
    {
        if (type is not null)
        {
            Criteria = d => d.Type == type;
        }

        if (status is not null)
        {
            Criteria = Criteria.AndAlso(d => d.Status == status);
        }

        OrderBy($"-{nameof(LoadDocument.CreatedAt)}");
    }
}
