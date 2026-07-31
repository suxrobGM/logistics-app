import {
  HttpContext,
  HttpContextToken,
  HttpErrorResponse,
  HttpRequest,
  type HttpEvent,
  type HttpHandlerFn,
} from "@angular/common/http";
import { inject } from "@angular/core";
import { catchError, Observable, throwError } from "rxjs";
import { ErrorHandlerService } from "../../errors";

/**
 * Suppresses the global failure toast for one request. For calls whose failure is not worth
 * interrupting the user over - background polls, advisory reads - where the caller handles the
 * error itself. The error still propagates to the caller.
 */
export const SKIP_ERROR_TOAST = new HttpContextToken(() => false);

/** Convenience wrapper for the common `{ context: silentErrors() }` call site. */
export const silentErrors = (): HttpContext => new HttpContext().set(SKIP_ERROR_TOAST, true);

export function errorHandlerInterceptor(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  const errorHandlerService = inject(ErrorHandlerService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Skip showing toast for 401 errors - auth flow handles session expiry
      if (error.status !== 401 && !req.context.get(SKIP_ERROR_TOAST)) {
        errorHandlerService.handleError(error);
      }
      return throwError(() => error);
    }),
  );
}
