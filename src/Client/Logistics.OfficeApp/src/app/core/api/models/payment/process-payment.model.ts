import {PaymentMethodType} from "@/core/enums";
import {AddressDto} from "../address.model";

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
