export interface ResponseResult<T = any> {
  isSuccess: boolean;
  isError: boolean;
  value?: T;
  error?: string;
}
