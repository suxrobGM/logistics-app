import {CreateTripLoadCommand} from "./create-trip-load.command";

export interface CreateTripCommand {
  name: string;
  newLoads?: CreateTripLoadCommand[];
  attachLoadIds?: string[];
}
