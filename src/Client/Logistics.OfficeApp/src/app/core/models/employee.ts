import {SalaryType} from '@core/enums';
import {Role} from './role';

export interface Employee {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  roles?: Role[];
  joinedDate: Date;
  truckNumber?: string;
  truckId?: string;
  salary?: number;
  salaryType?: SalaryType;
}
