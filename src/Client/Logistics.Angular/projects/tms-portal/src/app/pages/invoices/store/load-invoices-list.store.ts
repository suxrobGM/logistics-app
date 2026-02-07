import { formatSortField, getInvoices } from "@logistics/shared/api";
import type { InvoiceDto } from "@logistics/shared/api";
import { createListStore } from "@/shared/stores";

/**
 * Store for the load invoices list page.
 * Filters by InvoiceType=load and supports status, customer, and date range filters.
 */
export const LoadInvoicesListStore = createListStore<InvoiceDto>(getInvoices, {
  defaultSortField: "-CreatedAt",
  defaultPageSize: 10,
  buildParams: (state) => {
    const orderBy = formatSortField(state.sortField, state.sortOrder) || "-CreatedAt";

    return {
      Page: state.page,
      PageSize: state.pageSize,
      Search: state.search,
      OrderBy: orderBy,
      InvoiceType: "load",
      Status: state.filters["Status"],
      CustomerName: state.filters["CustomerName"],
      StartDate: state.filters["StartDate"],
      EndDate: state.filters["EndDate"],
    };
  },
});
