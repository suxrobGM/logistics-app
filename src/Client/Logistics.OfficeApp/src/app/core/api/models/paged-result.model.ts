import {Result} from "./result.model";

export interface PagedResult<T> extends Result<T[]> {
  totalItems: number;
  totalPages: number;
}
