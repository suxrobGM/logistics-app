import { AddressDto } from "../address.dto";
import { GeoPointDto } from "../geo-point.dto";
import { LoadType } from "../load";

export interface CreateTripLoadCommand {
  tempId?: string; // Temporary ID for mapping optimized stops
  name: string;
  originAddress: AddressDto;
  originLocation: GeoPointDto;
  destinationAddress: AddressDto;
  destinationLocation: GeoPointDto;
  deliveryCost: number;
  distance: number;
  type: LoadType;
  assignedDispatcherId: string;
  customerId?: string;
}
