import { CreateTripLoadCommand } from "./create-trip-load.command";
import { TripStopDto } from "./trip-stop.dto";

export interface UpdateTripCommand {
  tripId: string;
  name?: string;
  truckId?: string;
  newLoads?: CreateTripLoadCommand[] | null;
  attachedLoadIds?: string[] | null;
  detachedLoadIds?: string[] | null;
  optimizedStops?: TripStopDto[] | null;
}
