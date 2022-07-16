export interface Load {
    id?: string;
    referenceId?: number;
    name?: string;
    sourceAddress: string;
    destinationAddress: string;
    deliveryCost: number;
    distance: number;
    status: string;
    assignedDispatcherId: string;
    assignedDispatcherName?: string;
    assignedTruckId: string;
    assignedTruckDriverName?: string;
}