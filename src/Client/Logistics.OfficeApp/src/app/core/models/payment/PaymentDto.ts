import {PaymentFor, PaymentMethod, PaymentStatus} from '@core/enums';
import {AddressDto} from '../AddressDto';


export interface PaymentDto {
  id: string;
  createdDate: string;
  paymentDate?: string;
  method?: PaymentMethod;
  amount: number;
  status: PaymentStatus;
  paymentFor: PaymentFor;
  billingAddress?: AddressDto;
  comment?: string;
}
