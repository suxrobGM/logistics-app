import {HttpClient} from "@angular/common/http";
import {Observable} from "rxjs";
import {ApiBase} from "./api-base";
import {Result, TenantDto} from "./models";

export class TenantApi extends ApiBase {
  constructor(apiUrl: string, http: HttpClient) {
    super(apiUrl, http);
  }

  getTenant(tenantId: string): Observable<Result<TenantDto>> {
    return this.get(`/tenants/${tenantId}`);
  }
}
