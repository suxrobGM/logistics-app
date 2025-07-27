import {AddressDto} from "../address.dto";
import {CustomerDto} from "../customer";
import {LoadStatus} from "../load";

export interface TripLoadDto {
  id: string;
  number: number;
  name: string;
  status: LoadStatus;
  distance: number;
  deliveryCost: number;
  originAddress: AddressDto;
  destinationAddress: AddressDto;
  customer: CustomerDto;
}
