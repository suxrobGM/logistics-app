export interface UpdateTruckCommand {
  id: string;
  truckNumber?: string;
  driverIds?: string[];
}
