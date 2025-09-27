import { SelectOption } from "@/shared/models";

export enum TruckStatus {
  Available = "available",
  EnRoute = "en_route",
  Loading = "loading",
  Unloading = "unloading",
  Maintenance = "maintenance",
  OutOfService = "out_of_service",
  Offline = "offline",
}

export enum TruckType {
  Flatbed = "flatbed",
  Reefer = "reefer",
  FreightTruck = "freight_truck",
  Tanker = "tanker",
  BoxTruck = "box_truck",
  CarHauler = "car_hauler",
  DumpTruck = "dump_truck",
  TowTruck = "tow_truck",
}

export const truckTypeOptions: SelectOption<TruckType>[] = [
  { label: "Flatbed", value: TruckType.Flatbed },
  { label: "Reefer", value: TruckType.Reefer },
  { label: "Freight Truck", value: TruckType.FreightTruck },
  { label: "Tanker", value: TruckType.Tanker },
  { label: "Box Truck", value: TruckType.BoxTruck },
  { label: "Car Hauler", value: TruckType.CarHauler },
  { label: "Dump Truck", value: TruckType.DumpTruck },
  { label: "Tow Truck", value: TruckType.TowTruck },
] as const;

export const truckStatusOptions: SelectOption<TruckStatus>[] = [
  { label: "Available", value: TruckStatus.Available },
  { label: "En Route", value: TruckStatus.EnRoute },
  { label: "Loading", value: TruckStatus.Loading },
  { label: "Unloading", value: TruckStatus.Unloading },
  { label: "Maintenance", value: TruckStatus.Maintenance },
  { label: "Out of Service", value: TruckStatus.OutOfService },
  { label: "Offline", value: TruckStatus.Offline },
] as const;
