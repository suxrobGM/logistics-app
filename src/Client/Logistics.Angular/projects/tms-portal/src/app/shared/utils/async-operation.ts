import { inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import { ToastService } from "@/core/services";

export interface AsyncOperationOptions {
  successMessage?: string;
  navigateTo?: string;
}

export interface AsyncOperationState {
  loading: ReturnType<typeof signal<boolean>>;
  error: ReturnType<typeof signal<string | null>>;
  execute: <T>(fn: () => Promise<T>, options?: AsyncOperationOptions) => Promise<T | undefined>;
  reset: () => void;
}

/**
 * Creates an async operation handler with loading state, error handling, and optional toast/navigation.
 * Use this to reduce boilerplate in CRUD components.
 *
 * @example
 * ```typescript
 * export class CustomerAddComponent {
 *   private readonly asyncOp = useAsyncOperation();
 *   protected readonly isLoading = this.asyncOp.loading;
 *
 *   async createCustomer(formValue: CustomerFormValue) {
 *     await this.asyncOp.execute(
 *       () => this.api.invoke(createCustomer, { body: command }),
 *       { successMessage: "Customer created", navigateTo: "/customers" }
 *     );
 *   }
 * }
 * ```
 */
export function useAsyncOperation(): AsyncOperationState {
  const toastService = inject(ToastService);
  const router = inject(Router);

  const loading = signal(false);
  const error = signal<string | null>(null);

  const execute = async <T>(
    fn: () => Promise<T>,
    options?: AsyncOperationOptions,
  ): Promise<T | undefined> => {
    loading.set(true);
    error.set(null);

    try {
      const result = await fn();

      if (options?.successMessage) {
        toastService.showSuccess(options.successMessage);
      }

      if (options?.navigateTo) {
        router.navigateByUrl(options.navigateTo);
      }

      return result;
    } catch (err) {
      const message = extractErrorMessage(err);
      error.set(message);
      toastService.showError(message);
      return undefined;
    } finally {
      loading.set(false);
    }
  };

  const reset = () => {
    loading.set(false);
    error.set(null);
  };

  return { loading, error, execute, reset };
}

function extractErrorMessage(err: unknown): string {
  if (err instanceof Error) {
    return err.message;
  }
  if (typeof err === "string") {
    return err;
  }
  return "An unexpected error occurred";
}
