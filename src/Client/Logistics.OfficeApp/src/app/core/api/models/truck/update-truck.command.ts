import { TruckStatus, TruckType } from "./enums";

export interface UpdateTruckCommand {
  id: string;
  truckNumber?: string;
  truckType?: TruckType;
  truckStatus?: TruckStatus;
  mainDriverId?: string;
  secondaryDriverId?: string;
}
