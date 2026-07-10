import {
  formatSortField,
  getInvoices,
  type InvoiceDto,
  type InvoiceStatus,
  type SalaryType,
} from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the payroll invoices list page.
 * Supports filtering by employee, status, salary type, and date range.
 */
export const PayrollInvoicesListStore = createListStore<InvoiceDto>(getInvoices, {
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
      Status: state.filters["Status"] as InvoiceStatus | undefined,
      SalaryType: state.filters["SalaryType"] as SalaryType | undefined,
      StartDate: state.filters["StartDate"] as string | undefined,
      EndDate: state.filters["EndDate"] as string | undefined,
    };
  },
});
