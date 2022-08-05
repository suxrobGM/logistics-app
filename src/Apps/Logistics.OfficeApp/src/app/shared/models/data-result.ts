export interface DataResult<T> {
  success: boolean;
  value?: T;
  error?: string;
}