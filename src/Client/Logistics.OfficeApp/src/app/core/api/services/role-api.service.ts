import { Observable } from "rxjs";
import { ApiBase } from "../api-base";
import { PagedResult, RoleDto, SearchableQuery } from "../models";

export class RoleApiService extends ApiBase {
  getRoles(query?: SearchableQuery): Observable<PagedResult<RoleDto>> {
    const url = `/roles/tenant?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }
}
