import {ResponseResult} from './responseResult';

export interface PagedResponseResult<T> extends ResponseResult<T[]> {
  totalItems: number;
  totalPages: number;
}
