import {SelectOption} from "@/shared/models";

export enum SalaryType {
  None = "none",
  Monthly = "monthly",
  Weekly = "weekly",
  ShareOfGross = "share_of_gross",
  Hourly = "hourly",
  RatePerMile = "rate_per_mile",
  RatePerKilometer = "rate_per_kilometer",
}

export const salaryTypeOptions: SelectOption<SalaryType>[] = [
  {label: "None", value: SalaryType.None},
  {label: "Monthly", value: SalaryType.Monthly},
  {label: "Weekly", value: SalaryType.Weekly},
  {label: "Share of Gross", value: SalaryType.ShareOfGross},
  {label: "Hourly", value: SalaryType.Hourly},
  {label: "Rate Per Mile", value: SalaryType.RatePerMile},
  {label: "Rate Per Kilometer", value: SalaryType.RatePerKilometer},
] as const;
