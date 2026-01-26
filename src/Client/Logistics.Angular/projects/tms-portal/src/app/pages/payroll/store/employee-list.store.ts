import { formatSortField, getInvoices } from "@logistics/shared/api";
import type { InvoiceDto } from "@logistics/shared/api";
import { createListStore } from "@/shared/stores";

/**
 * Store for the employee payroll invoices list page.
 * Requires EmployeeId to be set via setFilters before loading.
 */
export const EmployeePayrollListStore = createListStore<InvoiceDto>(getInvoices, {
  defaultSortField: "-CreatedAt",
  defaultPageSize: 10,
  buildParams: (state) => {
    const orderBy = formatSortField(state.sortField, state.sortOrder) || "-CreatedAt";

    return {
      Page: state.page,
      PageSize: state.pageSize,
      OrderBy: orderBy,
      InvoiceType: "payroll",
      EmployeeId: state.filters["EmployeeId"] as string | undefined,
    };
  },
});
