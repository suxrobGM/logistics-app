import { type HttpEvent, type HttpHandlerFn, HttpRequest } from "@angular/common/http";
import { inject } from "@angular/core";
import { Observable } from "rxjs";
import { API_CONFIG } from "../api.provider";

/**
 * Interceptor to add the Bearer token to the request headers.
 * This interceptor uses the API_CONFIG to get the token and credentials settings.
 * @param req The HTTP request to intercept.
 * @param next The next handler in the chain.
 * @returns An observable of the HTTP event.
 */
export function tokenAuthInterceptor(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  const config = inject(API_CONFIG);
  const token = config.tokenGetter?.();

  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    });
  }

  if (config.withCredentials) {
    req = req.clone({ withCredentials: true });
  }
  return next(req);
}
