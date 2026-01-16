import { computed, inject } from "@angular/core";
import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";
import { rxMethod } from "@ngrx/signals/rxjs-interop";
import type { TableLazyLoadEvent } from "primeng/table";
import { catchError, from, of, pipe, switchMap, tap } from "rxjs";
import { Api, formatSortField } from "@/core/api";
import type { ApiFnOptional, ApiFnRequired } from "@/core/api/generated/api";
import type { PagedResponse } from "@/core/api/models";
import type { AppError } from "@/core/errors";
import type { ListQueryParams, ListState, ListStoreConfig } from "./base-list.types";

/**
 * Creates the initial state for a list store.
 */
function getInitialState<T>(config?: { defaultPageSize?: number }): ListState<T> {
  return {
    data: [],
    isLoading: false,
    error: null,
    totalRecords: 0,
    totalPages: 0,
    page: 1,
    pageSize: config?.defaultPageSize ?? 10,
    first: 0,
    search: "",
    sortField: "",
    sortOrder: -1,
    filters: {},
  };
}

/**
 * Creates a reusable list store with pagination, sorting, filtering, and error handling.
 *
 * @example
 * ```typescript
 * export const CustomersListStore = createListStore<CustomerDto, GetCustomers$Params>({
 *   apiFn: getCustomers,
 *   defaultSortField: 'Name',
 *   defaultPageSize: 10,
 * });
 * ```
 */
export function createListStore<T, P extends ListQueryParams = ListQueryParams>(
  apiFn: ApiFnRequired<P, PagedResponse<T>> | ApiFnOptional<P, PagedResponse<T>>,
  config?: ListStoreConfig<T, P>,
) {
  const initialState = getInitialState<T>({ defaultPageSize: config?.defaultPageSize });

  return signalStore(
    withState<ListState<T>>(initialState),

    withComputed((store) => ({
      /** Whether the data set is empty (not loading and no items) */
      isEmpty: computed(() => !store.isLoading() && store.data().length === 0 && !store.error()),
      /** Whether there's an error */
      hasError: computed(() => store.error() !== null),
      /** Whether the error is retryable */
      canRetry: computed(() => store.error()?.retryable ?? false),
    })),

    withMethods((store, api = inject(Api)) => {
      /**
       * Builds query parameters from the current state.
       */
      const buildQueryParams = (): P => {
        const state: ListState<T> = {
          data: store.data(),
          isLoading: store.isLoading(),
          error: store.error(),
          totalRecords: store.totalRecords(),
          totalPages: store.totalPages(),
          page: store.page(),
          pageSize: store.pageSize(),
          first: store.first(),
          search: store.search(),
          sortField: store.sortField(),
          sortOrder: store.sortOrder(),
          filters: store.filters(),
        };

        if (config?.buildParams) {
          return config.buildParams(state);
        }

        const orderBy =
          formatSortField(state.sortField, state.sortOrder) || config?.defaultSortField;

        return {
          Page: state.page,
          PageSize: state.pageSize,
          Search: state.search || undefined,
          OrderBy: orderBy || undefined,
          ...state.filters,
        } as P;
      };

      /**
       * Loads data from the API using rxMethod for reactive handling.
       */
      const load = rxMethod<void>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null })),
          switchMap(() => {
            const params = buildQueryParams();

            return from(api.invoke(apiFn, params)).pipe(
              tap((result: PagedResponse<T>) => {
                patchState(store, {
                  data: result.items ?? [],
                  totalRecords: result.pagination?.total ?? 0,
                  totalPages: result.pagination?.totalPages ?? 0,
                  isLoading: false,
                  error: null,
                });
              }),

              catchError((error: AppError) => {
                patchState(store, {
                  isLoading: false,
                  error: error,
                });
                return of(null);
              }),
            );
          }),
        ),
      );

      return {
        /**
         * Loads data from the API.
         */
        load,

        /**
         * Sets the current page and optionally the page size, then reloads.
         */
        setPage(page: number, pageSize?: number): void {
          const size = pageSize ?? store.pageSize();
          patchState(store, {
            page,
            pageSize: size,
            first: (page - 1) * size,
          });
          load();
        },

        /**
         * Sets the search query and reloads from page 1.
         */
        setSearch(search: string): void {
          patchState(store, { search, page: 1, first: 0 });
          load();
        },

        /**
         * Sets the sort field and order, then reloads.
         */
        setSort(sortField: string, sortOrder: 1 | -1): void {
          patchState(store, { sortField, sortOrder });
          load();
        },

        /**
         * Sets additional filters and reloads from page 1.
         */
        setFilters(filters: Record<string, unknown>): void {
          patchState(store, { filters: { ...store.filters(), ...filters }, page: 1, first: 0 });
          load();
        },

        /**
         * Handles PrimeNG table lazy load events.
         */
        onLazyLoad(event: TableLazyLoadEvent): void {
          const pageSize = event.rows ?? store.pageSize();
          const page = (event.first ?? 0) / pageSize + 1;
          const sortField = (event.sortField as string) ?? "";
          const sortOrder = (event.sortOrder ?? -1) as 1 | -1;

          patchState(store, {
            page,
            pageSize,
            first: event.first ?? 0,
            sortField,
            sortOrder,
          });
          load();
        },

        /**
         * Retries the last operation.
         */
        retry(): void {
          load();
        },

        /**
         * Resets the store to initial state.
         */
        reset(): void {
          patchState(store, initialState);
        },

        /**
         * Updates an item in the list by ID (optimistic update).
         */
        updateItem(id: string, updates: Partial<T>): void {
          patchState(store, {
            data: store.data().map((item) => {
              const itemWithId = item as T & { id?: string };
              return itemWithId.id === id ? { ...item, ...updates } : item;
            }),
          });
        },

        /**
         * Removes an item from the list by ID (optimistic update).
         */
        removeItem(id: string): void {
          patchState(store, {
            data: store.data().filter((item) => {
              const itemWithId = item as T & { id?: string };
              return itemWithId.id !== id;
            }),
            totalRecords: Math.max(0, store.totalRecords() - 1),
          });
        },

        /**
         * Clears any error state.
         */
        clearError(): void {
          patchState(store, { error: null });
        },
      };
    }),
  );
}
