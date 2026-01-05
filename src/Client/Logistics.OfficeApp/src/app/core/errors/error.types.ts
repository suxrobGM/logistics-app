/**
 * Categories of errors for different handling strategies
 */
export type ErrorCategory = "validation" | "network" | "server" | "auth" | "unknown";

/**
 * Standardized application error structure
 */
export interface AppError {
  /** The category of error for handling decisions */
  category: ErrorCategory;
  /** User-friendly error message */
  message: string;
  /** The original error for debugging */
  originalError?: unknown;
  /** Whether the operation can be retried */
  retryable: boolean;
  /** HTTP status code if applicable */
  statusCode?: number;
  /** Field-level validation errors */
  fieldErrors?: Record<string, string[]>;
}
