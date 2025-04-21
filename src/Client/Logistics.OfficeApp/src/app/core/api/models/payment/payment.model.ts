import {PaymentFor, PaymentMethodType, PaymentStatus} from "@/core/enums";
import {AddressDto} from "../address.model";

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
