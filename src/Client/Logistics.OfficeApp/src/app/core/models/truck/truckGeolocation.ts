export interface TruckGeolocation {
  truckId: string;
  tenantId?: string;
  latitude: number;
  longitude: number;
  currentAddress?: string;
  truckNumber?: string;
  driversName?: string;
}
