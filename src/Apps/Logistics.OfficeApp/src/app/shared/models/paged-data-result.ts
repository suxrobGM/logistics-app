export interface PagedDataResult<T> {
    success: boolean;
    items?: T[];
    error?: string;
    totalItems?: number;
    totalPages?: number;
}