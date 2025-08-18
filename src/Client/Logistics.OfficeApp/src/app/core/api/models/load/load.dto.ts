import {AddressDto} from "../address.dto";
import {CustomerDto} from "../customer";
import {GeoPointDto} from "../geo-point.dto";
import {InvoiceDto} from "../invoice";
import {LoadStatus, LoadType} from "./enums";

export interface LoadDto {
  id: string;
  number: number;
  name: string;
  type: LoadType;
  originAddress: AddressDto;
  originLocation: GeoPointDto;
  destinationAddress: AddressDto;
  destinationLocation: GeoPointDto;
  deliveryCost: number;
  distance: number;
  status: LoadStatus;
  createdAt: Date;
  dispatchedAt?: Date;
  pickedUpAt?: Date;
  deliveredAt?: Date;
  cancelledAt?: Date;
  assignedDispatcherId: string;
  assignedDispatcherName?: string;
  assignedTruckId: string;
  assignedTruckNumber?: string;
  assignedTruckDriversName?: string[];
  currentAddress?: AddressDto;
  currentLocation?: GeoPointDto;
  customer?: CustomerDto;
  invoice?: InvoiceDto;
  tripId?: string;
  tripNumber?: number;
  tripName?: string;
}
