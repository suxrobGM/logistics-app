import {EmployeeDto} from "../employee/employee.dto";
import {PaymentDto} from "../payment/payment.dto";

export interface PayrollDto {
  id: string;
  startDate: string;
  endDate: string;
  payment: PaymentDto;
  employee: EmployeeDto;
}
