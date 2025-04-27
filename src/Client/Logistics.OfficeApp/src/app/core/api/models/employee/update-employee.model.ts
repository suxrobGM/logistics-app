import {SalaryType} from "./enums";

export interface UpdateEmployeeCommand {
  userId: string;
  role?: string;
  salary?: number;
  salaryType?: SalaryType;
}
