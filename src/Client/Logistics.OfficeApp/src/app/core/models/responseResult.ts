export interface ResponseResult<T = unknown> {
  isSuccess: boolean;
  isError: boolean;
  data?: T;
  error?: string;
}
