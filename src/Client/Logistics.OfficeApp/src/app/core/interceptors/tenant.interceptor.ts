import { type HttpEvent, type HttpHandlerFn, HttpRequest } from "@angular/common/http";
import { inject } from "@angular/core";
import { Observable } from "rxjs";
import { TenantService } from "@/core/services/tenant.service";

export function tenantInterceptor(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  const tenantService = inject(TenantService);
  const headers = tenantService.generateTenantHeaders(request.headers);

  request = request.clone({
    headers: headers,
  });
  return next(request);
}
