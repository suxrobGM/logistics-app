import {CreateTripLoadCommand} from "./create-trip-load.command";

export interface UpdateTripCommand {
  tripId: string;
  name?: string;
  truckId?: string;
  plannedStart?: Date;
  newLoads?: CreateTripLoadCommand[];
  attachLoadIds?: string[];
  detachLoadIds?: string[];
}
