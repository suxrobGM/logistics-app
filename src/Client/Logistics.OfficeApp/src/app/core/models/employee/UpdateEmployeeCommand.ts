import {SalaryType} from '@/core/enums';

export interface UpdateEmployeeCommand {
  userId: string;
  role?: string;
  salary?: number;
  salaryType?: SalaryType;
}
