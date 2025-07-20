import {AddressDto} from "../address.dto";
import {CustomerDto} from "../customer";
import {InvoiceDto} from "../invoice";
import {LoadStatus, LoadType} from "./enums";

export interface LoadDto {
  id: string;
  number: number;
  name: string;
  type: LoadType;
  originAddress: AddressDto;
  originAddressLat: number;
  originAddressLong: number;
  destinationAddress: AddressDto;
  destinationAddressLat: number;
  destinationAddressLong: number;
  deliveryCost: number;
  distance: number;
  status: LoadStatus;
  dispatchedDate: string;
  pickUpDate: string;
  deliveryDate: string;
  assignedDispatcherId: string;
  assignedDispatcherName?: string;
  assignedTruckId: string;
  assignedTruckNumber?: string;
  assignedTruckDriversName?: string[];
  currentLocation?: string;
  customer?: CustomerDto;
  invoice?: InvoiceDto;
  tripId?: string;
}
