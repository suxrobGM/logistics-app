export interface CreateLoad {
  name?: string;
  originAddress: string;
  originCoordinates: string;
  destinationAddress: string;
  destinationCoordinates: string;
  deliveryCost: number;
  distance: number;
  assignedDispatcherId: string;
  assignedTruckId: string;
}
