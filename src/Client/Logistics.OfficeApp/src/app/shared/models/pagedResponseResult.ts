export interface PagedResponseResult<T> {
  success: boolean;
  items?: T[];
  error?: string;
  itemsCount?: number;
  pagesCount?: number;
}