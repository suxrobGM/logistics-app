import { SalaryType } from "./enums";

export interface CreateEmployeeCommand {
  userId: string;
  role?: string;
  salary: number;
  salaryType: SalaryType;
}
