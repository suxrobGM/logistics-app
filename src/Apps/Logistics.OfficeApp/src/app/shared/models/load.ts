export interface Load {
    id?: string;
    name?: string;
    sourceAddress: string;
    destinationAddress: string;
    pricePerMile: number;
    totalTripMiles: number;
    isCompleted: boolean;
    status: string;
    assignedDispatcherId: string;
    assignedDispatcherName?: string;
    assignedTruckId: string;
    assignedTruckDriverName?: string;
}