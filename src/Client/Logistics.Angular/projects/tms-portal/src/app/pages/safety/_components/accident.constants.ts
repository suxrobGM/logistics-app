import type { SelectOption } from "@logistics/shared";
import type {
  AccidentSeverity,
  AccidentType,
  Address,
  EmployeeDto,
  TruckDto,
} from "@logistics/shared/api";

/** Step 1 (incident details) sub-form model. Shared by the parent pages and the field fragment. */
export interface AccidentIncidentModel {
  accidentDateTime: Date;
  location: Address | null;
  truck: TruckDto | null;
  driver: EmployeeDto | null;
  type: AccidentType;
  severity: AccidentSeverity;
  description: string;
  weatherConditions: string;
  roadConditions: string;
}

/** Step 2 (injuries & damage) sub-form model. Shared by the parent pages and the field fragment. */
export interface AccidentInjuriesDamageModel {
  injuriesReported: boolean;
  numberOfInjuries: number | null;
  injuryDescription: string;
  fatalitiesReported: boolean;
  numberOfFatalities: number | null;
  hazmatInvolved: boolean;
  hazmatDescription: string;
  estimatedDamage: number | null;
  damageDescription: string;
  vehicleTowed: boolean;
  towCompany: string;
}

export const ACCIDENT_TYPE_OPTIONS: SelectOption<AccidentType>[] = [
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
];

export const ACCIDENT_SEVERITY_OPTIONS: SelectOption<AccidentSeverity>[] = [
  { label: "Minor", value: "minor" },
  { label: "Moderate", value: "moderate" },
  { label: "Severe", value: "severe" },
  { label: "Fatal", value: "fatal" },
];

export function getAccidentTypeLabel(type: AccidentType | undefined): string {
  return ACCIDENT_TYPE_OPTIONS.find((o) => o.value === type)?.label ?? type ?? "Unknown";
}

export function getAccidentSeverityLabel(severity: AccidentSeverity | undefined): string {
  return (
    ACCIDENT_SEVERITY_OPTIONS.find((o) => o.value === severity)?.label ?? severity ?? "Unknown"
  );
}
