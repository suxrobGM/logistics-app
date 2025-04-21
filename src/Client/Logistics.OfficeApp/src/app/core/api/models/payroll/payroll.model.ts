import {EmployeeDto} from "../employee/employee.model";
import {PaymentDto} from "../payment/payment.model";

export interface PayrollDto {
  id: string;
  startDate: string;
  endDate: string;
  payment: PaymentDto;
  employee: EmployeeDto;
}
