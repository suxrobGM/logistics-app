import { computed, inject } from "@angular/core";
import { Api, cancelInvitation, getInvitations, resendInvitation } from "@logistics/shared/api";
import { formatSortField } from "@logistics/shared/api";
import type { InvitationDto } from "@logistics/shared/api";
import type { AppError } from "@logistics/shared/errors";
import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";
import type { TableLazyLoadEvent } from "primeng/table";
import { ToastService } from "@/core/services";

interface PendingInvitationsState {
  data: InvitationDto[];
  totalRecords: number;
  isLoading: boolean;
  error: AppError | null;
  page: number;
  pageSize: number;
  first: number;
  sortField: string;
  sortOrder: number;
  search: string;
}

const initialState: PendingInvitationsState = {
  data: [],
  totalRecords: 0,
  isLoading: false,
  error: null,
  page: 1,
  pageSize: 10,
  first: 0,
  sortField: "CreatedDate",
  sortOrder: -1,
  search: "",
};

export const PendingInvitationsStore = signalStore(
  withState(initialState),
  withComputed((state) => ({
    isEmpty: computed(() => !state.isLoading() && state.data().length === 0),
  })),
  withMethods((store, api = inject(Api), toastService = inject(ToastService)) => {
    const fetchData = async () => {
      patchState(store, { isLoading: true, error: null });

      try {
        const result = await api.invoke(getInvitations, {
          Page: store.page(),
          PageSize: store.pageSize(),
          OrderBy: formatSortField(store.sortField(), store.sortOrder()),
          Search: store.search() || undefined,
          Status: "pending",
        });

        patchState(store, {
          data: result.items ?? [],
          totalRecords: result.pagination?.total ?? 0,
          isLoading: false,
        });
      } catch (err) {
        patchState(store, {
          error: { category: "unknown", message: "Failed to load invitations", retryable: true },
          isLoading: false,
        });
      }
    };

    return {
      fetchData,

      retry() {
        fetchData();
      },

      setSearch(search: string) {
        patchState(store, { search, page: 1, first: 0 });
        fetchData();
      },

      onLazyLoad(event: TableLazyLoadEvent) {
        const page = Math.floor((event.first ?? 0) / (event.rows ?? 10)) + 1;
        patchState(store, {
          page,
          pageSize: event.rows ?? 10,
          first: event.first ?? 0,
          sortField: (event.sortField as string) ?? "CreatedDate",
          sortOrder: event.sortOrder ?? -1,
        });
        fetchData();
      },

      async resend(id: string) {
        try {
          await api.invoke(resendInvitation, { id });
          toastService.showSuccess("Invitation resent successfully");
          fetchData();
        } catch {
          toastService.showError("Failed to resend invitation");
        }
      },

      async cancel(id: string) {
        try {
          await api.invoke(cancelInvitation, { id });
          toastService.showSuccess("Invitation cancelled");
          fetchData();
        } catch {
          toastService.showError("Failed to cancel invitation");
        }
      },
    };
  }),
);
