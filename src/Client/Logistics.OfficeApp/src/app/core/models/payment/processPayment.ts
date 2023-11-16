import {PaymentMethod} from '@core/enums';
import {Address} from '../address';


export interface ProcessPayment {
  paymentId: string;
  paymentMethod: PaymentMethod;
  cardholderName?: string;
  cardNumber?: string;
  cardExpirationDate?: string;
  cardCvv?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankRoutingNumber?: string;
  billingAddress: Address;
}
