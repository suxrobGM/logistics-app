import type { LoadBoardProviderType } from "@logistics/shared/api";
import type { UiBadgeIntent } from "@logistics/shared/ui";

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

export function getProviderSeverity(type?: LoadBoardProviderType | string): UiBadgeIntent {
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

/** Broker credit score badge: >=75 healthy, 50-74 caution, <50 risky. */
export function getCreditSeverity(score?: number | null): UiBadgeIntent {
  if (score == null) return "secondary";
  if (score >= 75) return "success";
  if (score >= 50) return "warn";
  return "danger";
}

export function getPostedTruckStatusSeverity(status?: string): UiBadgeIntent {
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
