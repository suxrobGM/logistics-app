import {AddressDto} from "../address.model";
import {PaymentFor, PaymentMethodType, PaymentStatus} from "./enums";

export interface PaymentDto {
  id: string;
  createdDate: string;
  paymentDate?: string;
  method?: PaymentMethodType;
  amount: number;
  status: PaymentStatus;
  paymentFor: PaymentFor;
  billingAddress?: AddressDto;
  notes?: string;
}
