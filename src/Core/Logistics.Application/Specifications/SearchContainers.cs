using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public class SearchContainers : BaseSpecification<Container>
{
    public SearchContainers(
        string? search,
        string? orderBy,
        int page,
        int pageSize,
        ContainerStatus? status = null,
        ContainerIsoType? isoType = null,
        Guid? currentTerminalId = null)
    {
        Criteria = i =>
            (string.IsNullOrEmpty(search) || i.Number.Contains(search)
                                          || (i.BookingReference != null && i.BookingReference.Contains(search))
                                          || (i.BillOfLadingNumber != null && i.BillOfLadingNumber.Contains(search))) &&
            (!status.HasValue || i.Status == status.Value) &&
            (!isoType.HasValue || i.IsoType == isoType.Value) &&
            (!currentTerminalId.HasValue || i.CurrentTerminalId == currentTerminalId.Value);

        OrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
}
