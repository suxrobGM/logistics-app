import {SalaryType} from '@core/enums';

export interface CreateEmployee {
  userId: string;
  role?: string;
  salary: number;
  salaryType: SalaryType;
}
