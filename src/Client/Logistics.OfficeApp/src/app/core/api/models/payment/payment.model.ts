import {AddressDto} from "../address.dto";
import {MoneyDto} from "../money.dto";
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
