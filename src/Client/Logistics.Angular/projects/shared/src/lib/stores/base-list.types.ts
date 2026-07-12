import type { AppError } from "../errors";

/**
 * A lazy-load request from a data table: which page, and how to sort it.
 *
 * Deliberately owned by us rather than imported from a UI library, so handlers typed with
 * this keep binding unchanged if the table is ever swapped for a different implementation.
 */
export interface ListLazyLoadEvent {
  /** Index of the first row to load (0-based). */
  first?: number | null;
  /** Number of rows per page. */
  rows?: number | null;
  /** Field to sort by. */
  sortField?: string | string[] | null;
  /** 1 for ascending, -1 for descending. */
  sortOrder?: number | null;
}

/**
 * State interface for list components with pagination, sorting, and filtering.
 */
export interface ListState<T> {
  /** The data items */
  data: T[];
  /** Whether data is currently loading */
  isLoading: boolean;
  /** Error if the last operation failed */
  error: AppError | null;
  /** Total number of records across all pages */
  totalRecords: number;
  /** Total number of pages */
  totalPages: number;

  // Pagination
  /** Current page (1-indexed) */
  page: number;
  /** Number of items per page */
  pageSize: number;
  /** First item index (0-indexed) */
  first: number;

  // Filtering & Sorting
  /** Search query string */
  search: string;
  /** Current sort field */
  sortField: string;
  /** Sort order: 1 for ascending, -1 for descending */
  sortOrder: 1 | -1;
  /** Additional filter values */
  filters: Record<string, unknown>;
}

/**
 * Common query parameters for list API calls.
 */
export interface ListQueryParams {
  Page?: number;
  PageSize?: number;
  Search?: string;
  OrderBy?: string;
  [key: string]: unknown;
}

/**
 * Configuration for creating a list store.
 */
export interface ListStoreConfig<T, P extends ListQueryParams> {
  /** Default sort field (e.g., 'Name' or '-CreatedAt' for descending) */
  defaultSortField?: string;
  /** Default page size */
  defaultPageSize?: number;
  /** Custom function to build query params from state */
  buildParams?: (state: ListState<T>) => P;
}
