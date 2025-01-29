import {inject} from "@angular/core";
import {HttpRequest, HttpEvent, HttpHandlerFn} from "@angular/common/http";
import {Observable} from "rxjs";
import {TenantService} from "../services/tenant.service";

export function tenantInterceptor(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> {
  const tenantService = inject(TenantService);
  const tenantId = tenantService.getTenantName();
  const headers = tenantService.createTenantHeaders(request.headers, tenantId);
  tenantService.setTenantCookie(tenantId);

  request = request.clone({
    headers: headers,
  });
  return next(request);
}
