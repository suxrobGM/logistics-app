export interface Load {
    id?: string;
    referenceId?: number;
    name?: string;
    sourceAddress?: string;
    destinationAddress?: string;
    deliveryCost?: number;
    distance?: number;
    status?: string;
    dispatchedDate?: Date;
    pickUpDate?: Date;
    deliveryDate?: Date;
    assignedDispatcherId?: string;
    assignedDispatcherName?: string;
    assignedTruckId?: string;
    assignedDriverName?: string;
}