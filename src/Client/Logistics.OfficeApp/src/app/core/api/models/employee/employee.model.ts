import { SalaryType } from "./enums";
import { RoleDto } from "./role.model";

export interface EmployeeDto {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  roles: RoleDto[];
  joinedDate: Date;
  truckNumber?: string;
  truckId?: string;
  salary: number;
  salaryType: SalaryType;
}
