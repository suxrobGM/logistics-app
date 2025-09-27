import { Observable } from "rxjs";
import { ApiBase } from "../api-base";
import {
  CreateLoadCommand,
  GetLoadsQuery,
  LoadDto,
  PagedResult,
  Result,
  UpdateLoadCommand,
} from "../models";

export class LoadApiService extends ApiBase {
  getLoad(id: string): Observable<Result<LoadDto>> {
    return this.get(`/loads/${id}`);
  }

  getLoads(query?: GetLoadsQuery): Observable<PagedResult<LoadDto>> {
    return this.get(`/loads?${this.stringfyQuery(query)}`);
  }

  createLoad(command: CreateLoadCommand): Observable<Result> {
    return this.post("/loads", command);
  }

  updateLoad(command: UpdateLoadCommand): Observable<Result> {
    return this.put(`/loads/${command.id}`, command);
  }

  deleteLoad(loadId: string): Observable<Result> {
    return this.delete(`/loads/${loadId}`);
  }
}
