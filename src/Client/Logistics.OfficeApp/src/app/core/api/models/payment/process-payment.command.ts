import {PaymentMethodType} from "@/core/enums";
import {AddressDto} from "../address.dto";

export interface ProcessPaymentCommand {
  paymentId: string;
  paymentMethod: PaymentMethodType;
  cardholderName?: string;
  cardNumber?: string;
  cardExpirationDate?: string;
  cardCvv?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankRoutingNumber?: string;
  billingAddress: AddressDto;
}
