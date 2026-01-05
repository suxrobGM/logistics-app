import { HttpErrorResponse } from "@angular/common/http";
import { Injectable, inject } from "@angular/core";
import { ToastService } from "../services";
import type { AppError, ErrorCategory } from "./error.types";

/**
 * Service for categorizing and handling application errors consistently.
 */
@Injectable({ providedIn: "root" })
export class ErrorHandlerService {
  private readonly toastService = inject(ToastService);

  /**
   * Categorizes an HTTP error response into a standardized AppError.
   */
  categorizeError(error: HttpErrorResponse): AppError {
    const statusCode = error.status;

    // Network error (no connection)
    if (statusCode === 0) {
      return {
        category: "network",
        message: "Unable to connect to the server. Please check your internet connection.",
        originalError: error,
        retryable: true,
        statusCode,
      };
    }

    // Authentication/Authorization errors
    if (statusCode === 401) {
      return {
        category: "auth",
        message: "Your session has expired. Please sign in again.",
        originalError: error,
        retryable: false,
        statusCode,
      };
    }

    if (statusCode === 403) {
      return {
        category: "auth",
        message: "You don't have permission to perform this action.",
        originalError: error,
        retryable: false,
        statusCode,
      };
    }

    // Validation errors
    if (statusCode === 400 || statusCode === 422) {
      const fieldErrors = this.extractFieldErrors(error);
      const message = this.extractErrorMessage(error) || "Please check your input and try again.";

      return {
        category: "validation",
        message,
        originalError: error,
        retryable: false,
        statusCode,
        fieldErrors,
      };
    }

    // Not found
    if (statusCode === 404) {
      return {
        category: "unknown",
        message: this.extractErrorMessage(error) || "The requested resource was not found.",
        originalError: error,
        retryable: false,
        statusCode,
      };
    }

    // Server errors (5xx)
    if (statusCode >= 500) {
      return {
        category: "server",
        message: "A server error occurred. Please try again later.",
        originalError: error,
        retryable: true,
        statusCode,
      };
    }

    // Unknown errors
    return {
      category: "unknown",
      message: this.extractErrorMessage(error) || "An unexpected error occurred.",
      originalError: error,
      retryable: false,
      statusCode,
    };
  }

  /**
   * Handles an AppError by displaying appropriate feedback to the user.
   */
  handleError(error: AppError): void {
    // Log for debugging
    console.error(`[${error.category.toUpperCase()}]`, error.message, error.originalError);

    // Show toast based on category
    switch (error.category) {
      case "network":
        this.toastService.showError(error.message);
        break;
      case "auth":
        // Auth errors might be handled differently (e.g., redirect to login)
        this.toastService.showError(error.message);
        break;
      case "validation":
        this.toastService.showError(error.message);
        break;
      case "server":
        this.toastService.showError(error.message);
        break;
      default:
        this.toastService.showError(error.message);
    }
  }

  /**
   * Extracts the error message from various response formats.
   */
  private extractErrorMessage(error: HttpErrorResponse): string | null {
    // Try common error response formats
    if (typeof error.error === "string") {
      return error.error;
    }

    if (error.error?.error) {
      return error.error.error;
    }

    if (error.error?.message) {
      return error.error.message;
    }

    if (error.error?.title) {
      return error.error.title;
    }

    if (error.message) {
      return error.message;
    }

    return null;
  }

  /**
   * Extracts field-level validation errors from the response.
   */
  private extractFieldErrors(error: HttpErrorResponse): Record<string, string[]> | undefined {
    // ASP.NET Core validation errors format
    if (error.error?.errors && typeof error.error.errors === "object") {
      return error.error.errors;
    }

    return undefined;
  }

  /**
   * Creates an AppError from a category and message (for non-HTTP errors).
   */
  createError(category: ErrorCategory, message: string, retryable = false): AppError {
    return {
      category,
      message,
      retryable,
    };
  }
}
