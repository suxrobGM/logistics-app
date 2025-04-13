import {AddressDto} from "../address.dto";
import {
  CardFundingType,
  PaymentMethodType,
  UsBankAccountHolderType,
  UsBankAccountType,
} from "./enums";

export interface CreatePaymentMethodCommand {
  tenantId: string;
  type: PaymentMethodType;
  billingAddress: AddressDto;

  // Card-specific
  cardHolderName?: string;
  cardBrand?: string;
  cardNumber?: string;
  cvc?: string;
  expMonth?: number;
  expYear?: number;
  fundingType?: CardFundingType;

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
