import type { EldProviderType } from "@logistics/shared/api";

export interface EldProviderOption {
  label: string;
  value: EldProviderType;
  description: string;
}

export const ELD_PROVIDER_OPTIONS: EldProviderOption[] = [
  {
    label: "Demo (Testing)",
    value: "demo" as EldProviderType,
    description: "Simulated ELD data for testing without real devices",
  },
  {
    label: "Samsara",
    value: "samsara" as EldProviderType,
    description: "Connect to Samsara ELD devices",
  },
  {
    label: "Motive (KeepTruckin)",
    value: "motive" as EldProviderType,
    description: "Connect to Motive/KeepTruckin ELD devices",
  },
  {
    label: "Geotab",
    value: "geotab" as EldProviderType,
    description: "Connect to Geotab tachograph (EU) or US ELD devices",
  },
];

const ELD_PROVIDER_LABELS: Record<string, string> = {
  demo: "Demo (Testing)",
  samsara: "Samsara",
  motive: "Motive (KeepTruckin)",
  geotab: "Geotab",
  omnitracs: "Omnitracs",
  people_net: "PeopleNet",
  tt_eld: "TT-ELD",
};

export function getEldProviderLabel(type?: EldProviderType | string | null): string {
  if (!type) return "Unknown";
  return ELD_PROVIDER_LABELS[type] ?? type;
}
