export interface ResponseResult<T = any> {
  isSuccess: boolean;
  isError: boolean;
  data?: T;
  error?: string;
}
