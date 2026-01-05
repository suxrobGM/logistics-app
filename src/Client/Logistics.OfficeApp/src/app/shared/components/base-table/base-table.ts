import { Component, DestroyRef, inject, signal } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import type { TableLazyLoadEvent } from "primeng/table";
import { Observable, finalize } from "rxjs";
import type { PagedResult } from "@/core/api/models";

/**
 * Parameters for querying the table data.
 */
export interface TableQueryParams {
  /**
   * Zero-based page index.
   */
  page: number;
  /**
   * Number of items per page.
   */
  size: number;
  /**
   * Field to sort by.
   */
  sortField?: string;
  /**
   * Sort order: 1 for ascending, -1 for descending.
   */
  sortOrder?: 1 | -1;
  /**
   * Search term to filter the data.
   */
  search?: string;
}

/**
 * Base class for components that display data in a table format using PrimeNG Table component.
 * This class provides methods for querying data, handling pagination, sorting, and searching.
 * It should be extended by specific table components to implement the `query` method.
 */
@Component({
  template: "",
})
export abstract class BaseTableComponent<T> {
  private readonly destroyRef = inject(DestroyRef);

  /**
   * Loading state of the table
   */
  protected readonly isLoading = signal(false);

  /**
   * Current page index (0-based)
   */
  protected readonly first = signal(0);

  /**
   * Total number of records in the table
   */
  protected readonly totalRecords = signal(0); // server-side total

  /**
   * Data displayed in the table
   */
  protected readonly data = signal<T[]>([]);

  /**
   * Abstract method to be implemented by subclasses to query data.
   * This method should return an Observable that emits a PagedResult.
   * @param params - The parameters for pagination, sorting, and searching.
   * @return An Observable that emits a PagedResult of type T.
   */
  protected abstract query(params: TableQueryParams): Observable<PagedResult<T>>;

  /**
   * Fetches data based on the provided parameters.
   * Call this method on NgOnInit or when you need to refresh the table data.
   * @param params - The parameters for pagination, sorting, and searching.
   */
  protected fetch(params: TableQueryParams): void {
    const { page = 0, size = 10, sortField = "", sortOrder = 1, search } = params;
    this.isLoading.set(true);

    this.query({ page, size, sortField, sortOrder, search })
      .pipe(
        finalize(() => this.isLoading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((r) => {
        this.data.set(r.data || []);
        this.totalRecords.set(r.totalItems ?? 0);
        this.first.set(page * size);
      });
  }

  /**
   * Handles lazy loading events from the PrimeNG table.
   * This method should be bound to the table's lazyLoad event.
   * @param e - The lazy load event containing pagination and sorting information.
   */
  public onLazyLoad(e: TableLazyLoadEvent): void {
    const size = e.rows ?? 10;
    const page = (e.first ?? 0) / size;
    this.fetch({
      page,
      size,
      sortField: e.sortField as string,
      sortOrder: e.sortOrder as 1 | -1,
    });
  }

  /**
   * Handles search input changes.
   * This method can be bound to an input field to filter the table data.
   * @param value - The search term to filter the data.
   */
  public onSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.fetch({ search: value, page: 0, size: 10 });
  }
}
