import {PaymentFor} from './paymentFor';
import {PaymentMethod} from './paymentMethod';
import {PaymentStatus} from './paymentStatus';


export interface Payment {
  createdDate: string;
  paymentDate?: string;
  method: PaymentMethod;
  amount: number;
  status: PaymentStatus;
  paymentFor: PaymentFor;
  comment?: string;
}
