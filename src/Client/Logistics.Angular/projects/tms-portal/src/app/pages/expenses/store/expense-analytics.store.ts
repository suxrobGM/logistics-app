import { computed, inject } from "@angular/core";
import { Api, getExpenseStats, type ExpenseStatsDto } from "@logistics/shared/api";
import { downloadBlobFile } from "@logistics/shared/utils";
import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";
import {
  buildAnalyticsCsvRows,
  buildCompanyCategoryChart,
  buildMonthlyTrendChart,
  buildTruckCategoryChart,
  buildTypeChart,
} from "../_components/expense-analytics.utils";

interface ExpenseAnalyticsState {
  fromDate: Date | null;
  toDate: Date | null;
  stats: ExpenseStatsDto | null;
  isLoading: boolean;
}

function defaultRange(): { fromDate: Date; toDate: Date } {
  const toDate = new Date();
  const fromDate = new Date();
  fromDate.setFullYear(toDate.getFullYear() - 1);
  return { fromDate, toDate };
}

const initialState: ExpenseAnalyticsState = {
  ...defaultRange(),
  stats: null,
  isLoading: false,
};

export const ExpenseAnalyticsStore = signalStore(
  withState(initialState),
  withComputed((store) => ({
    typeChartData: computed(() => buildTypeChart(store.stats())),
    companyCategoryChartData: computed(() => buildCompanyCategoryChart(store.stats())),
    truckCategoryChartData: computed(() => buildTruckCategoryChart(store.stats())),
    monthlyTrendChartData: computed(() => buildMonthlyTrendChart(store.stats())),
  })),
  withMethods((store, api = inject(Api)) => ({
    setFromDate(fromDate: Date | null) {
      patchState(store, { fromDate });
    },
    setToDate(toDate: Date | null) {
      patchState(store, { toDate });
    },
    async load() {
      patchState(store, { isLoading: true });
      const stats = await api.invoke(getExpenseStats, {
        FromDate: store.fromDate()?.toISOString(),
        ToDate: store.toDate()?.toISOString(),
      });
      patchState(store, { stats: stats ?? null, isLoading: false });
    },
    exportCsv() {
      const stats = store.stats();
      if (!stats) return;

      const rows = buildAnalyticsCsvRows(stats);
      const csvContent = rows.map((row) => row.join(",")).join("\n");
      const blob = new Blob([csvContent], { type: "text/csv" });
      downloadBlobFile(blob, `expense-analytics-${new Date().toISOString().split("T")[0]}.csv`);
    },
  })),
);
