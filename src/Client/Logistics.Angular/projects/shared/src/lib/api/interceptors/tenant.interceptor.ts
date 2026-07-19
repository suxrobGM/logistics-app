import { type HttpEvent, type HttpHandlerFn, type HttpRequest } from "@angular/common/http";
import { inject, InjectionToken } from "@angular/core";
import { Observable } from "rxjs";

/**
 * Interface for resolving the current tenant id.
 * Applications should implement this interface and provide it via the TENANT_ID_PROVIDER token.
 */
export interface TenantIdProvider {
  /**
   * Get the current tenant id, or null if none is selected.
   */
  getTenantId(): string | null;
}

/**
 * Injection token for the tenant id provider service.
 *
 * Applications should provide this token with their tenant service:
 * ```typescript
 * providers: [
 *   { provide: TENANT_ID_PROVIDER, useExisting: TenantService }
 * ]
 * ```
 */
export const TENANT_ID_PROVIDER = new InjectionToken<TenantIdProvider>("TENANT_ID_PROVIDER");

/**
 * Interceptor to add the 'X-Tenant' header to the request headers.
 * Resolves the tenant id via the TENANT_ID_PROVIDER token, which each app provides.
 * If no provider is registered, or it returns no tenant id, the request passes through unchanged.
 * @param req The HTTP request to intercept.
 * @param next The next handler in the chain.
 * @returns An observable of the HTTP event.
 */
export function tenantInterceptor(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  const tenantIdProvider = inject(TENANT_ID_PROVIDER, { optional: true });
  const tenantId = tenantIdProvider?.getTenantId();

  if (tenantId) {
    req = req.clone({
      setHeaders: { "X-Tenant": tenantId },
    });
  }

  return next(req);
}
