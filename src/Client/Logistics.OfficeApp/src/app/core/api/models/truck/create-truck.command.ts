import { TruckStatus, TruckType } from "./enums";

export interface CreateTruckCommand {
  truckNumber: string;
  truckType: TruckType;
  truckStatus: TruckStatus;
  mainDriverId?: string;
  secondaryDriverId?: string;
}
