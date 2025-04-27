import {SelectOption} from "@/core/types";

export enum SalaryType {
  None = "none",
  Monthly = "monthly",
  Weekly = "weekly",
  ShareOfGross = "share_of_gross",
}

export const salaryTypeOptions: SelectOption<SalaryType>[] = [
  {label: "None", value: SalaryType.None},
  {label: "Monthly", value: SalaryType.Monthly},
  {label: "Weekly", value: SalaryType.Weekly},
  {label: "Share of Gross", value: SalaryType.ShareOfGross},
] as const;
