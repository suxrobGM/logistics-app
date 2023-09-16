import {LoadStatus} from './loadStatus';

export interface UpdateLoad {
  id: string;
  name?: string;
  originAddress: string;
  originLatitude: number;
  originLongitude: number;
  destinationAddress: string;
  destinationLatitude: number;
  destinationLongitude: number;
  deliveryCost: number;
  distance: number;
  assignedDispatcherId: string;
  assignedTruckId: string;
  status?: LoadStatus;
}
