import {HttpClient} from "@angular/common/http";
import {Observable} from "rxjs";
import {ApiBase} from "./api-base";
import {PagedResult, SearchableQuery, UserDto} from "./models";

export class UserApi extends ApiBase {
  constructor(apiUrl: string, http: HttpClient) {
    super(apiUrl, http);
  }

  getUsers(query?: SearchableQuery): Observable<PagedResult<UserDto>> {
    const url = `/users?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }
}
