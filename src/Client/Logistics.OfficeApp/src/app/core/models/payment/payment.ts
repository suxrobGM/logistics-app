import {PaymentFor, PaymentMethod, PaymentStatus} from '@core/enums';


export interface Payment {
  id: string;
  createdDate: string;
  paymentDate?: string;
  method?: PaymentMethod;
  amount: number;
  status: PaymentStatus;
  paymentFor: PaymentFor;
  comment?: string;
}
