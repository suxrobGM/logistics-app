import { Observable } from "rxjs";
import { ApiBase } from "../api-base";
import { Result, TenantDto } from "../models";

export class TenantApiService extends ApiBase {
  getTenant(tenantId: string): Observable<Result<TenantDto>> {
    return this.get(`/tenants/${tenantId}`);
  }
}
