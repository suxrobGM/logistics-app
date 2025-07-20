import {AddressDto} from "../address.dto";
import {LoadDto} from "../load";
import {TripStatus} from "./enums";

export interface TripDto {
  id: string;
  number: number;
  name: string;
  originAddress: AddressDto;
  destinationAddress: AddressDto;
  totalDistance: number;
  plannedStart: Date;
  actualStart?: Date;
  completedAt?: Date;
  status: TripStatus;
  truckId: string;
  truckNumber: string;
  loads: LoadDto[];
}
