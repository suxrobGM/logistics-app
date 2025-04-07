import {PaymentMethodType, PaymentStatus} from "@/core/enums";
import {AddressDto} from "../address.dto";

export interface SubscriptionPaymentDto {
  id: string;
  amount: number;
  status: PaymentStatus;
  method: PaymentMethodType;
  paymentDate: Date;
  billingAddress: AddressDto;
}
