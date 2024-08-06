import {SalaryType} from '@core/enums';

export interface CreateEmployeeCommand {
  userId: string;
  role?: string;
  salary: number;
  salaryType: SalaryType;
}
