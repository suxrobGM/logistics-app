import { Observable } from "rxjs";
import { ApiBase } from "../api-base";
import {
  CreateTruckCommand,
  PagedResult,
  Result,
  SearchableQuery,
  TruckDto,
  UpdateTruckCommand,
} from "../models";

export class TruckApiService extends ApiBase {
  getTruck(truckId: string): Observable<Result<TruckDto>> {
    return this.get(`/trucks/${truckId}`);
  }

  getTrucks(query?: SearchableQuery): Observable<PagedResult<TruckDto>> {
    return this.get(`/trucks?${this.stringfySearchableQuery(query)}`);
  }

  createTruck(command: CreateTruckCommand): Observable<Result> {
    return this.post("/trucks", command);
  }

  updateTruck(command: UpdateTruckCommand): Observable<Result> {
    return this.put(`/trucks/${command.id}`, command);
  }

  deleteTruck(truckId: string): Observable<Result> {
    return this.delete(`/trucks/${truckId}`);
  }
}
