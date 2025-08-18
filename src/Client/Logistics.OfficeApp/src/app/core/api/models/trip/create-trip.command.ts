import {CreateTripLoadCommand} from "./create-trip-load.command";

export interface CreateTripCommand {
  name: string;
  plannedStart: Date;
  newLoads?: CreateTripLoadCommand[];
  attachLoadIds?: string[];
}
