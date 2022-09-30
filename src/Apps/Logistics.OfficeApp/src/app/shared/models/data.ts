export interface DataResult<T = any> {
  success: boolean;
  value?: T;
  error?: string;
}