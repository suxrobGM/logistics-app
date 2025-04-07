import {LoadStatus} from "@/core/enums";
import {CustomerDto} from "../customer";
import {InvoiceDto} from "../invoice";
import {AddressDto} from "../address.dto";

export interface LoadDto {
  id: string;
  refId: number;
  name?: string;
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
}
