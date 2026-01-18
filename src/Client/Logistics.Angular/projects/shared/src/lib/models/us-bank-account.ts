import type { UsBankAccountHolderType, UsBankAccountType } from "../api/generated/models";

export interface UsBankAccount {
  accountNumber: string;
  accountHolderName: string;
  bankName: string;
  routingNumber: string;
  accountHolderType: UsBankAccountHolderType;
  accountType: UsBankAccountType;
}
