import {AddressDto} from "../address.dto";
import {
  PaymentMethodType,
  PaymentMethodVerificationStatus,
  UsBankAccountHolderType,
  UsBankAccountType,
} from "./enums";

export interface CreatePaymentMethodCommand {
  type: PaymentMethodType;
  billingAddress: AddressDto;
  verificationStatus?: PaymentMethodVerificationStatus;
  stripePaymentMethodId?: string;

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
  verificationUrl?: string;

  // International Bank Account
  swiftCode?: string;
}
