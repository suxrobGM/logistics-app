import {PaymentFor, PaymentMethod, PaymentStatus} from '@core/enums';
import {Address} from '../Address_';


export interface Payment {
  id: string;
  createdDate: string;
  paymentDate?: string;
  method?: PaymentMethod;
  amount: number;
  status: PaymentStatus;
  paymentFor: PaymentFor;
  billingAddress?: Address;
  comment?: string;
}
