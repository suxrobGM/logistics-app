import { type HttpInterceptorFn } from "@angular/common/http";
import { inject } from "@angular/core";
import { TenantContextService } from "../services";

export const tenantInterceptor: HttpInterceptorFn = (req, next) => {
  const tenantService = inject(TenantContextService);
  const tenantId = tenantService.getTenantId();

  if (tenantId) {
    const clonedReq = req.clone({
      setHeaders: {
        "X-Tenant": tenantId,
      },
    });
    return next(clonedReq);
  }

  return next(req);
};
