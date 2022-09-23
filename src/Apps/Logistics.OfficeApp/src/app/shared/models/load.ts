import { LoadStatus } from './loadStatus';

export interface Load {
  id?: string;
  refId?: number;
  name?: string;
  sourceAddress?: string;
  destinationAddress?: string;
  deliveryCost?: number;
  distance?: number;
  status?: LoadStatus;
  dispatchedDate?: string;
  pickUpDate?: string;
  deliveryDate?: string;
  assignedDispatcherId?: string;
  assignedDispatcherName?: string;
  assignedTruckId?: string;
  assignedDriverId?: string;
  assignedDriverName?: string;
}