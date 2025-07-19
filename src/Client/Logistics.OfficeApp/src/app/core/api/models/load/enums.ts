import {SelectOption} from "@/shared/models";

export enum LoadStatus {
  Dispatched = "dispatched",
  PickedUp = "picked_up",
  Delivered = "delivered",
}

export enum LoadType {
  GeneralFreight = "general_freight",
  RefrigeratedGoods = "refrigerated_goods",
  HazardousMaterials = "hazardous_materials",
  OversizeHeavy = "oversize_heavy",
  Liquid = "liquid",
  Bulk = "bulk",
  Vehicle = "vehicle",
  Livestock = "livestock",
}

export const loadStatusOptions: SelectOption<LoadStatus>[] = [
  {label: "Dispatched", value: LoadStatus.Dispatched},
  {label: "Picked Up", value: LoadStatus.PickedUp},
  {label: "Delivered", value: LoadStatus.Delivered},
] as const;

export const loadTypeOptions: SelectOption<LoadType>[] = [
  {label: "General Freight", value: LoadType.GeneralFreight},
  {label: "Refrigerated Goods", value: LoadType.RefrigeratedGoods},
  {label: "Hazardous Materials", value: LoadType.HazardousMaterials},
  {label: "Oversize/Heavy", value: LoadType.OversizeHeavy},
  {label: "Liquid", value: LoadType.Liquid},
  {label: "Bulk", value: LoadType.Bulk},
  {label: "Vehicle/Car", value: LoadType.Vehicle},
  {label: "Livestock", value: LoadType.Livestock},
] as const;
