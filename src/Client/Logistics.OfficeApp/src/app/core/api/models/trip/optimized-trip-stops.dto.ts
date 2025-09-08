import {TripStopDto} from "./trip-stop.dto";

export interface OptimizedTripStopsDto {
  totalDistance: number;
  orderedStops: TripStopDto[];
}
