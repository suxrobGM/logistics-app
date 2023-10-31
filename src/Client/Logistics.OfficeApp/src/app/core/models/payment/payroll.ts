import {Employee} from '../employee/employee';
import {Payment} from './payment';


export interface Payroll {
  id: string;
  startDate: string;
  endDate: string;
  payment: Payment;
  employee: Employee; 
}
