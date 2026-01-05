import { formatSortField, getPayments$Json } from "@/core/api";
import type { PaymentDto } from "@/core/api/models";
import { createListStore } from "@/shared/stores";
import { PredefinedDateRanges } from "@/shared/utils";

/**
 * Store for the payments list page.
 * Filters payments to the past 90 days by default.
 */
export const PaymentsListStore = createListStore<PaymentDto>(getPayments$Json, {
  defaultSortField: "-CreatedDate",
  defaultPageSize: 10,
  buildParams: (state) => {
    const past90days = PredefinedDateRanges.getPast90Days();
    const orderBy = formatSortField(state.sortField, state.sortOrder) || "-CreatedDate";

    return {
      Page: state.page,
      PageSize: state.pageSize,
      Search: state.search || undefined,
      OrderBy: orderBy,
      StartDate: past90days.startDate.toISOString(),
      EndDate: past90days.endDate.toISOString(),
    };
  },
});
