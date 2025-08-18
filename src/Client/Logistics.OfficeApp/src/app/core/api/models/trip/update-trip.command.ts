import {CreateTripLoadCommand} from "./create-trip-load.command";

export interface UpdateTripCommand {
  tripId: string;
  name?: string;
  truckId?: string;
  newLoads?: CreateTripLoadCommand[];
  attachLoadIds?: string[];
  detachLoadIds?: string[];
}
