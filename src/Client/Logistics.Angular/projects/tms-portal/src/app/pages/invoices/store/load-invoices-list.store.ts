import { createListStore } from "@logistics/shared/stores";
import { formatSortField, getInvoices } from "@/core/api";
import type { InvoiceDto } from "@/core/api/models";

/**
 * Store for the load invoices list page.
 * Requires LoadId to be set via setFilters before loading.
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
      LoadId: state.filters["LoadId"] as string | undefined,
      InvoiceType: "load",
    };
  },
});
