import { computed, inject } from "@angular/core";
import { Api, getConditionReports } from "@logistics/shared/api";
import type { ConditionReportDto } from "@logistics/shared/api/models";
import type { AppError } from "@logistics/shared/errors";
import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";

interface ConditionReportsState {
  data: ConditionReportDto[];
  isLoading: boolean;
  error: AppError | null;
  search: string;
  sortField: string;
  sortOrder: number;
}

const initialState: ConditionReportsState = {
  data: [],
  isLoading: false,
  error: null,
  search: "",
  sortField: "inspectedAt",
  sortOrder: -1, // descending
};

/**
 * Store for the condition reports list page.
 */
export const ConditionReportsListStore = signalStore(
  withState<ConditionReportsState>(initialState),

  withComputed((store) => ({
    isEmpty: computed(() => !store.isLoading() && store.data().length === 0 && !store.error()),
    hasError: computed(() => store.error() !== null),
    filteredData: computed(() => {
      let data = [...store.data()];
      const search = store.search().toLowerCase();

      // Apply search filter
      if (search) {
        data = data.filter(
          (r) =>
            r.vin?.toLowerCase().includes(search) ||
            r.inspectorName?.toLowerCase().includes(search) ||
            r.vehicleMake?.toLowerCase().includes(search) ||
            r.vehicleModel?.toLowerCase().includes(search)
        );
      }

      // Apply sorting
      const sortField = store.sortField() as keyof ConditionReportDto;
      const sortOrder = store.sortOrder();
      if (sortField) {
        data.sort((a, b) => {
          const aVal = a[sortField];
          const bVal = b[sortField];
          if (aVal == null) return 1;
          if (bVal == null) return -1;
          if (aVal < bVal) return -1 * sortOrder;
          if (aVal > bVal) return 1 * sortOrder;
          return 0;
        });
      }

      return data;
    }),
    totalRecords: computed(() => store.data().length),
  })),

  withMethods((store, api = inject(Api)) => ({
    async load(loadId?: string): Promise<void> {
      patchState(store, { isLoading: true, error: null });

      try {
        const result = await api.invoke(getConditionReports, loadId ? { loadId } : undefined);
        patchState(store, { data: result ?? [], isLoading: false });
      } catch (error) {
        patchState(store, { isLoading: false, error: error as AppError });
      }
    },

    setSearch(search: string): void {
      patchState(store, { search });
    },

    setSort(field: string, order: number): void {
      patchState(store, { sortField: field, sortOrder: order });
    },

    reset(): void {
      patchState(store, initialState);
    },
  }))
);
