import {Result} from "./Result";

export interface PagedResponseResult<T> extends Result<T[]> {
  totalItems: number;
  totalPages: number;
}
