export interface PagedDataResult<T> {
    success: boolean;
    items?: T[];
    error?: string;
    ItemsCount?: number;
    PagesCount?: number;
}