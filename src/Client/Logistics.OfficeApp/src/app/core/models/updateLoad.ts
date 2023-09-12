import {LoadStatus} from './loadStatus';

export interface UpdateLoad {
  id: string;
  name?: string;
  originAddress?: string;
  originCoordinates?: string;
  destinationAddress?: string;
  destinationCoordinates: string;
  deliveryCost: number;
  distance: number;
  assignedDispatcherId: string;
  assignedTruckId: string;
  status?: LoadStatus;
}
