import {Observable} from "rxjs";
import {ApiBase} from "./api-base";
import {PagedResult, SearchableQuery, UserDto} from "./models";

export class UserApi extends ApiBase {
  constructor(apiUrl: string) {
    super(apiUrl);
  }

  getUsers(query?: SearchableQuery): Observable<PagedResult<UserDto>> {
    const url = `/users?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }
}
