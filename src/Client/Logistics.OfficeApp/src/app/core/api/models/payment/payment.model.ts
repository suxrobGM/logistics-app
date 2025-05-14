import {AddressDto} from "../address.model";
import {MoneyDto} from "../money.model";
import {PaymentStatus} from "./enums";

//import {PaymentMethodDto} from "./payment-method.model";

export interface PaymentDto {
  id: string;
  createdDate: Date;
  methodId: string;
  amount: MoneyDto;
  status: PaymentStatus;
  billingAddress: AddressDto;
  description?: string;
}
