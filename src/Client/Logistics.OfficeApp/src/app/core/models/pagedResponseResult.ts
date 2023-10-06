export interface PagedResponseResult<T> {
  isSuccess: boolean;
  isError: boolean;
  items?: T[];
  error?: string;
  itemsCount?: number;
  pagesCount?: number;
}
