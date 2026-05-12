import type { LoadBoardProviderType } from "@logistics/shared/api";

export interface EquipmentOption {
  label: string;
  value: string;
}

export interface ProviderOption {
  label: string;
  value: LoadBoardProviderType;
  description: string;
}

export const EQUIPMENT_OPTIONS: EquipmentOption[] = [
  { label: "Dry Van", value: "Dry Van" },
  { label: "Flatbed", value: "Flatbed" },
  { label: "Reefer", value: "Reefer" },
  { label: "Step Deck", value: "Step Deck" },
  { label: "Lowboy", value: "Lowboy" },
  { label: "Car Hauler", value: "Car Hauler" },
  { label: "Box Truck", value: "Box Truck" },
];

export const PROVIDER_OPTIONS: ProviderOption[] = [
  {
    label: "Demo (Testing)",
    value: "demo" as LoadBoardProviderType,
    description: "Simulated load board data for testing",
  },
  {
    label: "DAT",
    value: "dat" as LoadBoardProviderType,
    description: "Connect to DAT Freight & Analytics",
  },
  {
    label: "Truckstop",
    value: "truckstop" as LoadBoardProviderType,
    description: "Connect to Truckstop.com",
  },
  {
    label: "123Loadboard",
    value: "one_two3_loadboard" as LoadBoardProviderType,
    description: "Connect to 123Loadboard",
  },
];

export function getProviderLabel(type?: LoadBoardProviderType | string): string {
  if (!type) return "Unknown";
  return PROVIDER_OPTIONS.find((o) => o.value === type)?.label ?? type;
}

export function getProviderSeverity(
  type?: LoadBoardProviderType | string,
): "info" | "success" | "warn" | "secondary" {
  switch (type) {
    case "dat":
      return "info";
    case "truckstop":
      return "success";
    case "one_two3_loadboard":
      return "warn";
    default:
      return "secondary";
  }
}

export function getPostedTruckStatusSeverity(
  status?: string,
): "success" | "warn" | "danger" | "secondary" {
  switch (status) {
    case "available":
      return "success";
    case "booked":
      return "warn";
    case "expired":
      return "danger";
    default:
      return "secondary";
  }
}
