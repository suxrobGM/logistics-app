import {
  HttpErrorResponse,
  type HttpEvent,
  type HttpHandlerFn,
  HttpRequest,
} from "@angular/common/http";
import { inject } from "@angular/core";
import { Observable, catchError, throwError } from "rxjs";
import { ErrorHandlerService } from "../../errors";

export function errorHandlerInterceptor(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  const errorHandlerService = inject(ErrorHandlerService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      errorHandlerService.handleError(error);
      return throwError(() => error);
    }),
  );
}
