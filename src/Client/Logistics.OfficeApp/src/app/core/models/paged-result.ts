import {Result} from "./_result";

export interface PagedResponseResult<T> extends Result<T[]> {
  totalItems: number;
  totalPages: number;
}
