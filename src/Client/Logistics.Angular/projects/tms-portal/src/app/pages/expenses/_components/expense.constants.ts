import type {
  CompanyExpenseCategory,
  ExpenseDto,
  ExpenseType,
  TruckExpenseCategory,
  VolumeUnit,
} from "@logistics/shared/api";

export interface SelectOption<T extends string = string> {
  label: string;
  value: T;
}

export const COMPANY_CATEGORIES: SelectOption<CompanyExpenseCategory>[] = [
  { label: "Office", value: "office" },
  { label: "Software", value: "software" },
  { label: "Insurance", value: "insurance" },
  { label: "Legal", value: "legal" },
  { label: "Travel", value: "travel" },
  { label: "Other", value: "other" },
];

export const TRUCK_CATEGORIES: SelectOption<TruckExpenseCategory>[] = [
  { label: "Fuel", value: "fuel" },
  { label: "Maintenance", value: "maintenance" },
  { label: "Tires", value: "tires" },
  { label: "Registration", value: "registration" },
  { label: "Toll", value: "toll" },
  { label: "Parking", value: "parking" },
  { label: "Other", value: "other" },
];

export const VOLUME_UNIT_OPTIONS: SelectOption<VolumeUnit>[] = [
  { label: "Gallons", value: "gallons" },
  { label: "Liters", value: "liters" },
];

export function getExpenseTypeLabel(type: ExpenseType | string | undefined): string {
  switch (type) {
    case "company":
      return "Company Expense";
    case "truck":
      return "Truck Expense";
    case "body_shop":
      return "Body Shop Expense";
    default:
      return "Expense";
  }
}

export function getCategoryLabel(expense: ExpenseDto): string {
  return expense.companyCategory ?? expense.truckCategory ?? "N/A";
}

export function toShortVolumeUnit(unit: VolumeUnit | undefined): "gal" | "L" | undefined {
  if (unit === "liters") return "L";
  if (unit === "gallons") return "gal";
  return undefined;
}
