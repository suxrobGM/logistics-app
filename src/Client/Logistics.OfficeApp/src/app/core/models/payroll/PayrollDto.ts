import {EmployeeDto} from "../employee/EmployeeDto";
import {PaymentDto} from "../payment/PaymentDto";

export interface PayrollDto {
  id: string;
  startDate: string;
  endDate: string;
  payment: PaymentDto;
  employee: EmployeeDto;
}
