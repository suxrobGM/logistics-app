import {CreateTripLoadCommand} from "./create-trip-load.command";
import {TripStopDto} from "./trip-stop.dto";

export interface CreateTripCommand {
  name: string;
  truckId: string;
  newLoads?: CreateTripLoadCommand[] | null;
  attachedLoadIds?: string[] | null;
  optimizedStops?: TripStopDto[] | null;
}
