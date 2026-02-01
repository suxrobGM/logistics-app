import type { AccidentSeverity, AccidentType } from "@logistics/shared/api";

export const ACCIDENT_TYPE_OPTIONS = [
  { label: "Collision", value: "collision" },
  { label: "Rollover", value: "rollover" },
  { label: "Jackknife", value: "jackknife" },
  { label: "Run Off Road", value: "run_off_road" },
  { label: "Rear End", value: "rear_end" },
  { label: "Sideswipe", value: "sideswipe" },
  { label: "Head On", value: "head_on" },
  { label: "Hit and Run", value: "hit_and_run" },
  { label: "Pedestrian Involved", value: "pedestrian_involved" },
  { label: "Property Damage Only", value: "property_damage_only" },
  { label: "Cargo Spill", value: "cargo_spill" },
  { label: "Other", value: "other" },
] as const;

export const ACCIDENT_SEVERITY_OPTIONS = [
  { label: "Minor", value: "minor" },
  { label: "Moderate", value: "moderate" },
  { label: "Severe", value: "severe" },
  { label: "Fatal", value: "fatal" },
] as const;

export function getAccidentTypeLabel(type: AccidentType | undefined): string {
  return ACCIDENT_TYPE_OPTIONS.find((o) => o.value === type)?.label ?? type ?? "Unknown";
}

export function getAccidentSeverityLabel(severity: AccidentSeverity | undefined): string {
  return (
    ACCIDENT_SEVERITY_OPTIONS.find((o) => o.value === severity)?.label ?? severity ?? "Unknown"
  );
}
