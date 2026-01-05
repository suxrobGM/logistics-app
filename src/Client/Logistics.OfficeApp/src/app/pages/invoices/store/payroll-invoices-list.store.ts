import { formatSortField, getInvoices$Json } from "@/core/api";
import type { InvoiceDto } from "@/core/api/models";
import { createListStore } from "@/shared/stores";

/**
 * Store for the payroll invoices list page.
 * Filters to only show payroll invoices.
 */
export const PayrollInvoicesListStore = createListStore<InvoiceDto>(getInvoices$Json, {
  defaultSortField: "-CreatedAt",
  defaultPageSize: 10,
  buildParams: (state) => {
    const orderBy = formatSortField(state.sortField, state.sortOrder) || "-CreatedAt";

    return {
      Page: state.page,
      PageSize: state.pageSize,
      EmployeeName: state.search || undefined,
      OrderBy: orderBy,
      InvoiceType: "payroll",
    };
  },
});
