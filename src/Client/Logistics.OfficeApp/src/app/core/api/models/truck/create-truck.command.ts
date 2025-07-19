import {TruckType} from "./enums";

export interface CreateTruckCommand {
  truckNumber: string;
  truckType: TruckType;
  driverIds: string[];
}
