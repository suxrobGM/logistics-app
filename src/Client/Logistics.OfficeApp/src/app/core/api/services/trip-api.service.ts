import { Observable } from "rxjs";
import { ApiBase } from "../api-base";
import {
  CreateTripCommand,
  GetTripsQuery,
  OptimizeTripStopsCommand,
  OptimizedTripStopsDto,
  PagedResult,
  Result,
  TripDto,
  UpdateTripCommand,
} from "../models";

export class TripApiService extends ApiBase {
  getTrip(id: string): Observable<Result<TripDto>> {
    return this.get(`/trips/${id}`);
  }

  getTrips(query?: GetTripsQuery): Observable<PagedResult<TripDto>> {
    return this.get(`/trips?${this.stringfyQuery(query)}`);
  }

  createTrip(command: CreateTripCommand): Observable<Result> {
    return this.post("/trips", command);
  }

  updateTrip(command: UpdateTripCommand): Observable<Result> {
    return this.put(`/trips/${command.tripId}`, command);
  }

  deleteTrip(tripId: string): Observable<Result> {
    return this.delete(`/trips/${tripId}`);
  }

  optimizeTripStops(command: OptimizeTripStopsCommand): Observable<Result<OptimizedTripStopsDto>> {
    return this.post("/trips/optimize", command);
  }
}
