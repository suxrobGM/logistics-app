import {Result} from "./result";

export interface PagedResult<T> extends Result<T[]> {
  totalItems: number;
  totalPages: number;
}
