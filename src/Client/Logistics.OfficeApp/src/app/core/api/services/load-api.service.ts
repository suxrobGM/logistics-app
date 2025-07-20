import {Observable} from "rxjs";
import {ApiBase} from "../api-base";
import {
  CreateLoadCommand,
  LoadDto,
  PagedResult,
  Result,
  SearchableQuery,
  UpdateLoadCommand,
} from "../models";

export class LoadApiService extends ApiBase {
  getLoad(id: string): Observable<Result<LoadDto>> {
    const url = `/loads/${id}`;
    return this.get(url);
  }

  getLoads(query?: SearchableQuery, onlyActiveLoads = false): Observable<PagedResult<LoadDto>> {
    let url = `/loads?${this.stringfyQuery(query)}`;

    if (onlyActiveLoads) {
      url += "&onlyActiveLoads=true";
    }
    return this.get(url);
  }

  createLoad(command: CreateLoadCommand): Observable<Result> {
    const url = `/loads`;
    return this.post(url, command);
  }

  updateLoad(command: UpdateLoadCommand): Observable<Result> {
    const url = `/loads/${command.id}`;
    return this.put(url, command);
  }

  deleteLoad(loadId: string): Observable<Result> {
    const url = `/loads/${loadId}`;
    return this.delete(url);
  }
}
