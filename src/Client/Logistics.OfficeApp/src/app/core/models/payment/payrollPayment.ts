import {Employee} from '../employee/employee';
import {Payment} from './payment';


export interface PayrollPayment {
  id: string;
  startDate: string;
  endDate: string;
  payment: Payment;
  employee?: Employee; 
}
