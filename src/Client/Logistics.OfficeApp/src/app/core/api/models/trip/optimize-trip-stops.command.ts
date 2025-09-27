import { TripStopDto } from "./trip-stop.dto";

export interface OptimizeTripStopsCommand {
  maxVehicles: number;
  stops: Omit<TripStopDto, "id" | "loadId">[];
}
