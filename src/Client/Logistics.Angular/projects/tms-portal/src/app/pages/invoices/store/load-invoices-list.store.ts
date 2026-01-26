import { formatSortField, getInvoices } from "@logistics/shared/api";
import type { InvoiceDto, InvoiceStatus } from "@logistics/shared/api";
import { createListStore } from "@/shared/stores";

/**
 * Store for the load invoices list page.
 * Supports filtering by LoadId, Status, CustomerId, CustomerName, date range, and overdue status.
 */
export const LoadInvoicesListStore = createListStore<InvoiceDto>(getInvoices, {
  defaultSortField: "-CreatedAt",
  defaultPageSize: 10,
  buildParams: (state) => {
    const orderBy = formatSortField(state.sortField, state.sortOrder) || "-CreatedAt";

    return {
      Page: state.page,
      PageSize: state.pageSize,
      OrderBy: orderBy,
      Search: state.search || undefined,
      InvoiceType: "load",
      LoadId: state.filters["LoadId"] as string | undefined,
      Status: state.filters["Status"] as InvoiceStatus | undefined,
      CustomerId: state.filters["CustomerId"] as string | undefined,
      CustomerName: state.filters["CustomerName"] as string | undefined,
      StartDate: state.filters["StartDate"] as string | undefined,
      EndDate: state.filters["EndDate"] as string | undefined,
      OverdueOnly: state.filters["OverdueOnly"] as boolean | undefined,
    };
  },
});
