import type { FuelCardProviderType, FuelCardTransactionStatus } from "@logistics/shared/api";
import type { UiBadgeIntent } from "@logistics/shared/ui";

export interface FuelCardProviderOption {
  label: string;
  value: FuelCardProviderType;
  description: string;
}

export const FUEL_CARD_PROVIDER_OPTIONS: FuelCardProviderOption[] = [
  {
    label: "Demo (Testing)",
    value: "demo" as FuelCardProviderType,
    description: "Simulated fuel card transactions for testing",
  },
  {
    label: "WEX",
    value: "wex" as FuelCardProviderType,
    description: "Connect to WEX fleet cards",
  },
  {
    label: "EFS",
    value: "efs" as FuelCardProviderType,
    description: "Connect to EFS (WEX OTR) fleet cards",
  },
];

export function getFuelCardProviderLabel(type?: FuelCardProviderType | string): string {
  if (!type) return "Unknown";
  return FUEL_CARD_PROVIDER_OPTIONS.find((o) => o.value === type)?.label ?? type;
}

export function getTransactionStatusSeverity(status?: FuelCardTransactionStatus): UiBadgeIntent {
  switch (status) {
    case "matched":
      return "success";
    case "pending":
      return "warn";
    case "ignored":
      return "secondary";
    default:
      return "secondary";
  }
}
