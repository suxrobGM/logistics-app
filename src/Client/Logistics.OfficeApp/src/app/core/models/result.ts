export interface Result<T = unknown> {
  success: boolean;
  data?: T;
  error?: string;
}
