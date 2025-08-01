import {AddressDto} from "../address.dto";
import {TripStatus} from "./enums";
import {TripLoadDto} from "./trip-load.dto";
import {TripStopDto} from "./trip-stop.dto";

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
  loads: TripLoadDto[];
  stops: TripStopDto[];
}
