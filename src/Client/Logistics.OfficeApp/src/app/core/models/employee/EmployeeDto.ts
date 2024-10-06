import {SalaryType} from "@/core/enums";
import {RoleDto} from "./RoleDto";

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
