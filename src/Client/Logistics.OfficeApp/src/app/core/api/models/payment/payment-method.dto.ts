import {AddressDto} from "../address.dto";
import {PaymentMethodType, UsBankAccountHolderType, UsBankAccountType} from "./enums";

export interface PaymentMethodDto {
  id: string;
  type: PaymentMethodType;
  isDefault: boolean;
  billingAddress: AddressDto;

  // Card-specific
  cardHolderName?: string;
  cardNumber?: string;
  cvc?: string;
  expMonth?: number;
  expYear?: number;

  // US Bank account-specific
  accountNumber?: string;
  accountHolderName?: string;
  bankName?: string;
  routingNumber?: string;
  accountHolderType?: UsBankAccountHolderType;
  accountType?: UsBankAccountType;

  // International Bank Account
  swiftCode?: string;
}
