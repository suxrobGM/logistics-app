import { computed, inject } from "@angular/core";
import { Api, getUpcomingMaintenance } from "@logistics/shared/api";
import type { MaintenanceScheduleDto } from "@logistics/shared/api";
import type { AppError } from "@logistics/shared/errors";
import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";

interface UpcomingServiceState {
  data: MaintenanceScheduleDto[];
  isLoading: boolean;
  error: AppError | null;
  daysAhead: number;
  includeOverdue: boolean;
}

const initialState: UpcomingServiceState = {
  data: [],
  isLoading: false,
  error: null,
  daysAhead: 90,
  includeOverdue: true,
};

/**
 * Store for the upcoming maintenance list page.
 * Uses a simple array-based store since the API returns an array, not paged results.
 */
export const UpcomingServiceStore = signalStore(
  withState<UpcomingServiceState>(initialState),

  withComputed((store) => ({
    isEmpty: computed(() => !store.isLoading() && store.data().length === 0 && !store.error()),
    overdueCount: computed(() => store.data().filter((m) => m.isOverdue).length),
    dueSoonCount: computed(
      () => store.data().filter((m) => !m.isOverdue && (m.daysUntilDue ?? 0) <= 30).length,
    ),
  })),

  withMethods((store, api = inject(Api)) => ({
    async load(): Promise<void> {
      patchState(store, { isLoading: true, error: null });

      try {
        const data = await api.invoke(getUpcomingMaintenance, {
          daysAhead: store.daysAhead(),
          includeOverdue: store.includeOverdue(),
        });
        patchState(store, { data: data ?? [], isLoading: false });
      } catch (error) {
        patchState(store, { isLoading: false, error: error as AppError });
      }
    },

    setFilters(daysAhead: number, includeOverdue: boolean): void {
      patchState(store, { daysAhead, includeOverdue });
      this.load();
    },

    retry(): void {
      this.load();
    },
  })),
);
