import {Injectable} from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
} from '@angular/common/http';
import {Observable} from 'rxjs';
import {TenantService} from '../services/tenant.service';

@Injectable()
export class TenantInterceptor implements HttpInterceptor {
  constructor(private tenantService: TenantService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const tenantId = this.tenantService.getTenantName();
    const headers = this.tenantService.createTenantHeaders(request.headers, tenantId);
    this.tenantService.setTenantCookie(tenantId);

    request = request.clone({
      headers: headers,
    });
    return next.handle(request);
  }
}
