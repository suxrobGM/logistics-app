import {ResponseResult} from './responseResult';

export interface PagedResponseResult<T> extends ResponseResult<T[]> {
  isSuccess: boolean;
  isError: boolean;
  error?: string;
  totalItems: number;
  totalPages: number;
}
