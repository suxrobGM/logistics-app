import {SalaryType} from '@core/enums';

export interface UpdateEmployee {
  userId: string;
  role?: string;
  salary?: number;
  salaryType?: SalaryType;
}
