export interface TruckGeolocation {
  truckId: string;
  tenantId?: string;
  latitude: number;
  longitude: number;
  currentLocation?: string;
  truckNumber?: string;
  driversName?: string;
}
