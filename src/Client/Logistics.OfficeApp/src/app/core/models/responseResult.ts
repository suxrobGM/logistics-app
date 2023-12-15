export interface ResponseResult<T = unknown> {
  isSuccess: boolean;
  data?: T;
  error?: string;
}
