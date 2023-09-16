import {LoadStatus} from './loadStatus';

export interface Load {
  id: string;
  refId: number;
  name?: string;
  originAddress: string;
  originCoordinates: string;
  destinationAddress: string;
  destinationCoordinates: string;
  deliveryCost: number;
  distance: number;
  status: LoadStatus;
  dispatchedDate: string;
  pickUpDate: string;
  deliveryDate: string;
  currentLocation?: string;
  assignedDispatcherId: string;
  assignedDispatcherName: string;
  assignedTruckId: string;
  assignedTruckNumber: string;
  assignedTruckDriversId: string[];
  assignedTruckDriversName: string[];
}
