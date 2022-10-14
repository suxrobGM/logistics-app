export interface ResponseResult<T = any> {
  success: boolean;
  value?: T;
  error?: string;
}