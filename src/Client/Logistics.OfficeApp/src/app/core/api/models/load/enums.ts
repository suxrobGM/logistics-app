import {SelectOption} from "@/shared/models";

export enum LoadStatus {
  Dispatched = "dispatched",
  PickedUp = "picked_up",
  Delivered = "delivered",
}

export const loadStatusOptions: SelectOption<LoadStatus>[] = [
  {label: "Dispatched", value: LoadStatus.Dispatched},
  {label: "Picked Up", value: LoadStatus.PickedUp},
  {label: "Delivered", value: LoadStatus.Delivered},
] as const;
