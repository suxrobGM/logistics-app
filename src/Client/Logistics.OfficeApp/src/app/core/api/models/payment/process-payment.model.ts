import {AddressDto} from "../address.model";
import {PaymentMethodType} from "./enums";

export interface ProcessPaymentCommand {
  paymentId: string;
  paymentMethod: PaymentMethodType;
  cardholderName?: string;
  cardNumber?: string;
  cardExpirationDate?: string;
  cardCvc?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankRoutingNumber?: string;
  billingAddress: AddressDto;
}
